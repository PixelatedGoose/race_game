using UnityEngine;
using UnityEngine.UI;
using System;


public class ScoreManager : MonoBehaviour, IDataPersistence
{
    [Header("Base score")]
     float basePointsPerSecond = 1.5f;   // was 0.1f  -> MUCH FASTER
     float baseSpeedMultiplier = 1.0f;   // was 0.5f  -> more reward for speed
     float maxForwardSpeedForBase = 40f; // m/s mapped to full bonus

    [Header("Drift score")]
     float peakSharpness = 60f;          // was 120f  -> max at smaller angle
     float sharpnessExponent = 0.75f;    // was 1.5f  -> low angles are rewarded more
     float timeScale = 2f;               
     float minSharpnessForScoring = 3f;  // was 10f   -> much easier to start drifting
     float minLateralSpeed = 0.5f;       // was 1f    -> needs less side slip
     float minForwardSpeed = 1f;         // was 2f    -> works at lower speeds

    [Header("Drift multiplicative settings")]
    [Tooltip("How strongly drift's finalMultiplier is applied per second. 1 = multiply by finalMultiplier once per second.")]
     float driftMultiplierRate = 1f;

    [Header("Animated bonus")]
    [Tooltip("How long the drift bonus is animated into the real score (seconds)")]
     float bonusApplyDuration = 1.0f; // seconds over which the bonus is added (animated)

    // NEW: drift reward tuning
    
    [Header("Debug")]
    [Tooltip("Enable detailed drift/score logs in console when each drift ends")]
     bool debugScoreBreakdown = false;

    public static ScoreManager instance;

    public float scoreFloat;
    int lastReportedScore = -1;
    float driftTime;
    CarController cr;
    RacerScript rc;
    bool OnGrass = false;

    // drift-delayed application state
    bool driftingActive = false;
    float driftStartScore = 0f;
    float driftSessionBaseGain = 0f;   // <- NEW: clean "earned during this drift"
    float driftCompoundMultiplier = 1f;
    float pendingDriftBonusTotal = 0f;
    float bonusApplyProgress = 0f;
    float bonusAddedSoFar = 0f;
    bool applyingBonus = false;

    bool touchedgrasswhiledrifting = false;

    [Header("Drift caps")]
     float maxDriftMultiplier = 10f;
     float minDriftBonus = 300f;        // was 0f   -> MIN 2k
     float maxDriftBonus = 7500f;        // was 4000 -> CAP ~3k

    // expose current drift multiplier for debug
    public float CurrentDriftMultiplier => driftingActive ? driftCompoundMultiplier : 1f;

    int lastTierLogged = 1;   // 1x, 2x, 3x...
    int driftCount = 0; // track total drifts for debug

    public event Action<int> OnScoreChanged;


    //for the data
    public void LoadData(GameData data)
    {
        if (data != null)
        {
            return;
        }
    }
    //we need to save the data marvin
    public void SaveData(ref GameData data)
    {
        if (data != null && rc.raceFinished == true) 
        {
            data.scored += this.GetScoreInt();
            print(data.scored);
        }       
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cr = FindFirstObjectByType<CarController>();
        rc = FindFirstObjectByType<RacerScript>();
    }

    void Update()
    {
        if (!EnsureCarController()) return;

        float dt = Time.deltaTime;
        Vector3 vel = GetVelocity();

        UpdateBaseScore(dt, vel);
        UpdateDriftState(dt, vel);
        AnimatePendingBonus(dt);
        UpdateScoreUIIfChanged();
    }

    // --- helpers ---

    bool EnsureCarController()
    {
        if (cr != null) return true;
        cr = FindFirstObjectByType<CarController>();
        return cr != null;
    }

    Vector3 GetVelocity()
    {
        return (cr.carRb != null) ? cr.carRb.linearVelocity : Vector3.zero;
    }

    void UpdateBaseScore(float dt, Vector3 vel)
    {
        float forwardSpeed = Mathf.Max(0f, Vector3.Dot(vel, cr.transform.forward));
        float speedFactor = Mathf.Clamp01(forwardSpeed / Mathf.Max(0.0001f, maxForwardSpeedForBase));
        float mult = 1f + speedFactor * baseSpeedMultiplier;

        // if on grass, heavily nerf base gain (e.g. 10% of normal)
        if (OnGrass)
            mult *= 0f;

        scoreFloat += basePointsPerSecond * mult * dt;
    }
    //are we drifting marvin??
    void UpdateDriftState(float dt, Vector3 vel)
    {
        bool canDriftNow = !OnGrass && cr.isDrifting;

        if (canDriftNow)
        {
            if (!driftingActive)
            {
                driftingActive = true;
                driftStartScore = scoreFloat; // now only for debug
                driftSessionBaseGain = 0f;     // NEW: start clean per‑drift gain
                driftCompoundMultiplier = 1f;
                driftTime = 0f;
                touchedgrasswhiledrifting = false;
            }

            float finalMult = ComputeDriftMultiplierIncrement(vel, dt);
            if (finalMult > 0f)
            {
                driftCompoundMultiplier *= Mathf.Pow(finalMult, dt * driftMultiplierRate);
                driftCompoundMultiplier = Mathf.Min(driftCompoundMultiplier, maxDriftMultiplier);

                // NEW: accumulate clean drift "earnings" not polluted by old bonuses
                driftSessionBaseGain += basePointsPerSecond * dt;
            }
        }
        else if (driftingActive)
        {
            ApplyDriftBonusOnce();
            driftingActive = false;
            driftTime = 0f;
            driftCompoundMultiplier = 1f;
            driftSessionBaseGain = 0f;  // reset for next drift
        }
    }

