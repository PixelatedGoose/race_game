using UnityEngine;
using UnityEngine.UI;
using System;

public class ScoreManager : MonoBehaviour, IDataPersistence
{
    [Header("Base Score Settings")]
    [SerializeField] private float basePointsPerSecond = 1.5f;
    [SerializeField] private float baseSpeedMultiplier = 1.0f;
    [SerializeField] private float maxForwardSpeedForBase = 40f;

    [Header("Drift Score Settings")]
    [SerializeField] private float peakSharpness = 60f;
    [SerializeField] private float sharpnessExponent = 0.75f;
    [SerializeField] private float timeScale = 2f;
    [SerializeField] private float minSharpnessForScoring = 3f;
    [SerializeField] private float minLateralSpeed = 0.5f;
    [SerializeField] private float minForwardSpeed = 1f;

    [Header("Drift Multiplier Settings")]
    [SerializeField, Tooltip("How strongly drift multiplier is applied per second")]
    private float driftMultiplierRate = 0.7f; // Changed from 1f to 0.7f - multiplier builds 30% slower
    [SerializeField] private float maxDriftMultiplier = 10f;

    [Header("Drift Bonus Ranges")]
    [SerializeField] private float minDriftBonus = 300f;
    [SerializeField] private float midDriftBonus = 2000f;
    [SerializeField] private float maxDriftBonus = 7500f;
    [SerializeField, Range(0f, 1f), Tooltip("Very High Quality threshold for mid-tier bonus")]
    private float midTierThreshold = 0.4f;
    [SerializeField] private float grassMaxBonus = 350f;

    [Header("Animation Settings")]
    [SerializeField, Tooltip("Duration to animate bonus into score bcs Animation = addicting")]
    private float bonusApplyDuration = 1.0f;

    [Header("Debug Settings")]
    [SerializeField] private bool debugScoreBreakdown = false;

    [Header("UI References")]
    [SerializeField] private MultCounter multCounter;

    public static ScoreManager instance;
    public event Action<int> OnScoreChanged;
    public float CurrentDriftMultiplier => isDriftingActive ? driftCompoundMultiplier : 0f;

    private float scoreFloat;
    private int lastReportedScore = -1;
    private float driftTime;
    private CarController carController;
    private RacerScript racerScript;
    private bool isOnGrass = false;

    private bool isDriftingActive = false;
    private float driftStartScore = 0f;
    private float driftSessionBaseGain = 0f;
    private float driftCompoundMultiplier = 1f;
    private bool touchedGrassWhileDrifting = false;

    private float pendingDriftBonusTotal = 0f;
    private float bonusApplyProgress = 0f;
    private float bonusAddedSoFar = 0f;
    private bool isApplyingBonus = false;

    private int driftCount = 0;

    //Points - aika
    private float TimeStartPoint = 5000f;
    private float RaceTimer = 0f;
    private const float PointsDecayTime = 300f; // nyt on 5min ennenkuin 0 aika pointtia

    //all this for the purple car
    private float scoreMultiplier = 1.0f;

    [SerializeField] private AudioSource driftMultLost;



    public void LoadData(GameData data)
    {
        if (data != null)
        {
            return;
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data != null && racerScript.raceFinished == true) 
        {
            int finalScore = GetScoreInt() + Mathf.FloorToInt(TimeStartPoint);
            data.scored += finalScore;
            Debug.Log($"ScoreManager: Saved final score {finalScore} to GameData.");
        }       
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        carController = FindFirstObjectByType<CarController>();
        racerScript = FindFirstObjectByType<RacerScript>();
        multCounter = FindFirstObjectByType<MultCounter>();
        print(scoreMultiplier);
    }

    void Update()
    {
        //print(TimeStartPoint);
        if (!EnsureCarController()) return;

        float deltaTime = Time.deltaTime;
        Vector3 velocity = GetVelocity();

        if (racerScript != null && racerScript.racestarted && !racerScript.raceFinished)
        {
            RaceTimer += deltaTime;
            UpdateTimePoints();
        }

        UpdateBaseScore(deltaTime, velocity);
        UpdateDriftState(deltaTime, velocity);
        AnimatePendingBonus(deltaTime);
        UpdateScoreUIIfChanged();
    }

    bool EnsureCarController()
    {
        if (carController != null) return true;
        carController = FindFirstObjectByType<CarController>();
        return carController != null;
    }

    Vector3 GetVelocity()
    {
        return (carController.carRb != null) ? carController.carRb.linearVelocity : Vector3.zero;
    }

    void UpdateTimePoints()
    {
        TimeStartPoint = Mathf.Max(0f, 5000f * (1f - RaceTimer / PointsDecayTime));
    }

    public void SetScoreMultiplier(float multiplier)
    {
        scoreMultiplier = multiplier;
    }

    void UpdateBaseScore(float deltaTime, Vector3 velocity)
    {
        if (isOnGrass) return;
           
        float forwardSpeed = Mathf.Max(0f, Vector3.Dot(velocity, carController.transform.forward));
        float speedFactor = Mathf.Clamp01(forwardSpeed / Mathf.Max(0.0001f, maxForwardSpeedForBase));
        float speedMultiplier = 1f + speedFactor * baseSpeedMultiplier;

        scoreFloat += basePointsPerSecond * speedMultiplier * scoreMultiplier * deltaTime;
    }

