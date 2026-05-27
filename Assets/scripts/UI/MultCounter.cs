using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class MultCounter : MonoBehaviour
{
    public Sprite[] numberSprites;
    public Image displayImage;
    public TextMeshProUGUI multiplierText;
    private PlayerCarController playerScript; 
    
    private bool isDrifting, useFullCooldown;
    private int currentFrame;
    private readonly float twoThirds = 2f / 3f;
    private float lastMultiplier;
    private readonly KeyValuePair<int, Color>[] colors = new KeyValuePair<int, Color>[4]
    {
        new(1, Color.white),
        new(3, Color.yellow),
        new(4, Color.orange),
        new(7, Color.red)
    };
    public void Awake()
    {
        playerScript = GameManager.CurrentCar.GetComponentInChildren<PlayerCarController>();
    }

    private void OnEnable()
    {
        playerScript.Controls.CarControls.Drift.performed += Activate;
        playerScript.Controls.CarControls.Drift.canceled += Disable;
    }

    private void OnDisable()
    {
        playerScript.Controls.CarControls.Drift.canceled -= Disable;
        playerScript.Controls.CarControls.Drift.performed -= Activate;
    }

    public virtual void Activate(InputAction.CallbackContext ctx)
    {
        isDrifting = true;
        StartCoroutine(MultiplierTimer());
    }

    public virtual void Disable(InputAction.CallbackContext ctx)
    {
        isDrifting = false;
        StopCoroutine(MultiplierTimer());
    }

    void Start()
    {
        displayImage.sprite = numberSprites[0];
        UpdateMultiplierText(1f);
    }

    void Update()
    {
        // Loop();
    }

    private IEnumerator CooldownRoutine()
    {
        while (!isDrifting)
        {
            yield return new WaitForSeconds(0.1f);

            currentFrame += useFullCooldown ? 1 : -1;
            bool finished = useFullCooldown ? currentFrame > 22 : currentFrame < 0;

            currentFrame = Mathf.Clamp(currentFrame, 0, numberSprites.Length - 1);
            displayImage.sprite = finished ? numberSprites[0] : numberSprites[currentFrame];
        }
    }

    // public void Loop()
    // {
        
    //     if (isCoolingDown) return;
        
    //     if (!isDrifting)
    //     {
    //         displayImage.sprite = numberSprites[0];
    //         UpdateMultiplierText(1f);
    //         return;
    //     }

    //     float mult = mult;
    //     UpdateMultiplierText(Mathf.RoundToInt(mult));

    //     qualityTimer += Time.deltaTime;
    //     if (qualityTimer >= 0.2f)
    //     {
    //         qualityTimer = 0f;
    //         bool startLoop = mult >= 7f && !isLooping;
    //         bool stopLoop = mult < 6.5f && isLooping;
    //         isLooping = startLoop ? true : stopLoop ? false : isLooping;
    //         if (startLoop) { currentFrame = 10; loopTimer = 0f; }
    //     }

    //     if (isLooping && mult >= 7f)
    //     {
    //         loopTimer += Time.deltaTime;
    //         if (loopTimer >= 0.15f)
    //         {
    //             loopTimer = 0f;
    //             currentFrame = currentFrame >= 12 ? 10 : currentFrame + 1;
    //             displayImage.sprite = numberSprites[currentFrame];
    //         }
    //         return;
    //     }

    //     updateTimer += Time.deltaTime;
    //     if (updateTimer < 0.1f) return;
    //     updateTimer = 0f;
    //     int idx = Mathf.RoundToInt(mult * twoThirds);
    //     displayImage.sprite = numberSprites[idx];
    // }

    public void UpdateMultiplierText(float mult)
    {
        multiplierText.text = $"{mult}";
        multiplierText.color = colors.Where(pair => mult >= pair.Key).Select(pair => pair.Value).DefaultIfEmpty(Color.white).Last();
        // mult >= 7f ? Color.red : mult >= 4f ? Color.orange : mult >= 2f ? Color.yellow : Color.white;
    }

    // its the great reset of 2026 
    public void ResetMultiplier()
    {
        UpdateMultiplierText(1f);

        int idx = System.Array.FindIndex(numberSprites, s => s == displayImage.sprite);
        idx = Mathf.Clamp(idx, 0, numberSprites.Length - 1);
        useFullCooldown = idx >= 10 && idx <= 12;
        currentFrame = useFullCooldown ? 13 : idx;
        displayImage.sprite = numberSprites[Mathf.Clamp(currentFrame, 0, numberSprites.Length - 1)];

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(CooldownRoutine());
        }
    }



    private IEnumerator MultiplierTimer()
    {
        float loopTimer = 0f;
        int loopFrame = 10;

        while (isDrifting)
        {
            float mult = ScoreManager.instance.CurrentDriftMultiplier;
            
            if (mult >= 10f)
            {
                if (mult != lastMultiplier)
                {
                    UpdateMultiplierText(Mathf.RoundToInt(mult));
                    lastMultiplier = mult;
                }

                loopTimer += Time.deltaTime;
                if (loopTimer >= 0.15f)
                {
                    loopTimer = 0f;
                    loopFrame = (loopFrame == 10) ? 11 : 10;
                    displayImage.sprite = numberSprites[loopFrame];
                }
            }
            else if (mult != lastMultiplier)
            {
                UpdateMultiplierText(Mathf.RoundToInt(mult));
                int idx = Mathf.Clamp(Mathf.RoundToInt(mult * twoThirds), 0, numberSprites.Length - 1);
                displayImage.sprite = numberSprites[idx];
                lastMultiplier = mult;
            }

            yield return null;
        }

        ResetMultiplier();
        yield break;
    }
}