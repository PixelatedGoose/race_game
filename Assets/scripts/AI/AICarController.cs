using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor; // Required for Handles and editor-specific functionality
#endif

public class AICarController : MonoBehaviour
{
    public Transform path; // Reference to the path object

    [Header("Path Following Settings")]
    public float waypointThreshold = 10.0f; // Increased from 5.0f for more leniency
    [SerializeField] private float bezierCurveResolution = 10; // Number of points to calculate for Bezier curves
    [SerializeField] private float angleThreshold = 35.0f; // Threshold for switching between straight lines and curves

    [Header("Car Movement Settings")]
    public float maxAcceleration = 300.0f;
    public float brakeAcceleration = 3.0f;
    public float turnSensitivity = 30.0f;
    public float maxSpeed = 100.0f;

    [Header("Physics Settings")]
    public float gravityMultiplier = 1.5f;
    public float grassSpeedMultiplier = 0.5f;

    [Header("Corner Slowdown Settings")]
    public float slowdownFactor = 0.2f; // Minimum slowdown factor (20%)
    public float cornerAngleThreshold = 60.0f; // Angle threshold for sharp corners

    public List<CarController.Wheel> wheels; // Use the same wheel structure as CarController
    public Rigidbody carRb;

    private float moveInput;
    private float steerInput;
    private float targetTorque;

    private List<Transform> nodes;
    private int currentNodeIndex = 0;

    private List<Vector3> bezierPoints = new List<Vector3>(); // Points for the current Bezier curve
    
    private bool isFollowingBezierCurve = false; // Tracks if the AI is currently following a Bezier curve

    void Start()
    {
        // Initialize waypoints from AiPath
        Transform[] pathTransform = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        foreach (var t in pathTransform)
        {
            if (t != path) // Exclude the parent object (path)
            {
                nodes.Add(t);
            }
        }

        if (carRb == null)
        {
            carRb = GetComponent<Rigidbody>();
        }

         // Lower the center of mass
        carRb.centerOfMass = new Vector3(0, -0.5f, 0); // Adjust the Y value as needed

        // Generate the initial Bezier curve
        UpdateBezierCurve();
    }

    void FixedUpdate()
    {
        if (nodes.Count == 0) return;

        MoveAlongPath();
        SteerTowardsPath();
        ApplySpeedLimit();
        ApplyGravity();
        Animatewheels();
    }

