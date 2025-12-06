using UnityEngine;
using UnityEngine.UI;
using System;

public class ScoreManager : MonoBehaviour
{
    [Header("Base score")]
    [SerializeField] float basePointsPerSecond = 0.1f;   // baseline pts/s
    [SerializeField] float baseSpeedMultiplier = 0.5f; // extra scaling by forward speed
    [SerializeField] float maxForwardSpeedForBase = 40f; // m/s mapped to full bonus

    [Header("Drift score")]
    [SerializeField] float peakSharpness = 120f;          // deg => mapped to 1
    [SerializeField] float sharpnessExponent = 1.5f;     // non-linear growth
    [SerializeField] float timeScale = 2f;               // seconds growth for time bonus
    [SerializeField] float minSharpnessForScoring = 10f; // degrees
    [SerializeField] float minLateralSpeed = 1f;         // m/s
    [SerializeField] float minForwardSpeed = 2f;         // m/s

    [Header("Drift multiplicative settings")]
    [Tooltip("How strongly drift's finalMultiplier is applied per second. 1 = multiply by finalMultiplier once per second.")]
    [SerializeField] float driftMultiplierRate = 1f;

    [Header("Animated bonus")]
    [Tooltip("How long the drift bonus is animated into the real score (seconds)")]
    [SerializeField] float bonusApplyDuration = 1.0f; // seconds over which the bonus is added (animated)

    // NEW: drift reward tuning
    [Header("Drift reward tuning")]
    [Tooltip("Extra base value to treat as 'earned during drift' for bonus calc.")]
    [SerializeField] float driftBaseReward = 10.0f;
    [Tooltip("How strong the multiplier is applied to drift earnings.")]
    [SerializeField] float driftBonusStrength = 2.5f;

    public static ScoreManager instance;

    public float scoreFloat;
    int lastReportedScore = -1;
    float driftTime;
    CarController cr;
    bool OnGrass = false;
    public Text[] ScoreTexts;

    // drift-delayed application state
    bool driftingActive = false;
    float driftStartScore = 0f;
    float driftCompoundMultiplier = 1f;
    float pendingDriftBonusTotal = 0f;
    float bonusApplyProgress = 0f;
    float bonusAddedSoFar = 0f;
    bool applyingBonus = false;

    // expose current drift multiplier for debug
    public float CurrentDriftMultiplier => driftingActive ? driftCompoundMultiplier : 1f;

    int lastTierLogged = 1;   // 1x, 2x, 3x...

    public event Action<int> OnScoreChanged;

    void Awake()
    {
        instance = this;
        UpdateScoreTexts();
    }

    void Start()
    {
        cr = FindFirstObjectByType<CarController>();
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
        scoreFloat += basePointsPerSecond * mult * dt;
    }

    void UpdateDriftState(float dt, Vector3 vel)
    {
        bool canDriftNow = !OnGrass && cr.isDrifting;

        if (canDriftNow)
        {
            if (!driftingActive)
            {
                driftingActive = true;
                driftStartScore = scoreFloat;
                driftCompoundMultiplier = 1f;
                driftTime = 0f;
            }

            float finalMult = ComputeDriftMultiplierIncrement(vel, dt);
            if (finalMult > 0f)
                driftCompoundMultiplier *= Mathf.Pow(finalMult, dt * driftMultiplierRate);
        }
        else if (driftingActive)
        {
            ApplyDriftBonusOnce();
            driftingActive = false;
            driftTime = 0f;
            driftCompoundMultiplier = 1f;
        }
    }

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
        UpdateScoreTexts();
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
        float norm = Mathf.Clamp01(sharpness / peakSharpness);
        float sharpBonus = Mathf.Pow(norm, sharpnessExponent);
        float timeBonus = 1f + driftTime / timeScale;
        float multiplier = 1f + sharpBonus * timeBonus;

        float forwardSp = Mathf.Max(0f, Vector3.Dot(vel, cr.transform.forward));
        float speedFactor = Mathf.Clamp01(forwardSp / Mathf.Max(0.5f, maxForwardSpeedForBase));

        finalMultiplier = multiplier * (1f + speedFactor);
        return true;
    }

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

        float norm = Mathf.Clamp01(sharp / peakSharpness);
        float sharpBonus = Mathf.Pow(norm, sharpnessExponent);
        float timeBonus = 1f + driftTime / timeScale;
        float mult = 1f + sharpBonus * timeBonus;

        float forward = Mathf.Max(0f, Vector3.Dot(vel, cr.transform.forward));
        float speedFactor = Mathf.Clamp01(forward / Mathf.Max(0.5f, maxForwardSpeedForBase));

        return mult * (1f + speedFactor);
    }

    // called once when drift ends
    void ApplyDriftBonusOnce()
    {
        float earned = Mathf.Max(0f, scoreFloat - driftStartScore);
        if (earned <= 0f) return;

        float effectiveEarned = earned + driftBaseReward;
        float rawGain = (driftCompoundMultiplier - 1f) * driftBonusStrength;
        if (rawGain <= 0f) return;

        float bonus = effectiveEarned * rawGain;
        bonus = Mathf.Clamp(bonus, 0f, effectiveEarned * 40f);

        pendingDriftBonusTotal = bonus;
        applyingBonus = bonus > 0f;
        bonusApplyProgress = 0f;
        bonusAddedSoFar = 0f;
    }

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

    public void SetOnGrass(bool isOnGrass)
    {
        if (OnGrass == isOnGrass) return;
        OnGrass = isOnGrass;

        if (OnGrass)
        {
            driftingActive = false;
            driftTime = 0f;
            driftCompoundMultiplier = 1f;
            pendingDriftBonusTotal = 0f;
            applyingBonus = false;
            bonusApplyProgress = 0f;
            bonusAddedSoFar = 0f;
        }
    }

    void UpdateScoreTexts()
    {
        string s = "Score: " + GetScoreInt().ToString();
        if (ScoreTexts == null) return;
        for (int i = 0; i < ScoreTexts.Length; i++)
            if (ScoreTexts[i] != null) ScoreTexts[i].text = s;
    }

    public int GetScoreInt() => Mathf.FloorToInt(scoreFloat);
}