using UnityEngine;

[ExecuteAlways]
public class TrailerCameraSplineFollow : MonoBehaviour
{
    [Header("Bezier Path (4 + n*3 control points)")]
    [Tooltip("Create empty GameObjects as control points. 4 points = 1 cubic segment. For multiple segments, add 3 per extra segment.")]
    public Transform[] controlPoints;

    [Header("Optional rotation controls")]
    [Tooltip("If provided (same count/pattern as controlPoints), rotation will follow a quaternion Bezier. Otherwise it orients to path tangent + offset.")]
    public Transform[] rotationControlPoints;
    public Vector3 rotationOffsetEuler;

    [Header("LookAt per segment")]
    [Tooltip("One element per segment. If null, will use the last non-null LookAt encountered; if none, falls back to rotation path or tangent.")]
    public Transform[] lookAtTargetsPerSegment;
    public bool lookAtOverridesRotationPath = true;

    [Header("Playback")]
    public float duration = 2.0f;
    public AnimationCurve ease = null;
    public bool ignoreTimeScale = true;

    [Header("Smoothing")]
    [Tooltip("Higher = faster response. 0 = instant snap.")]
    public float rotationSmoothingSpeed = 12f;

    [Header("Gizmos")]
    public Color gizmoColor = new Color(1f, 0.65f, 0f, 0.9f);
    public int gizmoSteps = 32;

    private int tweenId = -1;
    private float t; // 0..1
    private Transform lastLookAtTargetUsed;

    void Reset()
    {
        ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
        duration = 2f;
        gizmoSteps = 32;
        gizmoColor = new Color(1f, 0.65f, 0f, 0.9f);
        rotationSmoothingSpeed = 12f;
    }

    void OnValidate()
    {
        // Keep lookAtTargetsPerSegment sized to segment count
        int segCount = GetSegmentCount();
        if (segCount > 0)
        {
            if (lookAtTargetsPerSegment == null || lookAtTargetsPerSegment.Length != segCount)
            {
                var old = lookAtTargetsPerSegment;
                lookAtTargetsPerSegment = new Transform[segCount];
                if (old != null)
                {
                    int copy = Mathf.Min(segCount, old.Length);
                    for (int i = 0; i < copy; i++) lookAtTargetsPerSegment[i] = old[i];
                }
            }
        }
        else
        {
            lookAtTargetsPerSegment = null;
        }
    }

    public void PlayForward()
    {
        StartTweenTo(1f);
    }

    public void PlayBackward()
    {
        StartTweenTo(0f);
    }

    public void SetNormalizedPosition(float normalized) // optional, for debugging from inspector
    {
        t = Mathf.Clamp01(normalized);
        ApplyAt(t);
    }

    public void ClearLookAtMemory()
    {
        lastLookAtTargetUsed = null;
    }

    private void StartTweenTo(float target)
    {
        if (tweenId != -1)
        {
            LeanTween.cancel(tweenId);
            tweenId = -1;
        }

        float start = t;
        float distance = Mathf.Abs(target - start);
        float time = Mathf.Max(0.0001f, duration * distance);

        var descr = LeanTween.value(gameObject, start, target, time)
            .setOnUpdate((float val) =>
            {
                t = val;
                ApplyAt(t);
            })
            .setOnComplete(() => tweenId = -1);

        if (ignoreTimeScale) descr.setIgnoreTimeScale(true);
        if (ease != null) descr.setEase(ease);

        tweenId = descr.id;
    }