    //marvin we need to animate the bonus so people can get addicted to the score increasing
    void AnimatePendingBonus(float dt)
    {
        if (!applyingBonus || pendingDriftBonusTotal <= 0f) return;

        bonusApplyProgress += (bonusApplyDuration <= 0f) ? 1f : dt / bonusApplyDuration;
        float t = Mathf.Clamp01(bonusApplyProgress);
        float curveT = 1f - Mathf.Pow(1f - t, 2f);

        float want = pendingDriftBonusTotal * curveT;
        float add = want - bonusAddedSoFar;
        if (add > 0f)
        {
            scoreFloat += add;
            bonusAddedSoFar += add;
        }

        if (t >= 1f)
        {
            applyingBonus = false;
            pendingDriftBonusTotal = 0f;
            bonusApplyProgress = 0f;
            bonusAddedSoFar = 0f;
        }
    }

    void UpdateScoreUIIfChanged()
    {
        int intScore = Mathf.FloorToInt(scoreFloat);
        if (intScore == lastReportedScore) return;

        lastReportedScore = intScore;
        OnScoreChanged?.Invoke(intScore);
    }

    // Single helper that contains all drift gating, returns finalMultiplier only if OK
    bool PassesDriftGates(Vector3 vel, float deltaTime, out float finalMultiplier)
    {
        finalMultiplier = 1f;

        if (deltaTime <= 0f || cr == null || cr.carRb == null)
            return false;

        float speed = vel.magnitude;
        if (speed < minForwardSpeed)
            return false;

        float lateralSpeed = Mathf.Abs(Vector3.Dot(vel, cr.transform.right));
        if (lateralSpeed < minLateralSpeed)
            return false;

        float sharpness;
        try { sharpness = Mathf.Abs(cr.GetDriftSharpness()); }
        catch { return false; }

        if (sharpness < minSharpnessForScoring)
            return false;

        // passed all gates -> compute multiplier
        driftTime += deltaTime;

        // NEW: map [minSharpnessForScoring .. peakSharpness] -> [0..1] more softly
        float normRaw = Mathf.InverseLerp(minSharpnessForScoring, peakSharpness, sharpness);
        float sharpBonus = Mathf.Pow(normRaw, sharpnessExponent);

        float timeBonus = 1f + driftTime / timeScale;
        float multiplier = 1f + sharpBonus * timeBonus;

        float forwardSp = Mathf.Max(0f, Vector3.Dot(vel, cr.transform.forward));
        float speedFactor = Mathf.Clamp01(forwardSp / Mathf.Max(0.5f, maxForwardSpeedForBase));

        finalMultiplier = multiplier * (1f + speedFactor);
        return true;
    }

    //marvin we need to multiply
    void AccumulateDriftMultiplier(float finalMultiplier, float deltaTime)
    {
        float multThisFrame = Mathf.Pow(finalMultiplier, deltaTime * driftMultiplierRate);
        driftCompoundMultiplier *= multThisFrame;
    }

    // returns 0 if drift should not count this frame, otherwise the per‑frame finalMultiplier
    float ComputeDriftMultiplierIncrement(Vector3 vel, float dt)
    {
        if (dt <= 0f || cr == null || cr.carRb == null) return 0f;

        float speed = vel.magnitude;
        if (speed < minForwardSpeed) return 0f;

        float lateral = Mathf.Abs(Vector3.Dot(vel, cr.transform.right));
        if (lateral < minLateralSpeed) return 0f;

        float sharp;
        try { sharp = Mathf.Abs(cr.GetDriftSharpness()); }
        catch { return 0f; }
        if (sharp < minSharpnessForScoring) return 0f;

        driftTime += dt;


        float normRaw = Mathf.InverseLerp(minSharpnessForScoring, peakSharpness, sharp);
        float sharpBonus = Mathf.Pow(normRaw, sharpnessExponent);

        float timeBonus = 1f + driftTime / timeScale;
        float mult = 1f + sharpBonus * timeBonus;

        float forward = Mathf.Max(0f, Vector3.Dot(vel, cr.transform.forward));
        float speedFactor = Mathf.Clamp01(forward / Mathf.Max(0.5f, maxForwardSpeedForBase));

        return mult * (1f + speedFactor);
    }

