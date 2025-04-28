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

    // --- Serialized Fields ---
    [Header("Path Following Settings")]
    [Tooltip("The parent object containing all path waypoints as children.")]
    public Transform path;
    [Tooltip("Distance threshold for reaching a waypoint.")]
    [SerializeField] private float waypointThreshold = 10.0f;
    [Tooltip("Number of points to calculate for Bezier curves (higher = smoother).")]
    [SerializeField] private float bezierCurveResolution = 10f;
    [Tooltip("Angle threshold for switching between straight lines and curves.")]
    [SerializeField] private float angleThreshold = 35.0f;

    [Header("Car Movement Settings")]
    [Tooltip("Maximum acceleration applied to the car.")]
    [SerializeField] private float maxAcceleration = 300.0f;
    [Tooltip("Braking acceleration.")]
    [SerializeField] private float maxSpeed = 100.0f;

    [Header("Steering Settings")]
    [Tooltip("Left turn radius (how far the front left wheel can rotate).")]
    [SerializeField] private float leftTurnRadius = 10.0f;
    [Tooltip("Right turn radius (how far the front right wheel can rotate).")]
    [SerializeField] private float rightTurnRadius = 30.0f;
    [Tooltip("Current turn sensitivity.")]
    [SerializeField] private float turnSensitivity = 30.0f;

    [Header("Physics Settings")]
    [Tooltip("Multiplier for gravity force.")]
    [SerializeField] private float gravityMultiplier = 1.5f;
    [Tooltip("Speed multiplier when on grass.")]
    [SerializeField] private float grassSpeedMultiplier = 0.5f;

    [Header("Corner Slowdown Settings")]
    [Tooltip("Minimum slowdown factor (0-1).")]
    [SerializeField] private float slowdownFactor = 0.2f;
    [Tooltip("Angle threshold for sharp corners.")]
    [SerializeField] private float cornerAngleThreshold = 60.0f;

    [Header("Turn Detection Settings")]
    [Tooltip("Radius of the detection sphere for upcoming turns.")]
    [SerializeField] private float detectionRadius = 7.0f;
    [Tooltip("Tolerance for deviation from the Bezier curve.")]
    [SerializeField] private float curveTolerance = 2.0f;

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
    [SerializeField] private bool showToleranceSphere = true; // NEW
    [Tooltip("Show min/max turn sensitivity arcs.")]
    [SerializeField] private bool showTurnSensitivityArcs = true; // NEW
    [Tooltip("Enable debug logs in the console.")]
    [SerializeField] private bool enableDebugLogs = false;

    [Tooltip("List of wheels used by the car.")]
    [SerializeField] private List<CarController.Wheel> wheels;
    [Tooltip("Rigidbody component of the car.")]
    [SerializeField] private Rigidbody carRb;

    // --- Private State ---
    private float moveInput;
    private float steerInput;
    private float targetTorque;

    private List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private List<Vector3> bezierCurvePoints = new List<Vector3>();
    private bool isFollowingBezierCurve = false;

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
        MoveAlongPath();
        SteerTowardsPath();
        ApplyGravity();
        AnimateWheels();
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
        LogDebug($"Bezier curve generated with {curveNodes.Count} nodes. Current node index: {currentWaypointIndex}");
    }

    List<Vector3> GenerateMultiPointBezierCurve(List<Vector3> points)
    {
        List<Vector3> curvePoints = new List<Vector3>();

        // Use a higher resolution for better accuracy
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
        // Recursive De Casteljau's algorithm for multi-point Bezier curves
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
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
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

        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = targetTorque;
            wheel.wheelCollider.brakeTorque = 0f;
        }

        ApplySpeedLimit(maxSpeed);
    }

    void SteerTowardsPath()
    {
        if (bezierCurvePoints.Count == 0) return;

        // Steer towards the next point in the Bezier curve
        Vector3 targetPoint = bezierCurvePoints[0];
        Vector3 directionToTarget = targetPoint - transform.position;
        Vector3 localDirection = transform.InverseTransformDirection(directionToTarget.normalized);

        // Apply a dead zone for very small steering adjustments
        steerInput = Mathf.Abs(localDirection.x) > STEERING_DEAD_ZONE ? Mathf.Clamp(localDirection.x, -1.0f, 1.0f) : 0.0f;
        steerInput = Mathf.Lerp(steerInput, Mathf.Clamp(localDirection.x, -1.0f, 1.0f), 0.1f);

        foreach (var wheel in wheels)
        {
            if (wheel.axel == CarController.Axel.Front)
            {
                float steerAngle = steerInput * turnSensitivity;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, steerAngle, STEERING_LERP); // increasing the float makes the car not overshoot constantly
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
                LogDebug($"Upcoming turn detected. Max angle: {maxAngle}, Slowdown factor: {slowdownFactor}, Target speed: {targetSpeed}");
            }
            else
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

        LogDebug($"Adjusting turning rate. Closest distance: {closestDistance}, Turn sensitivity: {turnSensitivity}");
    }

    void OnDrawGizmos()
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

#if UNITY_EDITOR
        // Draw the activation range for the next node
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(waypoints[currentWaypointIndex].position, Vector3.up, waypointThreshold);

        // Draw arcs for left and right turn radius (toggleable)
        if (showTurnSensitivityArcs)
        {
            Handles.color = Color.yellow;
            Handles.DrawWireArc(transform.position, Vector3.up, transform.forward, rightTurnRadius, 5.0f); // Right turn radius (right)
            Handles.DrawWireArc(transform.position, Vector3.up, transform.forward, -leftTurnRadius, 5.0f); // Left turn radius (left)
        }
#endif
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
        ApplySpeedLimit(maxSpeed);
    }
}
