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

    float scoreFloat;
    int lastReportedScore = -1;
    float driftTime;
    CarController cr;
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
    }

    void Start()
    {
        // try to cache the CarController once
        cr = FindFirstObjectByType<CarController>();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // base continuous score (adds directly to float)
        Vector3 vel = (cr != null && cr.carRb != null) ? cr.carRb.linearVelocity : Vector3.zero; // use Rigidbody.velocity
        float forwardSpeed = Mathf.Max(0f, Vector3.Dot(vel, cr.transform.forward));
        float speedFactor = Mathf.Clamp01(forwardSpeed / Mathf.Max(0.0001f, maxForwardSpeedForBase));
        float baseMultiplier = 1f + speedFactor * baseSpeedMultiplier;
        scoreFloat += basePointsPerSecond * baseMultiplier * dt;

        // handle drift start / ongoing / end
        if (cr.isDrifting)
        {
            if (!driftingActive)
            {
                // drift just started
                driftingActive = true;
                driftStartScore = scoreFloat;          // remember score at drift start
                driftCompoundMultiplier = 1f;         // reset compound multiplier
                driftTime = 0f;
            }

            // update compound multiplier only (do NOT modify scoreFloat here)
            UpdateDriftCompound(dt, vel);
        }
        else
        {
            if (driftingActive)
            {
                // drift just ended -> prepare accumulated multiplicative bonus to be animated in
                PrepareDriftBonus();
                driftingActive = false;
                driftTime = 0f;
                driftCompoundMultiplier = 1f;
            }
        }

        // apply animated bonus over time if present
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
                // finished applying bonus
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

    // prepare the drift bonus to be animated in rather than applied instantly
    void PrepareDriftBonus()
    {
        // points earned while drifting (base + any other additions already in scoreFloat)
        float earnedDuringDrift = Mathf.Max(0f, scoreFloat - driftStartScore);
        if (earnedDuringDrift <= 0f)
            return; // nothing earned to amplify

        // bonus = earnedDuringDrift * (driftCompoundMultiplier - 1)
        float bonus = earnedDuringDrift * (driftCompoundMultiplier - 1f);
        if (bonus <= 0f) return;

        // safety clamp to avoid insane values
        bonus = Mathf.Clamp(bonus, 0f, earnedDuringDrift * 10f);

        // Instead of instantly adding bonus, set it as pending and animate it into scoreFloat
        pendingDriftBonusTotal = bonus;
        applyingBonus = true;
        bonusApplyProgress = 0f;
        bonusAddedSoFar = 0f;

        Debug.Log($"[ScoreManager] Drift ended. earnedDuringDrift={earnedDuringDrift:F2}, compound={driftCompoundMultiplier:F3}, pendingBonus={pendingDriftBonusTotal:F2}");
    }

    void UpdateScoreTexts()
    {
        string s = "Score: " + GetScoreInt().ToString();
        for (int i = 0; i < ScoreTexts.Length; i++)
            if (ScoreTexts[i] != null) ScoreTexts[i].text = s;
    }

    public int GetScoreInt() => Mathf.FloorToInt(scoreFloat);
    public float GetScoreFloat() => scoreFloat;
}