    void ApplyGravity()
    {
        if (!IsGrounded())
        {
            carRb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.magnitude, ForceMode.Acceleration);
        }
    }

    void ApplySpeedLimit()
    {
        float speed = carRb.linearVelocity.magnitude * 3.6f; // Convert to km/h

        // Cap speed at 70% of max speed if on a Bezier curve
        if (isFollowingBezierCurve)
        {
            float maxAllowedSpeed = maxSpeed * 0.7f; // 70% of max speed
            if (speed > maxAllowedSpeed)
            {
                speed = maxAllowedSpeed;
                Debug.Log($"Capping speed to 70% of max speed while on Bezier curve: {speed} km/h");
            }
        }

        // Apply the adjusted speed
        carRb.linearVelocity = carRb.linearVelocity.normalized * (speed / 3.6f);
    }

    float GetCornerSlowdownFactor()
    {
        float lookaheadSlowdownFactor = GetLookaheadSlowdownFactor();

        if (isFollowingBezierCurve && bezierPoints.Count > 2)
        {
            // Calculate the maximum angle between consecutive segments of the Bezier curve
            float maxAngle = 0f;
            for (int i = 0; i < bezierPoints.Count - 2; i++)
            {
                Vector3 direction1 = (bezierPoints[i + 1] - bezierPoints[i]).normalized;
                Vector3 direction2 = (bezierPoints[i + 2] - bezierPoints[i + 1]).normalized;
                float angle = Vector3.Angle(direction1, direction2);
                maxAngle = Mathf.Max(maxAngle, angle);
            }

            Debug.Log($"Maximum angle on Bezier curve: {maxAngle} degrees.");

            // Calculate slowdown factor based on the maximum angle
            float slowdownFactor = CalculateSlowdownFactor(maxAngle);

            // Combine with lookahead slowdown factor
            slowdownFactor = Mathf.Min(slowdownFactor, lookaheadSlowdownFactor);

            // Ensure a minimum speed by clamping the slowdown factor
            slowdownFactor = Mathf.Max(slowdownFactor, 0.5f); // Minimum slowdown factor is 50% of max speed
            Debug.Log($"Final slowdown factor for Bezier curve: {slowdownFactor}");

            return slowdownFactor;
        }
        else if (nodes.Count >= 3)
        {
            // Fallback to normal node-based slowdown logic
            Vector3 currentNode = nodes[currentNodeIndex].position;
            Vector3 nextNode = nodes[(currentNodeIndex + 1) % nodes.Count].position;
            Vector3 nextNextNode = nodes[(currentNodeIndex + 2) % nodes.Count].position;

            Vector3 directionToNext = (nextNode - currentNode).normalized;
            Vector3 directionToNextNext = (nextNextNode - nextNode).normalized;
            float currentAngle = Vector3.Angle(directionToNext, directionToNextNext);

            Debug.Log($"Current corner angle: {currentAngle} degrees.");

            float slowdownFactor = CalculateSlowdownFactor(currentAngle);

            // Combine with lookahead slowdown factor
            slowdownFactor = Mathf.Min(slowdownFactor, lookaheadSlowdownFactor);

            // Ensure a minimum speed by clamping the slowdown factor
            slowdownFactor = Mathf.Max(slowdownFactor, 0.5f); // Minimum slowdown factor is 50% of max speed
            Debug.Log($"Final slowdown factor for normal corner: {slowdownFactor}");

            return slowdownFactor;
        }

        return 1.0f; // No slowdown if there aren't enough points
    }

    float CalculateSlowdownFactor(float angle)
    {
        if (angle < cornerAngleThreshold) return 1.0f; // No slowdown for angles below the threshold

        // Increase sensitivity for sharper angles
        return Mathf.Clamp(1.0f - Mathf.Pow((angle - cornerAngleThreshold) / (180.0f - cornerAngleThreshold), 1.5f), slowdownFactor, 1.0f);
    }

    float GetLookaheadSlowdownFactor()
    {
        if (nodes.Count < 3) return 1.0f; // No slowdown if there aren't enough nodes

        // Increase the number of nodes to look ahead
        int lookaheadSteps = 5; // Increased from 3 to 5
        float maxAngle = 0f;

        for (int i = 0; i < lookaheadSteps - 2; i++)
        {
            int currentIndex = (currentNodeIndex + i) % nodes.Count;
            int nextIndex = (currentNodeIndex + i + 1) % nodes.Count;
            int nextNextIndex = (currentNodeIndex + i + 2) % nodes.Count;

            Vector3 currentNode = nodes[currentIndex].position;
            Vector3 nextNode = nodes[nextIndex].position;
            Vector3 nextNextNode = nodes[nextNextIndex].position;

            Vector3 directionToNext = (nextNode - currentNode).normalized;
            Vector3 directionToNextNext = (nextNextNode - nextNode).normalized;

            float angle = Vector3.Angle(directionToNext, directionToNextNext);
            maxAngle = Mathf.Max(maxAngle, angle);
        }

        Debug.Log($"Lookahead maximum angle: {maxAngle} degrees.");

        // Calculate slowdown factor based on the maximum angle
        float slowdownFactor = CalculateSlowdownFactor(maxAngle);

        // Ensure a minimum speed by clamping the slowdown factor
        slowdownFactor = Mathf.Max(slowdownFactor, 0.3f); // Minimum slowdown factor is 30% of max speed
        Debug.Log($"Lookahead slowdown factor: {slowdownFactor}");

        return slowdownFactor;
    }

    bool IsInSharpCorner()
    {
        if (nodes.Count < 3) return false;

        // Get the current, next, and next-next nodes
        Vector3 currentNode = nodes[currentNodeIndex].position;
        Vector3 nextNode = nodes[(currentNodeIndex + 1) % nodes.Count].position;
        Vector3 nextNextNode = nodes[(currentNodeIndex + 2) % nodes.Count].position;

        // Calculate the angle between the current, next, and next-next nodes
        Vector3 directionToNext = (nextNode - currentNode).normalized;
        Vector3 directionToNextNext = (nextNextNode - nextNode).normalized;
        float angle = Vector3.Angle(directionToNext, directionToNextNext);
        return angle > cornerAngleThreshold;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.5f);
    }

    bool IsOnGrass()
    {
        foreach (var wheel in wheels)
        {
            if (Physics.Raycast(wheel.wheelCollider.transform.position, -wheel.wheelCollider.transform.up, out RaycastHit hit, wheel.wheelCollider.radius + wheel.wheelCollider.suspensionDistance))
            {
                return hit.collider.CompareTag("Grass");
            }
        }
        return false;
    }

    void Animatewheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void UpdateBezierCurve()
    {
        bezierPoints.Clear();

        // Ensure there are enough nodes to calculate a curve
        if (nodes.Count < 3)
        {
            Debug.LogWarning("Not enough nodes to calculate a Bezier curve.");
            return;
        }

        // Start grouping nodes for a single Bezier curve
        List<Vector3> curveNodes = new List<Vector3>();
        curveNodes.Add(nodes[currentNodeIndex].position); // Add the current node

        int index = currentNodeIndex;
        while (true)
        {
            // Get the next and next-next nodes
            Vector3 currentNode = nodes[index].position;
            Vector3 nextNode = nodes[(index + 1) % nodes.Count].position;
            Vector3 nextNextNode = nodes[(index + 2) % nodes.Count].position;

            // Calculate the angle between the current, next, and next-next nodes
            Vector3 directionToNext = (nextNode - currentNode).normalized;
            Vector3 directionToNextNext = (nextNextNode - nextNode).normalized;
            float angle = Vector3.Angle(directionToNext, directionToNextNext);

            if (angle > angleThreshold)
            {
                // Add the next node to the curve
                curveNodes.Add(nextNode);
                index = (index + 1) % nodes.Count;
            }
            else
            {
                // Add the next node and stop grouping
                curveNodes.Add(nextNode);
                index = (index + 1) % nodes.Count;
                break;
            }

            // Stop if we've looped through all nodes
            if (index == currentNodeIndex)
            {
                Debug.LogWarning("Looped through all nodes without finding a straight section.");
                break;
            }
        }

        // Generate a Bezier curve using the grouped nodes
        bezierPoints = GenerateMultiPointBezierCurve(curveNodes);

        // Update the current node index to the node after the curve
        currentNodeIndex = index;
        isFollowingBezierCurve = true; // Enter Bezier curve mode
        Debug.Log($"Bezier curve generated with {curveNodes.Count} nodes. Current node index: {currentNodeIndex}");
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
        Debug.Log($"Current Node Index: {currentNodeIndex}, Node Position: {nodes[currentNodeIndex].position}");

        if (isFollowingBezierCurve)
        {
            // Handle Bezier curve logic
            if (bezierPoints.Count == 0)
            {
                // If there are no more points in the current curve, move to the next node
                isFollowingBezierCurve = false; // Exit Bezier curve mode
                currentNodeIndex = (currentNodeIndex + 1) % nodes.Count; // Advance to the next node

                // Check if the next node starts a new Bezier curve
                UpdateBezierCurve();
                return;
            }

            // Move towards the next point in the Bezier curve
            Vector3 targetPoint = bezierPoints[0];
            Vector3 directionToTarget = targetPoint - transform.position;
            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget < waypointThreshold)
            {
                bezierPoints.RemoveAt(0); // Move to the next point in the curve
            }

            // Apply forward movement
            moveInput = 1.0f; // Move forward
            targetTorque = moveInput * maxAcceleration;

            if (IsOnGrass())
            {
                targetTorque *= grassSpeedMultiplier;
            }

            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = targetTorque;
                wheel.wheelCollider.brakeTorque = 0f;
            }

            return; // Exit early to avoid processing normal nodes
        }

        // Handle normal node logic
        Vector3 currentNode = nodes[currentNodeIndex].position;
        Vector3 directionToNode = currentNode - transform.position;
        float distanceToNode = directionToNode.magnitude;

        // Check if the AI has passed the node
        Vector3 forward = transform.forward;
        float dotProduct = Vector3.Dot(forward, directionToNode.normalized);

        if (distanceToNode < waypointThreshold || dotProduct < 0)
        {
            // Advance to the next node if within threshold or if the node has been passed
            Debug.Log($"Node passed or within threshold. Advancing to node {currentNodeIndex + 1}");
            currentNodeIndex = (currentNodeIndex + 1) % nodes.Count;

            // Clear Bezier curve if the next node is a normal node
            if (!IsBezierNode(currentNodeIndex))
            {
                bezierPoints.Clear();
                isFollowingBezierCurve = false;
                Debug.Log("Cleared Bezier curve for normal node.");
            }

            // Always check if the next node starts a new Bezier curve
            UpdateBezierCurve();
            return; // Exit early to avoid further processing
        }

        // Apply forward movement
        moveInput = 1.0f; // Move forward
        targetTorque = moveInput * maxAcceleration;

        if (IsOnGrass())
        {
            targetTorque *= grassSpeedMultiplier;
        }

        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = targetTorque;
            wheel.wheelCollider.brakeTorque = 0f;
        }
    }

    void SteerTowardsPath()
    {
        if (bezierPoints.Count == 0) return;

        // Steer towards the next point in the Bezier curve
        Vector3 targetPoint = bezierPoints[0];
        Vector3 directionToTarget = targetPoint - transform.position;
        Vector3 localDirection = transform.InverseTransformDirection(directionToTarget.normalized);

        steerInput = Mathf.Clamp(localDirection.x, -1.0f, 1.0f);

        foreach (var wheel in wheels)
        {
            if (wheel.axel == CarController.Axel.Front)
            {
                float steerAngle = steerInput * turnSensitivity;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, steerAngle, 0.1f);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (nodes == null || nodes.Count == 0 || currentNodeIndex >= nodes.Count) return;

        // Draw the Bezier curve or straight line
        Gizmos.color = Color.blue;
        for (int i = 0; i < bezierPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(bezierPoints[i], bezierPoints[i + 1]);
        }

        // Draw spheres for all nodes
        for (int i = 0; i < nodes.Count; i++)
        {
            if (i == currentNodeIndex)
            {
                // Draw a green sphere for the current node
                Gizmos.color = Color.green;
            }
            else if (i > currentNodeIndex && i <= (currentNodeIndex + 2) % nodes.Count)
            {
                // Draw a yellow sphere for nodes skipped by the Bezier curve
                Gizmos.color = Color.yellow;
            }
            else
            {
                // Draw a red sphere for other nodes
                Gizmos.color = Color.red;
            }

            Gizmos.DrawSphere(nodes[i].position, 0.5f);

            // Draw a line from the node to the closest point on the curve
            if (bezierPoints.Count > 0)
            {
                Vector3 closestPoint = FindClosestPointOnCurve(nodes[i].position);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(nodes[i].position, closestPoint);
            }
        }

#if UNITY_EDITOR
        // Draw the activation range for the next node
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(nodes[currentNodeIndex].position, Vector3.up, waypointThreshold);
#endif
    }

    Vector3 FindClosestPointOnCurve(Vector3 nodePosition)
    {
        Vector3 closestPoint = bezierPoints[0];
        float closestDistance = Vector3.Distance(nodePosition, closestPoint);

        foreach (var point in bezierPoints)
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

    bool IsBezierNode(int nodeIndex)
    {
        // Check if the node is part of the current Bezier curve
        if (bezierPoints.Count == 0) return false;

        Vector3 nodePosition = nodes[nodeIndex].position;
        foreach (var point in bezierPoints)
        {
            if (Vector3.Distance(nodePosition, point) < waypointThreshold)
            {
                return true; // The node is part of the Bezier curve
            }
        }

        return false; // The node is not part of the Bezier curve
    }
}
