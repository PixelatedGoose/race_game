using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class ButtonInstructions : MonoBehaviour
{
    private TextMeshProUGUI InstructionText;
    private Waitbeforestart WaitBeforeStart;

    private CarInputActions InputActions;
    private List<TutorialStep> TutorialSteps;
    private Dictionary<string, InputAction> CachedActions = new();
    private int currentStep = 0;
    private bool tutorialComplete;
    private InputDevice lastUsedDevice;

    public event Action OnTutorialComplete;

    [Serializable]
    public class TutorialStep
    {
        public string Instruction;
        public string ActionName;
        public string CompositePart;
        public bool RequiresHold;
        public float HoldTime = 0.3f;
        public Vector2 RequiredDirection;
        public string[] RequiredComboActions; // for multiple button combos

        [NonSerialized] public float HoldTimer; // per-step timer
    }

    private void Awake()
    {
        Time.timeScale = 0f;

        if (InstructionText == null)
            InstructionText = GetComponent<TextMeshProUGUI>();

        WaitBeforeStart = FindFirstObjectByType<Waitbeforestart>();
        WaitBeforeStart.enabled = false;

        InputActions = new CarInputActions();

        TutorialSteps = new List<TutorialStep>()
        {
            new TutorialStep{ Instruction="Hold {button} to accelerate", ActionName="MoveForward", RequiresHold=true, HoldTime=2f },
            new TutorialStep{ Instruction="Hold {button} to brake", ActionName="Brake", RequiresHold=true, HoldTime=2f },
            new TutorialStep{ Instruction="Steer left with {button}", ActionName="Move", CompositePart="left", RequiredDirection=Vector2.left, HoldTime=0.3f },
            new TutorialStep{ Instruction="Steer right with {button}", ActionName="Move", CompositePart="right", RequiredDirection=Vector2.right, HoldTime=0.3f },
            new TutorialStep{ Instruction="Hold {button} to drift", ActionName="Drift", RequiresHold=true, HoldTime=2f },
            new TutorialStep{ Instruction="Hold {button} to use turbo", ActionName="Turbo", RequiresHold=true, HoldTime=2f },
            //sloppy? yes, will i fix it? no why wont i fix it? idk you tell me, most likely reason for me not fixing it? im lazy and tired
            new TutorialStep{ 
                Instruction="Hold {combo} to turbo while drifting", 
                RequiresHold=true, 
                HoldTime=2f, 
                RequiredComboActions=new string[]{"Drift", "Turbo"} 
            },
            new TutorialStep{ Instruction="Press {button} to respawn", ActionName="Respawn" }
        };
    }

    private void OnEnable()
    {
        InputActions.CarControls.Enable();

        foreach (var step in TutorialSteps)
        {
            if (!string.IsNullOrEmpty(step.ActionName))
            {
                var action = InputActions.asset.FindAction(step.ActionName);
                if (action != null)
                    CachedActions[step.ActionName] = action;
            }
        }

        StartStep();
    }

    private void OnDisable()
    {
        foreach (var step in TutorialSteps)
        {
            if (!string.IsNullOrEmpty(step.ActionName) && CachedActions.TryGetValue(step.ActionName, out var action))
                action.performed -= OnActionPerformed;
        }

        InputActions.Dispose();
    }

    private void Update()
    {
        if (currentStep >= TutorialSteps.Count) return;

        TrackLastUsedDevice();
        ShowStep();

        var step = TutorialSteps[currentStep];

        // Combo step
        if (step.RequiredComboActions != null && step.RequiredComboActions.Length > 0)
        {
            bool allPressed = true;
            foreach (var actName in step.RequiredComboActions)
            {
                if (!CachedActions.TryGetValue(actName, out var act) || !act.IsPressed())
                {
                    allPressed = false;
                    break;
                }
            }

            if (allPressed)
            {
                step.HoldTimer += Time.unscaledDeltaTime;
                if (step.HoldTimer >= step.HoldTime) AdvanceStep();
            }
            else step.HoldTimer = 0f;
            return;
        }

        // Directional hold
        if (step.RequiredDirection != Vector2.zero && CachedActions.TryGetValue(step.ActionName, out var dirAction))
        {
            Vector2 input = dirAction.ReadValue<Vector2>();
            if (input.sqrMagnitude >= 0.25f)
            {
                float alignment = Vector2.Dot(input.normalized, step.RequiredDirection.normalized);
                if (alignment >= 0.7f)
                {
                    step.HoldTimer += Time.unscaledDeltaTime;
                    if (step.HoldTimer >= step.HoldTime) AdvanceStep();
                }
                else step.HoldTimer = 0f;
            }
            else step.HoldTimer = 0f;
            return;
        }

        // Single button hold
        if (step.RequiresHold && !string.IsNullOrEmpty(step.ActionName) && CachedActions.TryGetValue(step.ActionName, out var holdAction))
        {
            if (holdAction.IsPressed())
            {
                step.HoldTimer += Time.unscaledDeltaTime;
                if (step.HoldTimer >= step.HoldTime) AdvanceStep();
            }
            else step.HoldTimer = 0f;
        }
    }

    private void TrackLastUsedDevice()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            lastUsedDevice = Keyboard.current;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            lastUsedDevice = Mouse.current;

        if (Gamepad.current != null)
        {
            foreach (var stick in new[] { Gamepad.current.leftStick, Gamepad.current.rightStick })
                if (stick.ReadValue() != Vector2.zero)
                    lastUsedDevice = Gamepad.current;

            foreach (var button in new[] { Gamepad.current.buttonSouth, Gamepad.current.buttonNorth,
                                           Gamepad.current.buttonEast, Gamepad.current.buttonWest })
                if (button.wasPressedThisFrame)
                    lastUsedDevice = Gamepad.current;
        }
    }

    private void StartStep()
    {
        if (currentStep >= TutorialSteps.Count) return;

        var step = TutorialSteps[currentStep];

        if (!string.IsNullOrEmpty(step.ActionName) && CachedActions.TryGetValue(step.ActionName, out var action))
        {
            InstructionText.text = step.Instruction.Replace("{button}", GetBindingDisplay(action, step.CompositePart));
            step.HoldTimer = 0f;
            action.performed -= OnActionPerformed;
            action.performed += OnActionPerformed;
        }
        else if (step.RequiredComboActions != null)
        {
            InstructionText.text = step.Instruction.Replace("{combo}", GetComboBindingDisplay(step.RequiredComboActions));
            step.HoldTimer = 0f;
        }
    }

    private void OnActionPerformed(InputAction.CallbackContext ctx)
    {
        var step = TutorialSteps[currentStep];

        // Only trigger immediate advance for non-hold, non-directional steps
        if (!step.RequiresHold && step.RequiredDirection == Vector2.zero && step.RequiredComboActions == null)
        {
            bool pressed = ctx.ReadValue<float>() > 0.5f;
            if (pressed) AdvanceStep();
        }
    }

    private void AdvanceStep()
    {
        currentStep++;
        if (currentStep >= TutorialSteps.Count)
        {
            CompleteTutorial();
            return;
        }
        StartStep();
    }

    private void CompleteTutorial()
    {
        tutorialComplete = true;
        InstructionText.text = "Ready to race!";
        if (WaitBeforeStart != null) WaitBeforeStart.enabled = true;
        Time.timeScale = 1f;
        OnTutorialComplete?.Invoke();
    }

    private void ShowStep()
    {
        if (currentStep >= TutorialSteps.Count) return;

        var step = TutorialSteps[currentStep];

        if (!string.IsNullOrEmpty(step.ActionName) && CachedActions.TryGetValue(step.ActionName, out var action))
        {
            InstructionText.text = step.Instruction.Replace("{button}", GetBindingDisplay(action, step.CompositePart));
        }
        else if (step.RequiredComboActions != null)
        {
            InstructionText.text = step.Instruction.Replace("{combo}", GetComboBindingDisplay(step.RequiredComboActions));
        }
    }

    private string GetBindingDisplay(InputAction action, string CompositePart = null)
    {
        if (action == null) return "";
        string deviceType = lastUsedDevice switch
        {
            Keyboard => "Keyboard",
            Gamepad => "Gamepad",
            _ => null
        };

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var Binding = action.bindings[i];

            if (!string.IsNullOrEmpty(CompositePart) && (!Binding.isPartOfComposite || !Binding.name.Equals(CompositePart, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (Binding.isPartOfComposite && string.IsNullOrEmpty(CompositePart))
                continue;

            // Skip bindings that don't match the device type
            if (deviceType != null && !Binding.path.Contains(deviceType, StringComparison.OrdinalIgnoreCase))
                continue;

            // Convert the binding path to a human-readable name
            return InputControlPath.ToHumanReadableString(
                Binding.effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            );
        }

        // fallback: show first binding for the action
        return InputControlPath.ToHumanReadableString(
            action.bindings[0].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
    }

    private string GetComboBindingDisplay(string[] ActionNames)
    {
        List<string> displays = new();
        foreach (var name in ActionNames)
        {
            if (CachedActions.TryGetValue(name, out var action))
                displays.Add(GetBindingDisplay(action));
        }
        return string.Join(" + ", displays);
    }
}