using UnityEngine;

[ExecuteAlways]
public class TrailerCameraSplineFollow : MonoBehaviour
{
    [System.Serializable]
    public class BezierRoute
    {
        [Tooltip("Optional label for clarity in Inspector")]
        public string name;

        [Header("Bezier Path (4 + n*3 control points)")]
        public Transform[] controlPoints;

        [Header("Optional rotation controls")]
        [Tooltip("If provided (same count/pattern as controlPoints), rotation will follow a quaternion Bezier.")]
        public Transform[] rotationControlPoints;

        [Header("LookAt per segment")]
        [Tooltip("One element per segment. If null in a segment, we keep the last non-null target encountered; if none, fallback to rotation or tangent.")]
        public Transform[] lookAtTargetsPerSegment;
        public bool lookAtOverridesRotationPath = true;

        [Header("Timing")]
        public float duration = 2.0f;
        public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    [Header("Routes")]
    [Tooltip("Route from A to B (Play menu)")]
    public BezierRoute routeAB;
    [Tooltip("Route from A to C (Credits)")]
    public BezierRoute routeAC;

    [Header("Global Playback")]
    public bool ignoreTimeScale = true;

    [Header("Smoothing")]
    [Tooltip("Higher = faster response. 0 = instant snap.")]
    public float rotationSmoothingSpeed = 12f;

    [Header("Gizmos")]
    public Color gizmoColorAB = new Color(0.1f, 0.8f, 1f, 0.9f);
    public Color gizmoColorAC = new Color(1f, 0.65f, 0f, 0.9f);
    public int gizmoSteps = 32;

    // State
    private BezierRoute _currentRoute;
    private int _tweenId = -1;
    private float _t; // 0..1 along current route
    private Transform _lastLookAtTargetUsed;
    private int _seq; // cancel/ignore stale chains
    private bool _didTweenUpdateThisFrame; // NEW

    void Reset()
    {
        rotationSmoothingSpeed = 12f;
        gizmoSteps = 32;
        gizmoColorAB = new Color(0.1f, 0.8f, 1f, 0.9f);
        gizmoColorAC = new Color(1f, 0.65f, 0f, 0.9f);
    }

    void Awake()
    {
        // Default to AB route, starting at A
        if (_currentRoute == null) _currentRoute = routeAB;
        ApplyAt(_currentRoute, 0f);
        _t = 0f;
    }

    void OnValidate()
    {
        AutoSizeLookAt(routeAB);
        AutoSizeLookAt(routeAC);
    }

    // Public API (keeps your existing hookups working)
    public void PlayForward() => GoToB();  // A -> B
    public void PlayBackward() => GoToA(); // B -> A
    public void GoToB() => StartSequenceToEndpoint(_currentRoute == routeAB ? Endpoint.B : EndpointViaA(Endpoint.B));
    public void GoToA() => StartSequenceToEndpoint(Endpoint.A);
    public void GoToC() => StartSequenceToEndpoint(_currentRoute == routeAC ? Endpoint.C : EndpointViaA(Endpoint.C));

    // Helper to enforce the rule “cannot go B->C directly”
    private Endpoint StartSequenceToEndpoint(Endpoint target)
    {
        // Interrupt any ongoing chain
        _seq++;

        // Decide sequence based on current route and t
        if (target == Endpoint.A)
        {
            TweenTo(0f, _currentRoute, null);
            return target;
        }

        // Target is B or C
        BezierRoute desired = (target == Endpoint.B) ? routeAB : routeAC;
        if (_currentRoute == desired)
        {
            TweenTo(1f, desired, null);
            return target;
        }

        // Different route requested. If not at A, go to A first on the current route, then switch and go to 1.
        int mySeq = _seq;
        if (_t > 0f)
        {
            TweenTo(0f, _currentRoute, () =>
            {
                if (mySeq != _seq) return; // canceled
                _currentRoute = desired;
                _lastLookAtTargetUsed = null;
                TweenTo(1f, _currentRoute, null);
            });
        }
        else
        {
            // Already at A, just switch route and go
            _currentRoute = desired;
            _lastLookAtTargetUsed = null;
            TweenTo(1f, _currentRoute, null);
        }

        return target;
    }

    private Endpoint EndpointViaA(Endpoint target) => target; // semantic marker

