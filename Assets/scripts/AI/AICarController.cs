using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AICarController : MonoBehaviour
{
    // --- Constants ---
    private const float GROUND_RAY_LENGTH = 0.5f;
    private const float STEERING_DEAD_ZONE = 0.05f;
    private const float STEERING_LERP = 0.6f;
    private const float NODE_GIZMO_RADIUS = 0.5f;
    private static readonly Vector3 DEFAULT_CENTER_OF_MASS = new Vector3(0, -0.3f, 0);

    // --- Path Following ---
    [Header("Path Following Settings")]
    [Tooltip("The parent object containing all path waypoints as children.")]
    public Transform path;
    [Tooltip("Distance threshold for reaching a waypoint.")]
    [SerializeField] private float waypointThreshold = 10.0f;
    [Tooltip("Number of points to calculate for Bezier curves (higher = smoother).")]
    [SerializeField] private float bezierCurveResolution = 10f;
    [Tooltip("Angle threshold for switching between straight lines and curves.")]
    [SerializeField] private float angleThreshold = 35.0f;

    // --- Car Movement ---
    [Header("Car Movement Settings")]
    [Tooltip("Maximum acceleration applied to the car.")]
    [SerializeField] private float maxAcceleration = 300.0f;
    [Tooltip("Braking acceleration.")]
    [SerializeField] private float maxSpeed = 100.0f;

    // --- Steering ---
    [Header("Steering Settings")]
    [Tooltip("Left turn radius (how far the front left wheel can rotate).")]
    [SerializeField] private float leftTurnRadius = 10.0f;
    [Tooltip("Right turn radius (how far the front right wheel can rotate).")]
    [SerializeField] private float rightTurnRadius = 30.0f;
    [Tooltip("Current turn sensitivity.")]
    [SerializeField] private float turnSensitivity = 30.0f;

    // --- Physics ---
    [Header("Physics Settings")]
    [Tooltip("Multiplier for gravity force.")]
    [SerializeField] private float gravityMultiplier = 1.5f;
    [Tooltip("Speed multiplier when on grass.")]
    [SerializeField] private float grassSpeedMultiplier = 0.5f;

    // --- Corner Slowdown ---
    [Header("Corner Slowdown Settings")]
    [Tooltip("Minimum slowdown factor (0-1).")]
    [SerializeField] private float slowdownFactor = 0.2f;
    [Tooltip("Angle threshold for sharp corners.")]
    [SerializeField] private float cornerAngleThreshold = 60.0f;

    // --- Turn Detection ---
    [Header("Turn Detection Settings")]
    [Tooltip("Radius of the detection sphere for upcoming turns.")]
    [SerializeField] private float detectionRadius = 7.0f;
    [Tooltip("Tolerance for deviation from the Bezier curve.")]
    [SerializeField] private float curveTolerance = 2.0f;

    // --- Avoidance ---
    [Header("Avoidance Settings")]
    [Tooltip("Extra buffer distance added to the safe radius for avoidance checks.")]
    [SerializeField] private float avoidanceBuffer = 2.0f;
    [Tooltip("How far to offset laterally when dodging another car.")]
    [SerializeField] private float avoidanceLateralOffset = 2.0f;

    // --- Waypoint Debug ---
    [Header("Waypoint Debug Settings")]
    [Tooltip("Show a horizontal (local X axis) line at each waypoint.")]
    [SerializeField] private bool showWaypointHorizontalLine = false;
    [Tooltip("Length of the horizontal line at each waypoint.")]
    [SerializeField] private float waypointLineLength = 5.0f;
    [Tooltip("Width of the horizontal line at each waypoint.")]
    [SerializeField] private float waypointLineWidth = 0.1f;

    // --- Debug ---
    [Header("Debug Settings")]
    [Tooltip("Show debug gizmos in the scene view.")]
    [SerializeField] private bool showDebugGizmos = true;
    [Tooltip("Show the detection sphere.")]
    [SerializeField] private bool showDetectionSphere = true;
    [Tooltip("Show the farthest point debug line.")]
    [SerializeField] private bool showFarthestPointLine = true;
    [Tooltip("Show the closest point debug line.")]
    [SerializeField] private bool showClosestPointLine = true;
    [Tooltip("Show the curve tolerance sphere.")]
    [SerializeField] private bool showToleranceSphere = true;
    [Tooltip("Show min/max turn sensitivity arcs.")]
    [SerializeField] private bool showTurnSensitivityArcs = true;
    [Tooltip("Enable debug logs in the console.")]
    [SerializeField] private bool enableDebugLogs = false;
    [Tooltip("Show the avoidance buffer radius as a magenta wire sphere.")]
    [SerializeField] private bool showAvoidanceBuffer = true;

    // --- References ---
    [Header("References")]
    [Tooltip("List of wheels used by the car.")]
    [SerializeField] private List<CarController.Wheel> wheels;
    [Tooltip("Rigidbody component of the car.")]
    [SerializeField] private Rigidbody carRb;
    [Tooltip("Reference to the player car.")]
    [SerializeField] private CarController playerCar;

    // --- Private State ---
    private float playerCarWidth = 2.0f; // fallback default
    private float playerCarLength = 4.0f; // fallback default
    private List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private List<Vector3> bezierCurvePoints = new List<Vector3>();
    private bool isFollowingBezierCurve = false;
    private Collider carCollider;
    private float carWidth = 2.0f; // fallback default
    private float carLength = 4.0f; // fallback default
    private float avoidanceOffset = 0f;
    private float moveInput;
    private float steerInput;
    private float targetTorque;
    private bool isBoosting = false;
    private float boostMultiplier = 1.25f;
    private int boostEndWaypointIndex = -1;
    private float boostTimer = 0f;
    private const float maxBoostDuration = 2f;

    // --- Public Properties ---
    public List<Transform> WaypointsPublic => waypoints;
    public int CurrentWaypointIndex => currentWaypointIndex;
    public float CarWidth => carWidth;
    public float CarLength => carLength;

    // --- Static ---
    [HideInInspector] public string carName = "AI Car"; //gets replaced at runtime
    [HideInInspector] public float assignedMaxSpeed;  //WIP - will be set by AIManager
    [HideInInspector] public float assignedTurnSensitivity; //WIP - will be set by AIManager
    [HideInInspector] public int currentPlacement = 1; //updates at runtime
    public static List<AICarController> AllAICars = new List<AICarController>();

    private void OnEnable() => AllAICars.Add(this);
    private void OnDisable() => AllAICars.Remove(this);

    private void Start()
    {
        if (path == null)
        {
            Debug.LogError("Path transform is not assigned.");
            enabled = false;
            return;
        }

        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        waypoints = new List<Transform>();
        foreach (var t in pathTransforms)
        {
            if (t != path)
                waypoints.Add(t);
        }

        if (carRb == null)
            carRb = GetComponent<Rigidbody>();

        carRb.centerOfMass = DEFAULT_CENTER_OF_MASS;
        UpdateBezierCurve();

        carCollider = GetComponent<Collider>();
        if (carCollider != null)
        {
            carWidth = carCollider.bounds.size.x;
            carLength = carCollider.bounds.size.z;
        }

        // Find the player car if not assigned
        if (playerCar == null)
            playerCar = FindFirstObjectByType<CarController>();
        if (playerCar != null && playerCar.carRb != null)
        {
            var playerCollider = playerCar.GetComponent<Collider>();
            if (playerCollider != null) {
                playerCarWidth = playerCollider.bounds.size.x;
                playerCarLength = playerCollider.bounds.size.z;
            }
        }

        // Find the player car from GameManager
        if (playerCar == null)
        {
            var gm = GameManager.instance;
            if (gm != null && gm.currentCar != null)
            {
                playerCar = gm.currentCar.GetComponent<CarController>();
                var playerCollider = gm.currentCar.GetComponent<Collider>();
                if (playerCollider != null) {
                    playerCarWidth = playerCollider.bounds.size.x;
                    playerCarLength = playerCollider.bounds.size.z;
                }
            }
        }
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log(message);
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        CheckUpcomingTurn();
        EnforceMaxSpeed();
        AvoidOtherCars();
        MoveAlongPath();
        SteerTowardsPath();
        ApplyGravity();
        AnimateWheels();
        HandleBoostTimer();
    }

    private void ApplyGravity()
    {
        if (!IsGrounded())
        {
            carRb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.magnitude, ForceMode.Acceleration);
        }
    }

    private void ApplySpeedLimit(float targetSpeed)
    {
        float currentSpeed = carRb.linearVelocity.magnitude * 3.6f; // Convert to km/h
        if (currentSpeed > targetSpeed)
        {
            carRb.linearVelocity = carRb.linearVelocity.normalized * (targetSpeed / 3.6f);
            LogDebug($"Clamping speed to target speed: {targetSpeed} km/h");
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, GROUND_RAY_LENGTH);
    }

    private bool IsOnGrass()
    {
        if (wheels == null) return false;
        foreach (var wheel in wheels)
        {
            if (Physics.Raycast(wheel.wheelCollider.transform.position, -wheel.wheelCollider.transform.up, out RaycastHit hit, wheel.wheelCollider.radius + wheel.wheelCollider.suspensionDistance))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Grass"))
                    return true;
            }
        }
        return false;
    }

    private void AnimateWheels()
    {
        if (wheels == null) return;
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void UpdateBezierCurve()
    {
        bezierCurvePoints.Clear();

        // Ensure there are enough nodes to calculate a curve
        if (waypoints.Count < 3)
        {
            LogDebug("Not enough nodes to calculate a Bezier curve.");
            return;
        }

        // Start grouping nodes for a single Bezier curve
        List<Vector3> curveNodes = new List<Vector3>();
        curveNodes.Add(waypoints[currentWaypointIndex].position); // Add the current node

        int index = currentWaypointIndex;
        while (true)
        {
            // Get the next and next-next nodes
            Vector3 currentNode = waypoints[index].position;
            Vector3 nextNode = waypoints[(index + 1) % waypoints.Count].position;
            Vector3 nextNextNode = waypoints[(index + 2) % waypoints.Count].position;

            // Calculate the angle between the current, next, and next-next nodes
            Vector3 directionToNext = (nextNode - currentNode).normalized;
            Vector3 directionToNextNext = (nextNextNode - nextNode).normalized;
            float angle = Vector3.Angle(directionToNext, directionToNextNext);

            if (angle > angleThreshold)
            {
                // Add the next node to the curve
                curveNodes.Add(nextNode);
                index = (index + 1) % waypoints.Count;
            }
            else
            {
                // Add the next node and stop grouping
                curveNodes.Add(nextNode);
                index = (index + 1) % waypoints.Count;
                break;
            }

            // Stop if looped through all nodes
            if (index == currentWaypointIndex)
            {
                LogDebug("Looped through all nodes without finding a straight section.");
                break;
            }
        }

        // Generate a Bezier curve using the grouped nodes
        bezierCurvePoints = GenerateMultiPointBezierCurve(curveNodes);

        // Update the current node index to the node after the curve
        currentWaypointIndex = index;
        isFollowingBezierCurve = true; // Enter Bezier curve mode
    }

    List<Vector3> GenerateMultiPointBezierCurve(List<Vector3> points)
    {
        List<Vector3> curvePoints = new List<Vector3>();

        // Using a high resolution may cause issues with navigation
        int resolution = Mathf.CeilToInt(bezierCurveResolution * points.Count * 2); // Double the resolution
        for (float t = 0; t <= 1; t += 1.0f / resolution)
        {
            curvePoints.Add(CalculateBezierPoint(points, t));
        }

        // Ensure the final node is included in the curve
        if (curvePoints.Count == 0 || curvePoints[curvePoints.Count - 1] != points[points.Count - 1])
        {
            curvePoints.Add(points[points.Count - 1]);
        }

        return curvePoints;
    }

    Vector3 CalculateBezierPoint(List<Vector3> points, float t)
    {
        // Recursive De Casteljau's algorithm for multi-point Bezier curves. given by copilot
        if (points.Count == 1)
            return points[0];

        List<Vector3> nextPoints = new List<Vector3>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            // Use weighted interpolation to keep the curve closer to the nodes
            nextPoints.Add(Vector3.Lerp(points[i], points[i + 1], t));
        }

        return CalculateBezierPoint(nextPoints, t);
    }

    void MoveAlongPath()
    {
        if (isFollowingBezierCurve)
        {
            HandleBezierCurveFollowing();
        }
        else
        {
            HandleWaypointFollowing();
        }
    }

    private void HandleBezierCurveFollowing()
    {
        if (bezierCurvePoints.Count == 0)
        {
            isFollowingBezierCurve = false;
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            UpdateBezierCurve();
            return;
        }

        Vector3 targetPoint = bezierCurvePoints[0];
        Vector3 directionToTarget = targetPoint - transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget < curveTolerance)
        {
            bezierCurvePoints.RemoveAt(0);
        }

        ApplyDriveInputs();
    }

    private void HandleWaypointFollowing()
    {
        Vector3 currentNode = waypoints[currentWaypointIndex].position;
        Vector3 directionToNode = currentNode - transform.position;
        float distanceToNode = directionToNode.magnitude;

        if (distanceToNode < waypointThreshold)
        {
            // Stop boosting if reached the boost end node
            if (isBoosting && currentWaypointIndex == boostEndWaypointIndex)
            {
                isBoosting = false;
                boostEndWaypointIndex = -1;
                boostTimer = 0f;
                LogDebug($"{carName} boost ended at node {currentWaypointIndex}.");
            }

            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            TryBoostOnStraight();
            UpdateBezierCurve();
            return;
        }

        ApplyDriveInputs();
    }

    private void ApplyDriveInputs()
    {
        moveInput = 1.0f;
        targetTorque = moveInput * maxAcceleration;

        if (Mathf.Abs(steerInput) > 0.5f)
        {
            targetTorque *= 0.5f;
        }

        if (IsOnGrass())
        {
            targetTorque *= grassSpeedMultiplier;
        }

        // Apply boost if active
        float speedLimit = maxSpeed;
        if (isBoosting)
        {
            speedLimit = (maxSpeed * boostMultiplier) + 20f; // Add flat +20
            targetTorque *= boostMultiplier;
        }

        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = targetTorque;
            wheel.wheelCollider.brakeTorque = 0f;
        }

        ApplySpeedLimit(speedLimit);
    }

    void SteerTowardsPath()
    {
        if (bezierCurvePoints.Count == 0) return;

        // Steer towards the next point in the Bezier curve, with avoidance offset
        Vector3 targetPoint = bezierCurvePoints[0];

        // Apply lateral offset in local space
        Vector3 localTarget = transform.InverseTransformPoint(targetPoint);
        localTarget.x += avoidanceOffset;
        targetPoint = transform.TransformPoint(localTarget);

        Vector3 directionToTarget = targetPoint - transform.position;
        Vector3 localDirection = transform.InverseTransformDirection(directionToTarget.normalized);

        steerInput = Mathf.Abs(localDirection.x) > STEERING_DEAD_ZONE ? Mathf.Clamp(localDirection.x, -1.0f, 1.0f) : 0.0f;
        steerInput = Mathf.Lerp(steerInput, Mathf.Clamp(localDirection.x, -1.0f, 1.0f), 0.1f);

        foreach (var wheel in wheels)
        {
            if (wheel.axel == CarController.Axel.Front)
            {
                float steerAngle = steerInput * turnSensitivity;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, steerAngle, STEERING_LERP);
            }
        }
    }

    void CheckUpcomingTurn()
    {
        FindFarthestAndClosestBezierPoints(out Vector3 farthestPoint, out float farthestDistance, out Vector3 closestPoint, out float closestDistance);
        AdjustSpeedForUpcomingTurn(farthestPoint, farthestDistance);
        AdjustTurningForUpcomingTurn(closestDistance);
    }

    private void FindFarthestAndClosestBezierPoints(out Vector3 farthestPoint, out float farthestDistance, out Vector3 closestPoint, out float closestDistance)
    {
        farthestPoint = Vector3.zero;
        farthestDistance = 0.0f;
        closestPoint = Vector3.zero;
        closestDistance = float.MaxValue;

        foreach (var point in bezierCurvePoints)
        {
            float distance = Vector3.Distance(transform.position, point);

            if (distance <= detectionRadius && distance > farthestDistance)
            {
                farthestPoint = point;
                farthestDistance = distance;
            }

            if (distance <= detectionRadius && distance < closestDistance)
            {
                closestPoint = point;
                closestDistance = distance;
            }
        }
    }

    private void AdjustSpeedForUpcomingTurn(Vector3 farthestPoint, float farthestDistance)
    {
        if (farthestDistance > 0.0f)
        {
            Vector3 directionToPoint = (farthestPoint - transform.position).normalized;
            float maxAngle = Vector3.Angle(transform.forward, directionToPoint);

            if (maxAngle > angleThreshold)
            {
                float normalizedAngle = Mathf.Clamp01((maxAngle - angleThreshold) / (180.0f - angleThreshold));
                float slowdownFactor = Mathf.Sin(normalizedAngle * Mathf.PI * 0.5f);
                float targetSpeed = Mathf.Lerp(maxSpeed * 0.3f, maxSpeed, 1.0f - slowdownFactor);

                ApplySpeedLimit(targetSpeed);
            {
                // Restore max speed if no sharp turn is detected
                ApplySpeedLimit(maxSpeed);
            }
        }
        else
        {
            // No points within detection radius, restore max speed
            ApplySpeedLimit(maxSpeed);
        }
    }
    }

    private void AdjustTurningForUpcomingTurn(float closestDistance)
    {
        if (closestDistance < float.MaxValue)
        {
            AdjustTurningRate(closestDistance);
        }
    }

    void AdjustTurningRate(float closestDistance)
    {
        // Scale the turning sensitivity based on the distance
        turnSensitivity = Mathf.Lerp(rightTurnRadius, leftTurnRadius, closestDistance / detectionRadius);

        // Clamp the turning sensitivity to ensure it stays within the defined range
        turnSensitivity = Mathf.Clamp(turnSensitivity, leftTurnRadius, rightTurnRadius);
    }

    private void TryBoostOnStraight()
    {
        isBoosting = false;
        boostEndWaypointIndex = -1;

        if (waypoints.Count < 4) return;

        int idx0 = currentWaypointIndex % waypoints.Count;
        int idx1 = (currentWaypointIndex + 1) % waypoints.Count;
        int idx2 = (currentWaypointIndex + 2) % waypoints.Count;
        int idx3 = (currentWaypointIndex + 3) % waypoints.Count;

        Vector3 p0 = waypoints[idx0].position;
        Vector3 p1 = waypoints[idx1].position;
        Vector3 p2 = waypoints[idx2].position;
        Vector3 p3 = waypoints[idx3].position;

        float angle1 = Vector3.Angle((p1 - p0).normalized, (p2 - p1).normalized);
        float angle2 = Vector3.Angle((p2 - p1).normalized, (p3 - p2).normalized);

        float threshold = 6f;

        if (angle1 <= threshold && angle2 <= threshold)
        {
            if (Random.value < 0.4f)
            {
                isBoosting = true;
                boostEndWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                boostTimer = maxBoostDuration; // Start the timer
                LogDebug($"BOOST ACTIVATED! {carName} will boost from node {currentWaypointIndex} to {boostEndWaypointIndex}. Angles: {angle1:F2}, {angle2:F2}");

                // --- Instant velocity boost ---
                Vector3 flatForward = transform.forward;
                flatForward.y = 0f;
                flatForward.Normalize();
                carRb.linearVelocity += flatForward * 20f;
            }
            else
            {
                LogDebug($"{carName} found a straight section but did not boost this time. Angles: {angle1:F2}, {angle2:F2}");
            }
        }
        else
        {
            LogDebug($"{carName} did not find a straight section for boost. Angles: {angle1:F2}, {angle2:F2}");
        }
    }

    private void HandleBoostTimer()
    {
        if (isBoosting)
        {
            boostTimer -= Time.fixedDeltaTime;
            if (boostTimer <= 0f)
            {
                isBoosting = false;
                boostEndWaypointIndex = -1;
                boostTimer = 0f;
                LogDebug($"{carName} boost ended due to timeout.");
            }
        }
    }

    private void AvoidOtherCars()
    {
        avoidanceOffset = 0f; // Reset each frame

        float mySafeRadius = Mathf.Max(this.CarWidth, this.CarLength) * 0.5f;

        foreach (var other in AllAICars)
        {
            if (other == this) continue;

            Vector3 toOther = other.transform.position - transform.position;
            float distance = toOther.magnitude;
            float otherSafeRadius = Mathf.Max(other.CarWidth, other.CarLength) * 0.5f;
            float minSafeDistance = mySafeRadius + otherSafeRadius + avoidanceBuffer;

            if (distance < minSafeDistance && Vector3.Dot(transform.forward, toOther.normalized) > 0.5f)
            {
                Vector3 myFuturePos = transform.position + carRb.linearVelocity * 0.5f;
                Vector3 otherFuturePos = other.transform.position + other.carRb.linearVelocity * 0.5f;
                float futureDist = (myFuturePos - otherFuturePos).magnitude;

                if (futureDist < minSafeDistance)
                {
                    float steerDirection = Vector3.Cross(transform.forward, toOther).y > 0 ? -1f : 1f;
                    avoidanceOffset += steerDirection * avoidanceLateralOffset;

                    if (distance < minSafeDistance * 0.5f && carRb.linearVelocity.magnitude > other.carRb.linearVelocity.magnitude)
                        moveInput = 0.7f;
                }
            }
        }

        // --- Player car avoidance ---
        if (playerCar != null && playerCar.carRb != null && playerCar != this)
        {
            Vector3 toPlayer = playerCar.transform.position - transform.position;
            float distance = toPlayer.magnitude;
            float playerSafeRadius = Mathf.Max(playerCarWidth, playerCarLength) * 0.5f;
            float minSafeDistance = mySafeRadius + playerSafeRadius + avoidanceBuffer;

            if (distance < minSafeDistance && Vector3.Dot(transform.forward, toPlayer.normalized) > 0.5f)
            {
                Vector3 myFuturePos = transform.position + carRb.linearVelocity * 0.5f;
                Vector3 playerFuturePos = playerCar.transform.position + playerCar.carRb.linearVelocity * 0.5f;
                float futureDist = (myFuturePos - playerFuturePos).magnitude;

                if (futureDist < minSafeDistance)
                {
                    float steerDirection = Vector3.Cross(transform.forward, toPlayer).y > 0 ? -1f : 1f;
                    avoidanceOffset += steerDirection * avoidanceLateralOffset;

                    if (distance < minSafeDistance * 0.5f && carRb.linearVelocity.magnitude > playerCar.carRb.linearVelocity.magnitude)
                        moveInput = 0.7f;
                }
            }
        }
    }

    Vector3 FindClosestPointOnCurve(Vector3 nodePosition)
    {
        Vector3 closestPoint = bezierCurvePoints[0];
        float closestDistance = Vector3.Distance(nodePosition, closestPoint);

        foreach (var point in bezierCurvePoints)
        {
            float distance = Vector3.Distance(nodePosition, point);
            if (distance < closestDistance)
            {
                closestPoint = point;
                closestDistance = distance;
            }
        }

        return closestPoint;
    }

    void EnforceMaxSpeed()
    {
        float speedLimit = maxSpeed;
        if (isBoosting)
            speedLimit = (maxSpeed * boostMultiplier) + 20f; // Add flat +20 for immediate effect

        ApplySpeedLimit(speedLimit);
    }