    private void ApplyAt(float normalizedT)
    {
        if (!HasValidPath()) return;

        int segIndex;
        float u;
        GetSegmentParams(normalizedT, out segIndex, out u);

        // Position + tangent
        Vector3 pos, tan;
        EvaluatePathAt(segIndex, u, out pos, out tan);
        transform.position = pos;

        // Rotation (LookAt per segment -> rotationControlPoints -> tangent)
        Quaternion rot;
        Transform lookAt = ResolveLookAtTarget(segIndex);
        if (lookAt != null && lookAtOverridesRotationPath)
        {
            Vector3 dir = lookAt.position - pos;
            rot = dir.sqrMagnitude > 0.000001f ? Quaternion.LookRotation(dir.normalized, Vector3.up) : transform.rotation;
        }
        else if (HasValidRotationPath())
        {
            rot = EvaluateRotationAt(segIndex, u);
        }
        else
        {
            rot = tan.sqrMagnitude > 0.000001f ? Quaternion.LookRotation(tan.normalized, Vector3.up) : transform.rotation;
        }

        if (rotationOffsetEuler != Vector3.zero)
            rot = rot * Quaternion.Euler(rotationOffsetEuler);

        // Universal smoothing for orientation changes
        if (Application.isPlaying && rotationSmoothingSpeed > 0f)
        {
            float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            float alpha = 1f - Mathf.Exp(-rotationSmoothingSpeed * Mathf.Max(0f, dt)); // framerate-independent
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, alpha);
        }
        else
        {
            transform.rotation = rot;
        }
    }

    private Transform ResolveLookAtTarget(int segIndex)
    {
        // explicit per-segment target
        Transform target = null;
        if (lookAtTargetsPerSegment != null && segIndex >= 0 && segIndex < lookAtTargetsPerSegment.Length)
            target = lookAtTargetsPerSegment[segIndex];

        if (target != null)
        {
            lastLookAtTargetUsed = target;
            return target;
        }

        // sticky: reuse last non-null
        if (lastLookAtTargetUsed != null) return lastLookAtTargetUsed;

        // seed from earlier segments (useful when scrubbing t without having moved through previous segments)
        if (lookAtTargetsPerSegment != null)
        {
            for (int i = segIndex - 1; i >= 0; i--)
            {
                if (lookAtTargetsPerSegment[i] != null)
                {
                    lastLookAtTargetUsed = lookAtTargetsPerSegment[i];
                    return lastLookAtTargetUsed;
                }
            }
            for (int i = segIndex + 1; i < lookAtTargetsPerSegment.Length; i++)
            {
                if (lookAtTargetsPerSegment[i] != null)
                {
                    lastLookAtTargetUsed = lookAtTargetsPerSegment[i];
                    return lastLookAtTargetUsed;
                }
            }
        }

        return null;
    }

    private int GetSegmentCount()
    {
        if (controlPoints == null || controlPoints.Length < 4) return 0;
        if (((controlPoints.Length - 1) % 3) != 0) return 0;
        return (controlPoints.Length - 1) / 3;
    }

    private void GetSegmentParams(float normalizedT, out int segIndex, out float u)
    {
        int segCount = GetSegmentCount();
        float tScaled = Mathf.Clamp01(normalizedT) * segCount;

        segIndex = Mathf.FloorToInt(tScaled);
        if (segIndex >= segCount) { segIndex = segCount - 1; tScaled = segCount; }
        u = Mathf.Clamp01(tScaled - segIndex);
    }

    private void EvaluatePathAt(int segIndex, float u, out Vector3 position, out Vector3 tangent)
    {
        int baseIdx = segIndex * 3;
        Vector3 p0 = controlPoints[baseIdx + 0].position;
        Vector3 p1 = controlPoints[baseIdx + 1].position;
        Vector3 p2 = controlPoints[baseIdx + 2].position;
        Vector3 p3 = controlPoints[baseIdx + 3].position;

        position = CubicBezier(p0, p1, p2, p3, u);
        tangent = CubicBezierTangent(p0, p1, p2, p3, u);
    }

    private Quaternion EvaluateRotationAt(int segIndex, float u)
    {
        int baseIdx = segIndex * 3;
        Quaternion r0 = rotationControlPoints[baseIdx + 0].rotation;
        Quaternion r1 = rotationControlPoints[baseIdx + 1].rotation;
        Quaternion r2 = rotationControlPoints[baseIdx + 2].rotation;
        Quaternion r3 = rotationControlPoints[baseIdx + 3].rotation;

        return CubicQuaternionBezier(r0, r1, r2, r3, u);
    }

    private bool HasValidPath()
    {
        if (controlPoints == null || controlPoints.Length < 4) return false;
        int n = controlPoints.Length;
        return ((n - 1) % 3) == 0; // 4 + n*3
    }

    private bool HasValidRotationPath()
    {
        if (rotationControlPoints == null) return false;
        if (rotationControlPoints.Length != (controlPoints?.Length ?? 0)) return false;
        if (rotationControlPoints.Length < 4) return false;
        return ((rotationControlPoints.Length - 1) % 3) == 0;
    }

    private static Vector3 CubicBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        float uu = u * u;
        float tt = t * t;

        return (uu * u) * a +
               (3f * uu * t) * b +
               (3f * u * tt) * c +
               (tt * t) * d;
    }

    private static Vector3 CubicBezierTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return 3f * u * u * (b - a) + 6f * u * t * (c - b) + 3f * t * t * (d - c);
    }

    private static Quaternion CubicQuaternionBezier(Quaternion q0, Quaternion q1, Quaternion q2, Quaternion q3, float t)
    {
        Quaternion q01 = Quaternion.Slerp(q0, q1, t);
        Quaternion q12 = Quaternion.Slerp(q1, q2, t);
        Quaternion q23 = Quaternion.Slerp(q2, q3, t);

        Quaternion q012 = Quaternion.Slerp(q01, q12, t);
        Quaternion q123 = Quaternion.Slerp(q12, q23, t);

        return Quaternion.Slerp(q012, q123, t);
    }

    private void OnDrawGizmos()
    {
        if (!HasValidPath()) return;

        Gizmos.color = gizmoColor;

        Vector3 prev, prevTan;
        EvaluatePathAt(0, 0f, out prev, out prevTan);

        int steps = Mathf.Max(2, gizmoSteps);
        for (int i = 1; i <= steps; i++)
        {
            float nt = i / (float)steps;
            int segIndex;
            float u;
            GetSegmentParams(nt, out segIndex, out u);

            Vector3 p, tan;
            EvaluatePathAt(segIndex, u, out p, out tan);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }

        // Draw control points
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.6f);
        foreach (var cp in controlPoints)
        {
            if (cp != null) Gizmos.DrawSphere(cp.position, 0.05f);
        }
    }
}