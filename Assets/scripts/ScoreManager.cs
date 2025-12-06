using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.AI;

public class ScoreManager : MonoBehaviour
{
    [Header("Base score")]
    [SerializeField] float basePointsPerSecond = 0.1f;   // baseline pts/s
    [SerializeField] float baseSpeedMultiplier = 0.5f; // extra scaling by forward speed
    [SerializeField] float maxForwardSpeedForBase = 40f; // m/s mapped to full bonus

    [Header("Drift score")]
    [SerializeField] float peakSharpness = 180f;          // deg => mapped to 1
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
    [SerializeField] float driftBaseReward = 5f;
    [Tooltip("How strong the multiplier is applied to drift earnings.")]
    [SerializeField] float driftBonusStrength = 1.5f;

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

    // animated bonus state
    float pendingDriftBonusTotal = 0f;
    float bonusApplyProgress = 0f; // 0..1
    float bonusAddedSoFar = 0f;
    bool applyingBonus = false;

    public event Action<int> OnScoreChanged;

    void Awake()
    {
        UpdateScoreTexts();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        //ottaa carcontrollerin 
        cr = FindFirstObjectByType<CarController>();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // pelaaja saa jatkuvasti scorea, nopeus vaikuttaa
        Vector3 vel = (cr != null && cr.carRb != null) ? cr.carRb.linearVelocity : Vector3.zero;
        float forwardSpeed = Mathf.Max(0f, Vector3.Dot(vel, cr.transform.forward));
        float speedFactor = Mathf.Clamp01(forwardSpeed / Mathf.Max(0.0001f, maxForwardSpeedForBase));
        float baseMultiplier = 1f + speedFactor * baseSpeedMultiplier;
        scoreFloat += basePointsPerSecond * baseMultiplier * dt;
        

        // drifttaus multiplier resettantuu jos driftaa nurmella
        if (!OnGrass && cr != null && cr.isDrifting)
        {
            if (!driftingActive)
            {
                driftingActive = true;
                driftStartScore = scoreFloat;
                driftCompoundMultiplier = 1f;
                driftTime = 0f;
            }
            UpdateDriftCompound(dt, vel);
        }
        else
        {
            if (driftingActive)
            {
                // diftaus loppuu nii multi resettantuu
                PrepareDriftBonus();
                driftingActive = false;
                driftTime = 0f;
                driftCompoundMultiplier = 1f;
            }
        }

        // Bonus tehty koukuttavvaksi: animoidaan pisteiden lisäys
        if (applyingBonus && pendingDriftBonusTotal > 0f)
        {
            bonusApplyProgress += (bonusApplyDuration <= 0f) ? 1f : (dt / bonusApplyDuration);
            float t = Mathf.Clamp01(bonusApplyProgress);
            // ease-out curve: fast start, slow finish
            float curveT = 1f - Mathf.Pow(1f - t, 2f);
            float wantAdded = pendingDriftBonusTotal * curveT;
            float toAdd = wantAdded - bonusAddedSoFar;
            if (toAdd > 0f)
            {
                scoreFloat += toAdd;
                bonusAddedSoFar += toAdd;
            }

            if (t >= 1f)
            {
                applyingBonus = false;
                pendingDriftBonusTotal = 0f;
                bonusApplyProgress = 0f;
                bonusAddedSoFar = 0f;
            }
        }

        int intScore = Mathf.FloorToInt(scoreFloat);
        if (intScore != lastReportedScore)
        {
            lastReportedScore = intScore;
            OnScoreChanged?.Invoke(intScore);
            UpdateScoreTexts();
        }
    }

    // updates driftCompoundMultiplier each frame while drifting (accumulate compound factor)
    void UpdateDriftCompound(float deltaTime, Vector3 vel)
    {
        if (deltaTime <= 0f) return;
        if (cr == null || cr.carRb == null) return;

        float speed = vel.magnitude;
        if (speed < minForwardSpeed) return;

        float lateralSpeed = Mathf.Abs(Vector3.Dot(vel, cr.transform.right));
        if (lateralSpeed < minLateralSpeed) return;

        float sharpness = 0f;
        try { sharpness = Mathf.Abs(cr.GetDriftSharpness()); }
        catch { return; }

        if (sharpness < minSharpnessForScoring) return;

        // compute finalMultiplier for this frame (same math)
        driftTime += deltaTime;
        float norm = Mathf.Clamp01(sharpness / peakSharpness);
        float sharpBonus = Mathf.Pow(norm, sharpnessExponent);
        float timeBonus = 1f + (driftTime / timeScale);
        float multiplier = 1f + sharpBonus * timeBonus;
        float forwardSp = Mathf.Max(0f, Vector3.Dot(vel, cr.transform.forward));
        float speedFactor = Mathf.Clamp01(forwardSp / Mathf.Max(0.5f, maxForwardSpeedForBase));
        float finalMultiplier = multiplier * (1f + speedFactor);

        // accumulate multiplicative factor (compound over the drift)
        float multThisFrame = Mathf.Pow(finalMultiplier, deltaTime * driftMultiplierRate);
        driftCompoundMultiplier *= multThisFrame;
    }

    // Driftaus on palkitsevampi
    void PrepareDriftBonus()
    {
        // points earned while drifting (base + any other additions already in scoreFloat)
        float earnedDuringDrift = Mathf.Max(0f, scoreFloat - driftStartScore);

        // treat as if you earned a bit more during drift (so small drifts still feel good)
        float effectiveEarned = earnedDuringDrift + driftBaseReward;

        if (effectiveEarned <= 0f)
            return; // nothing earned to amplify

        // stronger use of multiplier: (compound - 1) * driftBonusStrength
        float rawMultiplierGain = (driftCompoundMultiplier - 1f) * driftBonusStrength;
        if (rawMultiplierGain <= 0f) return;

        float bonus = effectiveEarned * rawMultiplierGain;

        // clamp but allow bigger bonuses
        bonus = Mathf.Clamp(bonus, 0f, effectiveEarned * 20f);

        // Instead of instantly adding bonus, set it as pending and animate it into scoreFloat
        pendingDriftBonusTotal = bonus;
        applyingBonus = true;
        bonusApplyProgress = 0f;
        bonusAddedSoFar = 0f;

        Debug.Log($"[ScoreManager] Drift ended. earned={earnedDuringDrift:F2}, eff={effectiveEarned:F2}, compound={driftCompoundMultiplier:F3}, bonus={pendingDriftBonusTotal:F2}");
    }

    void UpdateScoreTexts()
    {
        string s = "Score: " + GetScoreInt().ToString();
        for (int i = 0; i < ScoreTexts.Length; i++)
            if (ScoreTexts[i] != null) ScoreTexts[i].text = s;
    }

    public float GetScoreFloat() => scoreFloat;
    public int GetScoreInt() => Mathf.FloorToInt(scoreFloat);
    
    // NEW: handle grass surface state
    public void SetOnGrass(bool isOnGrass)
    {
        OnGrass = isOnGrass;
    }
}