    // called once when drift ends
    //marvin we need to give the bonus to the player
    void ApplyDriftBonusOnce()
    {
        // must have actually drifted
        if (driftTime <= 0.2f || driftCompoundMultiplier <= 1.01f)
        {
            touchedgrasswhiledrifting = false;
            return;
        }

        driftCount++;

        float minForThisDrift;
        float maxForThisDrift;

        if (touchedgrasswhiledrifting)
        {
            minForThisDrift = 0f;
            maxForThisDrift = 350f;
        }
        else
        {
            minForThisDrift = minDriftBonus;
            maxForThisDrift = maxDriftBonus;
        }

        // 0..1: how intense the drift was, based on multiplier
        float intensity = Mathf.InverseLerp(1f, maxDriftMultiplier, driftCompoundMultiplier);

        // 0..1: how long the drift lasted (3s drift => full)
        float timeFactor = Mathf.Clamp01(driftTime / 3f);

        // combine: both intensity and time matter
        float t = Mathf.Clamp01(0.5f * intensity + 0.5f * timeFactor);

        // use the per‑drift min/max
        float bonus = Mathf.Lerp(minForThisDrift, maxForThisDrift, t);

        // marvin ate half of the points
         bonus *= 0.2f;

        pendingDriftBonusTotal = bonus;
        applyingBonus = bonus > 0f;
        bonusApplyProgress = 0f;
        bonusAddedSoFar = 0f;

        // DEBUG LOG: detailed breakdown
        if (debugScoreBreakdown)
        {
            float scoreBeforeDrift = driftStartScore;
            float scoreAfterBonus = scoreFloat + bonus;
            
            Debug.Log($"═══════════════════════════════════════════════════════════════");
            Debug.Log($"[DRIFT #{driftCount}] ENDED - DETAILED BREAKDOWN");
            Debug.Log($"───────────────────────────────────────────────────────────────");
            Debug.Log($"TIME & MULTIPLIER:");
            Debug.Log($"  • Drift Duration: {driftTime:F2}s");
            Debug.Log($"  • Peak Multiplier: x{driftCompoundMultiplier:F2} (max cap: x{maxDriftMultiplier})");
            Debug.Log($"───────────────────────────────────────────────────────────────");
            Debug.Log($"QUALITY FACTORS:");
            Debug.Log($"  • Intensity (from multiplier): {intensity:P0} [{driftCompoundMultiplier:F2} → {maxDriftMultiplier:F2}]");
            Debug.Log($"  • Time Factor: {timeFactor:P0} [{driftTime:F2}s → 3.00s]");
            Debug.Log($"  • Combined Quality (t): {t:P0}");
            Debug.Log($"───────────────────────────────────────────────────────────────");
            Debug.Log($"BONUS RANGE:");
            Debug.Log($"  • Min Possible: {minForThisDrift:N0}");
            Debug.Log($"  • Max Possible: {maxForThisDrift:N0}");
            Debug.Log($"  • Grass Touched: {(touchedgrasswhiledrifting ? "YES (reduced range)" : "NO (full range)")}");
            Debug.Log($"───────────────────────────────────────────────────────────────");
            Debug.Log($"BONUS CALCULATION:");
            Debug.Log($"  • Raw Bonus (before Marvin): {bonus:N0}");
            Debug.Log($"  • Marvin Tax (×0.2): -{(bonus - bonus):N0}");
            Debug.Log($"  • Final Drift Bonus: +{bonus:N0}");
            Debug.Log($"───────────────────────────────────────────────────────────────");
            Debug.Log($"SCORE IMPACT:");
            Debug.Log($"  • Score Before Drift: {scoreBeforeDrift:N0}");
            Debug.Log($"  • Score After Bonus: {scoreAfterBonus:N0}");
            Debug.Log($"  • Total Gain This Drift: +{(scoreAfterBonus - scoreBeforeDrift):N0}");
            Debug.Log($"  • Current Total Score: {GetScoreInt():N0}");
            Debug.Log($"═══════════════════════════════════════════════════════════════");
        }

        // prepare for next drift
        touchedgrasswhiledrifting = false;
    }

    //marvin we need to see the multiplier work
    void DebugDriftMultiplierOncePerTier()
    {
        if (!driftingActive) { lastTierLogged = 1; return; }

        float mult = CurrentDriftMultiplier;
        if (mult <= 1.01f) { lastTierLogged = 1; return; }

        int tier = Mathf.FloorToInt(mult);
        if (tier <= lastTierLogged) return;

        string label = tier switch
        {
            2 => "DOUBLE",
            3 => "TRIPLE",
            4 => "QUAD",
            _ => $"x{tier}"
        };

        Debug.Log($"[Drift] NEW TIER: {label} (x{mult:0.00})");
        lastTierLogged = tier;
    }
    //NO MARVIN GET OFF THE GRASS
    public void SetOnGrass(bool isOnGrass)
    {
        if (OnGrass == isOnGrass) return;
        OnGrass = isOnGrass;

        if (OnGrass)
        {
            touchedgrasswhiledrifting = true;
        }
    }

    public int GetScoreInt()
    {
        return Mathf.FloorToInt(scoreFloat);
    }
}