#if UNITY_EDITOR // every visual debugging tool
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || waypoints == null || waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count) return;

        // Draw the Bezier curve or straight line
        Gizmos.color = Color.blue;
        for (int i = 0; i < bezierCurvePoints.Count - 1; i++)
        {
            Gizmos.DrawLine(bezierCurvePoints[i], bezierCurvePoints[i + 1]);
        }

        // Draw spheres for all nodes
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (i == currentWaypointIndex)
            {
                Gizmos.color = Color.green; // Current node
            }
            else if (i > currentWaypointIndex && i <= (currentWaypointIndex + 2) % waypoints.Count)
            {
                Gizmos.color = Color.yellow; // Nodes skipped by the Bezier curve
            }
            else
            {
                Gizmos.color = Color.red; // Other nodes
            }

            Gizmos.DrawSphere(waypoints[i].position, NODE_GIZMO_RADIUS);

            // Draw a horizontal line (with path direction) at each waypoint
            if (showWaypointHorizontalLine)
            {
                Transform wp = waypoints[i];
                Vector3 center = wp.position;

                // Use direction to next waypoint for orientation
                Vector3 dir;
                if (i < waypoints.Count - 1)
                    dir = (waypoints[i + 1].position - wp.position).normalized;
                else
                    dir = (waypoints[0].position - wp.position).normalized;

                // Get a vector perpendicular to the path direction (horizontal, in XZ plane)
                Vector3 up = Vector3.up;
                Vector3 right = Vector3.Cross(up, dir).normalized;

                Vector3 start = center - right * (waypointLineLength * 0.5f);
                Vector3 end = center + right * (waypointLineLength * 0.5f);

                // Draw a thicker line using Handles if available
#if UNITY_EDITOR
                Color prevColor = Handles.color;
                Handles.color = Color.white;
                Handles.DrawAAPolyLine(waypointLineWidth, new Vector3[] { start, end });
                Handles.color = prevColor;
#else
                Gizmos.color = Color.white;
                Gizmos.DrawLine(start, end);
#endif
            }

            // Draw a line from the node to the closest point on the curve
            if (bezierCurvePoints.Count > 0 && showClosestPointLine)
            {
                Vector3 closestPoint = FindClosestPointOnCurve(waypoints[i].position);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(waypoints[i].position, closestPoint);
            }
        }

        // Draw the detection sphere for upcoming turns
        if (showDetectionSphere)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f); // Semi-transparent red
            Gizmos.DrawSphere(transform.position, detectionRadius);
        }

        // Draw the curve tolerance sphere (toggleable)
        if (showToleranceSphere)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green
            Gizmos.DrawSphere(transform.position, curveTolerance);
        }

        // Draw a debug line to the farthest point within the detection radius
        if (showFarthestPointLine)
        {
            Vector3 farthestPoint = Vector3.zero;
            float farthestDistance = 0.0f;

            foreach (var point in bezierCurvePoints)
            {
                float distance = Vector3.Distance(transform.position, point);
                if (distance <= detectionRadius && distance > farthestDistance)
                {
                    farthestPoint = point;
                    farthestDistance = distance;
                }
            }

            if (farthestDistance > 0.0f)
            {
                Gizmos.color = Color.cyan; // Cyan line for the farthest point
                Gizmos.DrawLine(transform.position, farthestPoint);
            }
        }

        // Draw a label above the car in the Scene view with placement and name
        Handles.Label(transform.position + Vector3.up * 2.5f, $"{currentPlacement}. {carName}");

        // Visualize avoidance detection
        Gizmos.color = Color.yellow;
        float maxAvoidRadius = 0f;
        foreach (var other in AllAICars)
        {
            if (other == this) continue;
            float minSafeDistance = (this.CarWidth + other.CarWidth) * 0.5f + 2.0f;
            if (minSafeDistance > maxAvoidRadius) maxAvoidRadius = minSafeDistance;

            Vector3 toOther = other.transform.position - transform.position;
            float distance = toOther.magnitude;
            if (distance < minSafeDistance && Vector3.Dot(transform.forward, toOther.normalized) > 0.5f)
            {
                // Draw a line to the car being avoided
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, other.transform.position + Vector3.up * 0.5f);
            }
        }
        // Draw the largest avoidance radius for this car
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, maxAvoidRadius);

        // Visualize player avoidance
        if (playerCar != null && playerCar != this)
        {
            float minSafeDistance = (this.CarWidth + playerCarWidth) * 0.5f + 2.0f;
            Vector3 toPlayer = playerCar.transform.position - transform.position;
            float distance = toPlayer.magnitude;
            if (distance < minSafeDistance && Vector3.Dot(transform.forward, toPlayer.normalized) > 0.5f)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, playerCar.transform.position + Vector3.up * 0.5f);
            }
        }

        // Visualize avoidance buffer as a magenta wire sphere
        if (showAvoidanceBuffer)
        {
            float mySafeRadius = Mathf.Max(this.CarWidth, this.CarLength) * 0.5f;
            float bufferRadius = mySafeRadius + avoidanceBuffer;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, bufferRadius);
        }
    }
#endif
}