    void UpdateDriftState(float deltaTime, Vector3 velocity)
    {
        bool canDriftNow = !isOnGrass && carController.isDrifting;

        if (canDriftNow)
        {
            if (!isDriftingActive)
            {
                StartDrift();
            }

            float finalMultiplier = ComputeDriftMultiplierIncrement(velocity, deltaTime);
            if (finalMultiplier > 0f)
            {
                AccumulateDriftMultiplier(finalMultiplier, deltaTime);
            }
        }
        else if (isDriftingActive)
        {
            EndDrift();
        }
    }

    

    void StartDrift()
    {
        isDriftingActive = true;
        driftStartScore = scoreFloat;
        driftSessionBaseGain = 0f;
        driftCompoundMultiplier = 1f;
        driftTime = 0f;
        touchedGrassWhileDrifting = false;

        if (multCounter != null)
        {
            multCounter.ShowMultiplier(1f, 0f);
        }
    }

    void AccumulateDriftMultiplier(float finalMultiplier, float deltaTime)
    {
        driftCompoundMultiplier *= Mathf.Pow(finalMultiplier, deltaTime * driftMultiplierRate);
        driftCompoundMultiplier = Mathf.Min(driftCompoundMultiplier, maxDriftMultiplier);
        driftSessionBaseGain += basePointsPerSecond * deltaTime;
    }

    void EndDrift()
    {
        float intensity = Mathf.InverseLerp(1f, maxDriftMultiplier, driftCompoundMultiplier);
        float timeFactor = Mathf.Clamp01(driftTime / 3f);
        float combinedQuality = Mathf.Clamp01(0.5f * intensity + 0.5f * timeFactor);

        ApplyDriftBonusOnce();
        isDriftingActive = false;
        driftTime = 0f;
        driftCompoundMultiplier = 1f;
        driftSessionBaseGain = 0f;

        // Immediately hide multiplier - no delay
        if (multCounter != null)
        {
            multCounter.HideMultiplier();
        }
    }

    void AnimatePendingBonus(float deltaTime)
    {
        if (!isApplyingBonus || pendingDriftBonusTotal <= 0f) return;

        bonusApplyProgress += (bonusApplyDuration <= 0f) ? 1f : deltaTime / bonusApplyDuration;
        float normalizedTime = Mathf.Clamp01(bonusApplyProgress);
        float easedTime = 1f - Mathf.Pow(1f - normalizedTime, 2f);

        float targetBonus = pendingDriftBonusTotal * easedTime;
        float bonusToAdd = targetBonus - bonusAddedSoFar;
        
        if (bonusToAdd > 0f)
        {
            scoreFloat += bonusToAdd;
            bonusAddedSoFar += bonusToAdd;
        }

        if (normalizedTime >= 1f)
        {
            FinishBonusAnimation();
        }
    }

    void FinishBonusAnimation()
    {
        isApplyingBonus = false;
        pendingDriftBonusTotal = 0f;
        bonusApplyProgress = 0f;
        bonusAddedSoFar = 0f;
    }

    void UpdateScoreUIIfChanged()
    {
        int currentScore = Mathf.FloorToInt(scoreFloat);
        if (currentScore == lastReportedScore) return;

        lastReportedScore = currentScore;
        OnScoreChanged?.Invoke(currentScore);
    }

    float ComputeDriftMultiplierIncrement(Vector3 velocity, float deltaTime)
    {
        if (deltaTime <= 0f || carController == null || carController.carRb == null) return 0f;

        float speed = velocity.magnitude;
        if (speed < minForwardSpeed) return 0f;

        float lateralSpeed = Mathf.Abs(Vector3.Dot(velocity, carController.transform.right));
        if (lateralSpeed < minLateralSpeed) return 0f;

        float sharpness;
        try { sharpness = Mathf.Abs(carController.GetDriftSharpness()); }
        catch { return 0f; }
        if (sharpness < minSharpnessForScoring) return 0f;

        driftTime += deltaTime;

        float normalizedSharpness = Mathf.InverseLerp(minSharpnessForScoring, peakSharpness, sharpness);
        float sharpnessBonus = Mathf.Pow(normalizedSharpness, sharpnessExponent);

        float timeBonus = 1f + driftTime / timeScale;
        float baseMultiplier = 1f + sharpnessBonus * timeBonus;

        float forwardSpeed = Mathf.Max(0f, Vector3.Dot(velocity, carController.transform.forward));
        float speedFactor = Mathf.Clamp01(forwardSpeed / Mathf.Max(0.5f, maxForwardSpeedForBase));

        return baseMultiplier * (1f + speedFactor);
    }

