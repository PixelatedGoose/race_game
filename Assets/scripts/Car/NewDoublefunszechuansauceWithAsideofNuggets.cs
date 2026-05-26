using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(RacerScript))]
[RequireComponent(typeof(PlayerInput))]
public class NewDoublefunszechuansauceWithAsideofNuggets : BaseCarController
{
    public CarInputActions Controls { get; protected set; } = new CarInputActions();
    RacerScript racerScript;
    LogitechMovement LGM;
    private string CurrentControlScheme;
    PlayerInput PlayerInput;

    override protected void Awake()
    {
        CarRb = GetComponent<Rigidbody>();
        racerScript = GetComponent<RacerScript>();
        TurbeBar = GameManager.instance.CarUI.transform.Find("TurbeDisplay").GetComponentInChildren<Image>();
        TryGetComponent(out LGM);
        
        Controls.Enable();
        base.Awake();

        CarRb.centerOfMass = _CenterofMass;

        if (turbo != null)
        {
            Controls.CarControls.turbo.started += context => { turbo.Activate(); };
            Controls.CarControls.turbo.performed += context => { turbo.Stop(); };
        }
    }

    private void OnControlsChanged(PlayerInput input)
    {
        CurrentControlScheme = input.currentControlScheme;
        if (LGM != null) LGM.ReenableFromControlScheme(CurrentControlScheme);
    }

    void OnAnyActionTriggered(InputAction.CallbackContext ctx)
    {
        var control = ctx.action?.activeControl;
        if (control == null) return;

    }

    private void OnEnable()
    {
        // Add input event listeners
        Controls.Enable();
        PlayerInput = GetComponent<PlayerInput>();

        PlayerInput.onControlsChanged += OnControlsChanged;

        Controls.CarControls.Get().actionTriggered += OnAnyActionTriggered;

        Controls.CarControls.Move.performed += OnMovePerformed;
        Controls.CarControls.Move.canceled  += OnMoveCanceled;

        Controls.CarControls.Drift.performed   += OnDriftPerformed;
        Controls.CarControls.Drift.canceled    += OnDriftCanceled;
        Controls.CarControls.Brake.performed += OnBrakePerformed;
        Controls.CarControls.Brake.canceled  += OnBrakeCanceled;

        if (turbo != null)
        {
            Controls.CarControls.turbo.started += context => { turbo.Activate(); };
            Controls.CarControls.turbo.performed += context => { turbo.Stop(); };
        }
    }

    private void OnDisable()
    {
        // Remove input event listeners
        Controls.Disable();
        if (PlayerInput != null)
            PlayerInput.onControlsChanged -= OnControlsChanged;

        Controls.CarControls.Get().actionTriggered -= OnAnyActionTriggered;


        Controls.CarControls.Move.performed -= OnMovePerformed;
        Controls.CarControls.Move.canceled  -= OnMoveCanceled;
        Controls.CarControls.Drift.performed -= OnDriftPerformed;
        Controls.CarControls.Drift.canceled  -= OnDriftCanceled;
        Controls.CarControls.Brake.performed -= OnBrakePerformed;
        Controls.CarControls.Brake.canceled -= OnBrakeCanceled;

        if (turbo != null)
        {
            Controls.CarControls.turbo.started -= context => { turbo.Activate(); };
            Controls.CarControls.turbo.performed -= context => { turbo.Stop(); };
        }

        if (LGM != null) LGM.StopAllForceFeedback();
    }

    private void OnDestroy()
    {
        Controls.Disable();
        Controls.Dispose();

        if (LGM != null) LGM.StopAllForceFeedback();
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        MovementInputs = ctx.ReadValue<Vector2>();
        Steer();
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        MovementInputs = Vector2.zero;
        Steer();
    }

    override protected void Start()
    {
        base.Start();
    }

    //movement or anykind of input related will go here
    protected void Update()
    {
        MovementInputs = Controls.CarControls.Move.ReadValue<Vector2>();
        Animatewheels();

        Steer();
        CarMovement();
    }

    //physics related will go here
    override protected void FixedUpdate()
    {
        TurnSensitivity = CarRb.linearVelocity.magnitude / MaxSpeed * turnSensitivityRange + MaxTurnSensitivity;
        base.FixedUpdate();
        // HandleTurbo();
    }

    //que
//    protected void HandleTurbo()
//     {
//         if (!CanUseTurbo) return;
//         Turbe.TURBO(this);
//         TurbeMeter();
//     }


    //Arcade car style movement
    protected void CarMovement()
    {
        Vector3 flatForwardVelocity = Vector3.Lerp(
            CarRb.linearVelocity,
            MaxSpeed * Mathf.Abs(MovementInputs.y) * transform.forward, 
            Acceleration * Time.deltaTime
        );

        flatForwardVelocity.y = CarRb.linearVelocity.y;
        CarRb.linearVelocity = flatForwardVelocity;
    }

    void OnBrakePerformed(InputAction.CallbackContext ctx)
    {
        foreach (Wheel wheel in Wheels)
        {
            wheel.Brake(BrakeAcceleration);
        }
    }

    void OnBrakeCanceled(InputAction.CallbackContext ctx)
    {
        foreach (Wheel wheel in Wheels)
        {
            wheel.SetTorque(TargetTorque);
        }
    }

    void OnDriftPerformed(InputAction.CallbackContext ctx)
    {
        print("you're are drifting now");
    }

    void OnDriftCanceled(InputAction.CallbackContext ctx)
    {
        print("you stopped drifting");
    }
}