    private void TweenTo(float targetT, BezierRoute route, System.Action onComplete)
    {
        if (!HasValidPath(route)) return;

        // Cancel running tween
        if (_tweenId != -1)
        {
            LeanTween.cancel(_tweenId);
            _tweenId = -1;
        }

        float start = _t;
        float dist = Mathf.Abs(targetT - start);
        float dur = Mathf.Max(0.0001f, (route?.duration ?? 2f) * dist);

        int mySeq = _seq;

        var descr = LeanTween.value(gameObject, start, targetT, dur)
            .setOnUpdate((float val) =>
            {
                // If a new sequence started, ignore updates
                if (mySeq != _seq) return;

                _t = val;
                _didTweenUpdateThisFrame = true; // NEW: mark so LateUpdate won't re-apply
                ApplyAt(route, _t);
            })
            .setOnComplete(() =>
            {
                if (mySeq != _seq) return;
                _tweenId = -1;
                _t = targetT;
                _didTweenUpdateThisFrame = true; // NEW: ensure no double-apply this frame
                ApplyAt(route, _t);
                onComplete?.Invoke();
            });

        if (ignoreTimeScale) descr.setIgnoreTimeScale(true);
        if (route != null && route.ease != null) descr.setEase(route.ease);

        _tweenId = descr.id;
        _currentRoute = route;
    }

    private void ApplyAt(BezierRoute route, float normalizedT)
    {
        if (!HasValidPath(route)) return;

        int segIndex;
        float u;
        GetSegmentParams(route, normalizedT, out segIndex, out u);

        // Position + tangent
        Vector3 pos, tan;
        EvaluatePathAt(route, segIndex, u, out pos, out tan);
        transform.position = pos;

        // Rotation: LookAt per segment -> rotation path -> tangent
        Quaternion rot;
        Transform lookAt = ResolveLookAtTarget(route, segIndex);
        if (lookAt != null && route.lookAtOverridesRotationPath)
        {
            Vector3 dir = lookAt.position - pos;
            rot = dir.sqrMagnitude > 0.000001f ? Quaternion.LookRotation(dir.normalized, Vector3.up) : transform.rotation;
        }
        else if (HasValidRotationPath(route))
        {
            rot = EvaluateRotationAt(route, segIndex, u);
        }
        else
        {
            rot = tan.sqrMagnitude > 0.000001f ? Quaternion.LookRotation(tan.normalized, Vector3.up) : transform.rotation;
        }

        // Universal smoothing
        if (Application.isPlaying && rotationSmoothingSpeed > 0f)
        {
            float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            float alpha = 1f - Mathf.Exp(-rotationSmoothingSpeed * Mathf.Max(0f, dt));
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, alpha);
        }
        else
        {
            transform.rotation = rot;
        }
    }

    private void AutoSizeLookAt(BezierRoute route)
    {
        if (route == null) return;
        int segCount = GetSegmentCount(route);
        if (segCount <= 0) { route.lookAtTargetsPerSegment = null; return; }

        if (route.lookAtTargetsPerSegment == null || route.lookAtTargetsPerSegment.Length != segCount)
        {
            var old = route.lookAtTargetsPerSegment;
            route.lookAtTargetsPerSegment = new Transform[segCount];
            if (old != null)
            {
                int copy = Mathf.Min(segCount, old.Length);
                for (int i = 0; i < copy; i++) route.lookAtTargetsPerSegment[i] = old[i];
            }
        }
    }

    private Transform ResolveLookAtTarget(BezierRoute route, int segIndex)
    {
        Transform target = null;
        if (route.lookAtTargetsPerSegment != null &&
            segIndex >= 0 && segIndex < route.lookAtTargetsPerSegment.Length)
        {
            target = route.lookAtTargetsPerSegment[segIndex];
        }

        if (target != null)
        {
            _lastLookAtTargetUsed = target;
            return target;
        }

        if (_lastLookAtTargetUsed != null) return _lastLookAtTargetUsed;

        if (route.lookAtTargetsPerSegment != null)
        {
            for (int i = segIndex - 1; i >= 0; i--)
            {
                if (route.lookAtTargetsPerSegment[i] != null)
                {
                    _lastLookAtTargetUsed = route.lookAtTargetsPerSegment[i];
                    return _lastLookAtTargetUsed;
                }
            }
            for (int i = segIndex + 1; i < route.lookAtTargetsPerSegment.Length; i++)
            {
                if (route.lookAtTargetsPerSegment[i] != null)
                {
                    _lastLookAtTargetUsed = route.lookAtTargetsPerSegment[i];
                    return _lastLookAtTargetUsed;
                }
            }
        }

        return null;
    }

