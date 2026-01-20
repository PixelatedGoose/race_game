using UnityEngine;

public class FollowCarNoRotation : MonoBehaviour
{
    [Tooltip("Optional. If null, the first active object with the tag will be used.")]
    public Transform target;

    [Tooltip("Tag used to auto-find the target when 'target' is null.")]
    public string targetTag = "thisisacar";

    [Tooltip("If set, will follow this child under the tagged object.")]
    public string childName = "car";

    [Tooltip("Offset from the target. When false = world-space (ignores rotation). When true = target-local (follows rotation).")]
    public bool offsetInTargetLocalSpace = false;

    [Tooltip("Offset value (interpreted as world or local based on 'offsetInTargetLocalSpace').")]
    public Vector3 localOffset = new Vector3(0f, 3f, -6f);

    [Tooltip("Disable to freely move this object in Play Mode.")]
    public bool follow = true;

    void Awake()
    {
        TryFindTarget();
    }

    void LateUpdate()
    {
        if (!follow) return;

        if (target == null || !target.gameObject.activeInHierarchy)
            TryFindTarget();

        if (target == null) return;

        // Position using chosen space. Rotation is not modified.
        Vector3 desired = offsetInTargetLocalSpace
            ? target.TransformPoint(localOffset)
            : target.position + localOffset;

        transform.position = desired;
        // Do not modify rotation (keeps VFX base unrotated)
    }

    void TryFindTarget()
    {
        if (target != null && target.gameObject.activeInHierarchy) return;

        var root = GameObject.FindWithTag(targetTag); // first active with tag
        if (root != null)
        {
            if (!string.IsNullOrEmpty(childName))
            {
                var child = FindChildRecursive(root.transform, childName);
                target = child != null ? child : root.transform;
            }
            else
            {
                target = root.transform;
            }
        }
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }

    // Right-click the component header → Capture offset from current position
    [ContextMenu("Capture offset from current position")]
    void CaptureOffsetFromCurrent()
    {
        if (target == null) return;
        if (offsetInTargetLocalSpace)
            localOffset = target.InverseTransformPoint(transform.position);
        else
            localOffset = transform.position - target.position;
    }
}