    void ApplyDriftBonusOnce()
    {
        if (driftTime <= 0.2f || driftCompoundMultiplier <= 1.01f)
        {
            touchedGrassWhileDrifting = false;
            return;
        }

        driftCount++;

        float intensity = Mathf.InverseLerp(1f, maxDriftMultiplier, driftCompoundMultiplier);
        float timeFactor = Mathf.Clamp01(driftTime / 3f);
        float combinedQuality = Mathf.Clamp01(0.5f * intensity + 0.5f * timeFactor);

        float bonus = CalculateDriftBonus(combinedQuality);
        bonus *= 0.2f;
        bonus *= scoreMultiplier; //the purple car again

        StartBonusAnimation(bonus);

        if (debugScoreBreakdown)
        {
            LogDriftBreakdown(intensity, timeFactor, combinedQuality, bonus);
        }

        touchedGrassWhileDrifting = false;
    }

    float CalculateDriftBonus(float quality)
    {
        if (touchedGrassWhileDrifting)
        {
            return Mathf.Lerp(0f, grassMaxBonus, quality);
        }

        if (quality < midTierThreshold)
        {
            float normalizedQuality = quality / midTierThreshold;
            return Mathf.Lerp(minDriftBonus, midDriftBonus, normalizedQuality);
        }
        else
        {
            float normalizedQuality = (quality - midTierThreshold) / (1f - midTierThreshold);
            return Mathf.Lerp(midDriftBonus, maxDriftBonus, normalizedQuality);
        }
    }

    void StartBonusAnimation(float bonus)
    {
        pendingDriftBonusTotal = bonus;
        isApplyingBonus = bonus > 0f;
        bonusApplyProgress = 0f;
        bonusAddedSoFar = 0f;
    }

    void LogDriftBreakdown(float intensity, float timeFactor, float quality, float finalBonus)
    {
        float scoreAfterBonus = scoreFloat + finalBonus;
        string tier = quality < midTierThreshold ? "LOW-MID" : "MID-HIGH";
        string bonusRange = GetBonusRangeDescription(quality);
        
        Debug.Log($"═══════════════════════════════════════════════════════════════");
        Debug.Log($"[DRIFT #{driftCount}] ENDED - {tier} TIER");
        Debug.Log($"───────────────────────────────────────────────────────────────");
        Debug.Log($"TIME & MULTIPLIER:");
        Debug.Log($"  • Duration: {driftTime:F2}s");
        Debug.Log($"  • Peak Multiplier: x{driftCompoundMultiplier:F2} (max: x{maxDriftMultiplier})");
        Debug.Log($"───────────────────────────────────────────────────────────────");
        Debug.Log($"QUALITY FACTORS:");
        Debug.Log($"  • Intensity: {intensity:P0}");
        Debug.Log($"  • Time Factor: {timeFactor:P0}");
        Debug.Log($"  • Combined Quality: {quality:P0}");
        Debug.Log($"  • Tier: {tier} (threshold: {midTierThreshold:P0})");
        Debug.Log($"───────────────────────────────────────────────────────────────");
        Debug.Log($"BONUS CALCULATION:");
        Debug.Log($"  • Grass Touched: {(touchedGrassWhileDrifting ? "YES" : "NO")}");
        Debug.Log($"  • Bonus Range: {bonusRange}");
        Debug.Log($"  • Final Bonus: +{finalBonus:N0}");
        Debug.Log($"───────────────────────────────────────────────────────────────");
        Debug.Log($"SCORE IMPACT:");
        Debug.Log($"  • Before Drift: {driftStartScore:N0}");
        Debug.Log($"  • After Bonus: {scoreAfterBonus:N0}");
        Debug.Log($"  • Total Gain: +{(scoreAfterBonus - driftStartScore):N0}");
        Debug.Log($"═══════════════════════════════════════════════════════════════");
    }

    string GetBonusRangeDescription(float quality)
    {
        if (touchedGrassWhileDrifting)
            return $"0 → {grassMaxBonus:N0} (grass penalty)";
        
        if (quality < midTierThreshold)
            return $"{minDriftBonus:N0} → {midDriftBonus:N0} (low tier)";
        
        return $"{midDriftBonus:N0} → {maxDriftBonus:N0} (high tier)";
    }

    public void SetOnGrass(bool grassContact)
    {
        if (isOnGrass == grassContact) return;
        isOnGrass = grassContact;

        if (isOnGrass)
        {
            touchedGrassWhileDrifting = true;

            // Print when grass causes an active drift's multiplier to be lost
            if (isDriftingActive && driftCompoundMultiplier > 1.01f)
            {
                Debug.Log($"[ScoreManager] Drift multiplier reset by grass - peak mult: x{driftCompoundMultiplier:F2}, driftTime: {driftTime:F2}s");
                multCounter.UpdateMultiplierText(1f);
                driftMultLost.Play();
            }
        }
    }

    public int GetScoreInt()
    {
        return Mathf.FloorToInt(scoreFloat);
    }

    public float GetDriftTime()
    {
        return driftTime;
    }

    public float GetCurrentSharpness()
    {
        if (carController == null) return 0f;
        
        try 
        { 
            return Mathf.Abs(carController.GetDriftSharpness()); 
        }
        catch 
        { 
            return 0f; 
        }
    }
}