    private int GetSegmentCount(BezierRoute route)
    {
        if (route?.controlPoints == null || route.controlPoints.Length < 4) return 0;
        if (((route.controlPoints.Length - 1) % 3) != 0) return 0;
        return (route.controlPoints.Length - 1) / 3;
    }

    private void GetSegmentParams(BezierRoute route, float normalizedT, out int segIndex, out float u)
    {
        int segCount = GetSegmentCount(route);
        float tScaled = Mathf.Clamp01(normalizedT) * segCount;

        segIndex = Mathf.FloorToInt(tScaled);
        if (segIndex >= segCount) { segIndex = segCount - 1; tScaled = segCount; }
        u = Mathf.Clamp01(tScaled - segIndex);
    }

    private void EvaluatePathAt(BezierRoute route, int segIndex, float u, out Vector3 position, out Vector3 tangent)
    {
        int baseIdx = segIndex * 3;
        Vector3 p0 = route.controlPoints[baseIdx + 0].position;
        Vector3 p1 = route.controlPoints[baseIdx + 1].position;
        Vector3 p2 = route.controlPoints[baseIdx + 2].position;
        Vector3 p3 = route.controlPoints[baseIdx + 3].position;

        position = CubicBezier(p0, p1, p2, p3, u);
        tangent = CubicBezierTangent(p0, p1, p2, p3, u);
    }

    private Quaternion EvaluateRotationAt(BezierRoute route, int segIndex, float u)
    {
        int baseIdx = segIndex * 3;
        Quaternion r0 = route.rotationControlPoints[baseIdx + 0].rotation;
        Quaternion r1 = route.rotationControlPoints[baseIdx + 1].rotation;
        Quaternion r2 = route.rotationControlPoints[baseIdx + 2].rotation;
        Quaternion r3 = route.rotationControlPoints[baseIdx + 3].rotation;

        return CubicQuaternionBezier(r0, r1, r2, r3, u);
    }

    private bool HasValidPath(BezierRoute route)
    {
        if (route == null) return false;
        if (route.controlPoints == null || route.controlPoints.Length < 4) return false;
        int n = route.controlPoints.Length;
        return ((n - 1) % 3) == 0;
    }

    private bool HasValidRotationPath(BezierRoute route)
    {
        if (route == null) return false;
        if (route.rotationControlPoints == null) return false;
        if (route.rotationControlPoints.Length != (route.controlPoints?.Length ?? 0)) return false;
        if (route.rotationControlPoints.Length < 4) return false;
        return ((route.rotationControlPoints.Length - 1) % 3) == 0;
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
        DrawRouteGizmos(routeAB, gizmoColorAB);
        DrawRouteGizmos(routeAC, gizmoColorAC);
    }

    private void DrawRouteGizmos(BezierRoute route, Color col)
    {
        if (!HasValidPath(route)) return;

        Gizmos.color = col;

        // Draw curve
        int steps = Mathf.Max(2, gizmoSteps);
        Vector3 prev;
        Vector3 prevTan;
        EvaluatePathAt(route, 0, 0f, out prev, out prevTan);
        for (int i = 1; i <= steps; i++)
        {
            float nt = i / (float)steps;
            int segIndex;
            float u;
            GetSegmentParams(route, nt, out segIndex, out u);
            Vector3 p, tan;
            EvaluatePathAt(route, segIndex, u, out p, out tan);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }

        // Control points
        Gizmos.color = new Color(col.r, col.g, col.b, 0.6f);
        foreach (var cp in route.controlPoints)
        {
            if (cp != null) Gizmos.DrawSphere(cp.position, 0.05f);
        }
    }

    private enum Endpoint { A, B, C }

    // NEW: keep looking at targets even when not tweening
    private void LateUpdate()
    {
        if (!Application.isPlaying) return;
        if (_currentRoute == null || !HasValidPath(_currentRoute)) return;

        if (!_didTweenUpdateThisFrame)
        {
            // Re-apply at current t so rotation tracks moving LookAt targets
            ApplyAt(_currentRoute, _t);
        }

        _didTweenUpdateThisFrame = false;
    }
}