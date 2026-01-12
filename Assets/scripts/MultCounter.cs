using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultCounter : MonoBehaviour
{
    public Sprite[] numberSprites;
    public Image displayImage;
    public TextMeshProUGUI multiplierText;
    
    private bool isDrifting, isLooping, isCoolingDown, useFullCooldown;
    private float loopTimer, updateTimer, qualityTimer, cooldownTimer;
    private int currentFrame;
    private float cooldownMultiplier;

    void Start()
    {
        displayImage.sprite = numberSprites[0];
        UpdateMultiplierText(1f);
    }

    void Update()
    {
        if (isCoolingDown)
        {
            if ((cooldownTimer += Time.deltaTime) >= 0.1f)
            {
                cooldownTimer = 0;
                currentFrame += useFullCooldown ? 1 : -1;
                
                if ((useFullCooldown && currentFrame > 22) || (!useFullCooldown && currentFrame < 0))
                {
                    isCoolingDown = false;
                    displayImage.sprite = numberSprites[0];
                }
                else displayImage.sprite = numberSprites[currentFrame];
            }
            return;
        }

        if (!isDrifting) { displayImage.sprite = numberSprites[0]; UpdateMultiplierText(1f); return; }

        float mult = ScoreManager.instance.CurrentDriftMultiplier;
        UpdateMultiplierText(mult);
        // this is bcs for some fucking reason if not this the quality drift loop starts at 4 instead of 7
        if ((qualityTimer += Time.deltaTime) >= 0.2f)
        {
            qualityTimer = 0;
            if (mult >= 7f && !isLooping) { isLooping = true; currentFrame = 10; loopTimer = 0; }
            else if (mult < 6.5f && isLooping) isLooping = false;
        }

        if (isLooping && mult >= 7f)
        {
            if ((loopTimer += Time.deltaTime) >= 0.15f)
            {
                loopTimer = 0;
                currentFrame = currentFrame >= 12 ? 10 : currentFrame + 1;
                displayImage.sprite = numberSprites[currentFrame];
            }
        }
        else if ((updateTimer += Time.deltaTime) >= 0.1f)
        {
            updateTimer = 0;
            displayImage.sprite = numberSprites[mult <= 1f ? 0 : mult >= 7f ? 9 : Mathf.RoundToInt(((mult - 1f) / 6f) * 9)];
        }
    }

    public void UpdateMultiplierText(float mult)
    {
        multiplierText.text = $"{mult:F1}x";
        multiplierText.color = mult >= 7f ? Color.red : mult >= 4f ? new Color(1f, 0.5f, 0f) : mult >= 2f ? Color.yellow : Color.white;
    }

    public void StartMultiplier(float multiplier, float quality)
    {
        isDrifting = true;
        isCoolingDown = false;
        UpdateMultiplierText(multiplier);
    }

    // its the great reset of 2026 
    public void ResetMultiplier()
    {
        isDrifting = isLooping = false;
        isCoolingDown = true;
        //checks what sprite is showing when drift ends and from there goes either from 9-0 or 13-22 sprite to show cooldown
        UpdateMultiplierText(1f);
        for (int i = 0; i < numberSprites.Length; i++)
            if (numberSprites[i] == displayImage.sprite)
            {
                useFullCooldown = i >= 10 && i <= 12;
                currentFrame = useFullCooldown ? 13 : i;
                break;
            }
    }
}