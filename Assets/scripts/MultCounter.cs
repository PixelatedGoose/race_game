using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MultCounter : MonoBehaviour
{
    [Header("Sprite Settings")]
    public Sprite[] numberSprites;
    
    [Header("UI References")]
    public GameObject MultAnimCrap;
    public Image displayImage;
    public TextMeshProUGUI multiplierText; // NEW: Text to show the multiplier number
    
    [Header("Animation Settings")]
    [SerializeField] private float highQualityLoopSpeed = 0.15f;
    [SerializeField] private float qualityThresholdForLoop = 0.75f; // Increased from 0.6 to 0.75 - starts loop at ~7x mult
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private float qualityCheckInterval = 0.2f;
    
    [Header("Sprite Indices")]
    [SerializeField] private int idleSpriteIndex = 0;
    [SerializeField] private int firstAnimFrameIndex = 1;
    [SerializeField] private int lastAnimFrameIndex = 22;
    [SerializeField] private int loopStartIndex = 20;  // Changed from 10 to 20 - loop the LAST 3 sprites
    [SerializeField] private int loopEndIndex = 22;    // Changed from 12 to 22
    
    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownAnimationSpeed = 0.1f;
    
    private bool isLooping = false;
    private bool isDrifting = false;
    private bool isCoolingDown = false;
    private float loopTimer = 0f;
    private float updateTimer = 0f;
    private float qualityCheckTimer = 0f;
    private float cooldownTimer = 0f;
    private int currentLoopFrame = 10;
    private int currentCooldownFrame = 0;

    void Start()
    {
        if (displayImage != null && numberSprites.Length > 0)
        {
            displayImage.sprite = numberSprites[idleSpriteIndex];
        }
        
        UpdateMultiplierText(1f); // Show x1.0 at start
        
        Debug.Log($"[MultCounter] Initialized with {numberSprites.Length} sprites");
    }

    void Update()
    {
        if (isCoolingDown)
        {
            UpdateCooldownAnimation();
            return;
        }

        if (!isDrifting)
        {
            if (displayImage != null && numberSprites.Length > 0)
            {
                if (displayImage.sprite != numberSprites[idleSpriteIndex])
                {
                    displayImage.sprite = numberSprites[idleSpriteIndex];
                }
            }
            UpdateMultiplierText(1f);
            return;
        }

        // Check quality first
        CheckQualityForLoop();

        // Update text continuously during drift
        if (ScoreManager.instance != null)
        {
            UpdateMultiplierText(ScoreManager.instance.CurrentDriftMultiplier);
        }

        // Handle animations
        if (isLooping)
        {
            UpdateHighQualityLoop();
        }
        else
        {
            UpdateNormalDrift();
        }
    }

    void StartHighQualityLoop()
    {
        // Safety check - only start if multiplier is actually >= 7x
        if (ScoreManager.instance != null && ScoreManager.instance.CurrentDriftMultiplier < 7f)
        {
            Debug.LogWarning($"[MultCounter] Prevented loop start - multiplier too low: {ScoreManager.instance.CurrentDriftMultiplier:F2}");
            return;
        }
        
        int currentSpriteIndex = GetCurrentSpriteIndex();
        
        isLooping = true;
        loopTimer = 0f;
        updateTimer = 0f;
        
        currentLoopFrame = loopStartIndex;
        
        Debug.Log($"[MultCounter] *** HIGH QUALITY LOOP STARTED *** Current sprite: {currentSpriteIndex}, Multiplier: {ScoreManager.instance.CurrentDriftMultiplier:F2}, Starting loop at frame: {currentLoopFrame}");
        
        DisplayLoopFrame(currentLoopFrame);
    }

    int GetCurrentSpriteIndex()
    {
        if (displayImage == null || displayImage.sprite == null) return 0;
        
        for (int i = 0; i < numberSprites.Length; i++)
        {
            if (numberSprites[i] == displayImage.sprite)
            {
                return i;
            }
        }
        return 0;
    }

    void StopHighQualityLoop()
    {
        if (!isLooping) return;
        
        Debug.Log("[MultCounter] *** High quality loop STOPPED ***");
        
        isLooping = false;
        loopTimer = 0f;
        updateTimer = 0f;
        
        // DON'T call DisplayMultiplierSprite here - let UpdateNormalDrift handle it
    }

    void CheckQualityForLoop()
    {
        qualityCheckTimer += Time.deltaTime;
        
        if (qualityCheckTimer >= qualityCheckInterval)
        {
            qualityCheckTimer = 0f;
            
            if (ScoreManager.instance != null)
            {
                float currentMultiplier = ScoreManager.instance.CurrentDriftMultiplier;
                float driftTime = ScoreManager.instance.GetDriftTime();
                float sharpness = ScoreManager.instance.GetCurrentSharpness();
                
                // Use ONLY multiplier for loop check - simple and direct
                // Loop starts at 7x multiplier
                bool shouldLoop = currentMultiplier >= 7f;
                
                Debug.Log($"[MultCounter] Mult: {currentMultiplier:F2}, Time: {driftTime:F2}s, Sharpness: {sharpness:F2}, ShouldLoop: {shouldLoop}, IsLooping: {isLooping}");
                
                // Hysteresis - start at 7x, stop at 6.5x
                if (currentMultiplier >= 7f && !isLooping)
                {
                    StartHighQualityLoop();
                }
                else if (currentMultiplier < 6.5f && isLooping)
                {
                    StopHighQualityLoop();
                }
            }
        }
    }

    void UpdateNormalDrift()
    {
        // Don't update if we're looping
        if (isLooping) return;
        
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            
            if (ScoreManager.instance != null)
            {
                float currentMultiplier = ScoreManager.instance.CurrentDriftMultiplier;
                DisplayMultiplierSprite(currentMultiplier);
            }
        }
    }

    void UpdateCooldownAnimation()
    {
        cooldownTimer += Time.deltaTime;
        
        if (cooldownTimer >= cooldownAnimationSpeed)
        {
            cooldownTimer = 0f;
            
            currentCooldownFrame--;
            
            if (currentCooldownFrame <= idleSpriteIndex)
            {
                currentCooldownFrame = idleSpriteIndex;
                isCoolingDown = false;
                
                if (displayImage != null && numberSprites.Length > 0)
                {
                    displayImage.sprite = numberSprites[idleSpriteIndex];
                }
                
                UpdateMultiplierText(1f); // Reset to x1.0
                
                Debug.Log("[MultCounter] Cooldown animation complete - back to idle");
            }
            else
            {
                if (displayImage != null && currentCooldownFrame >= 0 && currentCooldownFrame < numberSprites.Length)
                {
                    displayImage.sprite = numberSprites[currentCooldownFrame];
                    Debug.Log($"[MultCounter] Cooldown frame: {currentCooldownFrame}");
                }
            }
        }
    }

    // NEW: Update the text with current multiplier
    void UpdateMultiplierText(float multiplier)
    {
        if (multiplierText == null) return;
        
        multiplierText.text = $"{multiplier:F1}x";
        
        // Optional: Color based on multiplier
        if (multiplier >= 7f)
            multiplierText.color = Color.red;
        else if (multiplier >= 4f)
            multiplierText.color = new Color(1f, 0.5f, 0f); // Orange
        else if (multiplier >= 2f)
            multiplierText.color = Color.yellow;
        else
            multiplierText.color = Color.white;
    }

    public void ShowMultiplier(float multiplier, float quality)
    {
        isDrifting = true;
        isCoolingDown = false;
        qualityCheckTimer = 0f;
        
        Debug.Log($"[MultCounter] ShowMultiplier called - Multiplier: {multiplier:F2}, Quality: {quality:F2} - IGNORING QUALITY");
        
        UpdateMultiplierText(multiplier);

        // COMPLETELY IGNORE quality parameter - never start loop here
        DisplayMultiplierSprite(multiplier);
        
        // Make absolutely sure loop doesn't start
        if (isLooping)
        {
            StopHighQualityLoop();
        }
    }

    public void HideMultiplier()
    {
        Debug.Log("[MultCounter] HideMultiplier called - starting cooldown animation");
        
        isDrifting = false;
        
        if (isLooping)
        {
            StopHighQualityLoop();
        }
        
        isCoolingDown = true;
        cooldownTimer = 0f;
        
        // Get current sprite to start cooldown from
        currentCooldownFrame = GetCurrentSpriteIndex();
        Debug.Log($"[MultCounter] Starting cooldown from sprite index {currentCooldownFrame}");
    }

    void DisplayMultiplierSprite(float multiplier)
    {
        if (displayImage == null)
        {
            Debug.LogError("[MultCounter] displayImage is NULL!");
            return;
        }
        
        int spriteIndex = GetSpriteIndexForMultiplier(multiplier);
        
        if (spriteIndex >= 0 && spriteIndex < numberSprites.Length)
        {
            displayImage.sprite = numberSprites[spriteIndex];
            Debug.Log($"[MultCounter] Displaying sprite index {spriteIndex} for multiplier {multiplier:F2}");
        }
        else
        {
            Debug.LogError($"[MultCounter] Sprite index {spriteIndex} out of range (array size: {numberSprites.Length})");
        }
    }

    int GetSpriteIndexForMultiplier(float multiplier)
    {
        if (multiplier <= 1.0f)
        {
            return idleSpriteIndex;
        }
        
        // Cap at sprite 19 when multiplier reaches 7x (leave 20-22 for loop)
        if (multiplier >= 7f)
        {
            return loopStartIndex - 1; // Sprite 19 - just before loop starts
        }
        
        // Map multiplier 1.0-7.0 to sprite indices 1-19 (stops before loop sprites)
        float normalizedMult = Mathf.Clamp01((multiplier - 1f) / 6f);
        int frameRange = (loopStartIndex - 1) - firstAnimFrameIndex; // Use 19 as max, not 22
        int frameIndex = Mathf.RoundToInt(normalizedMult * frameRange);
        
        int finalIndex = firstAnimFrameIndex + frameIndex;
        
        Debug.Log($"[MultCounter] GetSpriteIndex - Mult: {multiplier:F2}, Normalized: {normalizedMult:F2}, Frame: {frameIndex}, Final: {finalIndex}");
        
        return finalIndex;
    }

    void UpdateHighQualityLoop()
    {
        loopTimer += Time.deltaTime;
        
        if (loopTimer >= highQualityLoopSpeed)
        {
            loopTimer = 0f;
            
            currentLoopFrame++;
            if (currentLoopFrame > loopEndIndex)
            {
                currentLoopFrame = loopStartIndex;
            }
            
            DisplayLoopFrame(currentLoopFrame);
            Debug.Log($"[MultCounter] LOOPING - Current frame: {currentLoopFrame}");
        }
    }

    void DisplayLoopFrame(int frameIndex)
    {
        if (displayImage == null)
        {
            Debug.LogError("[MultCounter] displayImage is NULL in loop!");
            return;
        }
        
        if (frameIndex >= 0 && frameIndex < numberSprites.Length)
        {
            displayImage.sprite = numberSprites[frameIndex];
            Debug.Log($"[MultCounter] LOOP FRAME: Displaying sprite index {frameIndex}");
        }
        else
        {
            Debug.LogError($"[MultCounter] Loop frame {frameIndex} out of range!");
        }
    }
}