using UnityEngine;
#if UNITY_EDITOR
#endif

public class CheckpointArrow : MonoBehaviour
{
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        float arrowLength = 5f;
        
        // Calculate arrow end position along local Z-axis
        Vector3 arrowEnd = transform.position + transform.forward * arrowLength;
        
        Gizmos.DrawLine(transform.position, arrowEnd);
        
        // Draw arrowhead
        Vector3 arrowHeadSize = transform.forward * (arrowLength * 0.3f);
        Vector3 arrowHeadRight = transform.right * (arrowLength * 0.15f);
        
        Gizmos.DrawLine(arrowEnd, arrowEnd - arrowHeadSize + arrowHeadRight);
        Gizmos.DrawLine(arrowEnd, arrowEnd - arrowHeadSize - arrowHeadRight);
    }
#endif
}
