using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour
{
    public RenderTexture MiniMapTexture;
    public Material MiniMapMaterial;

	

    [Header("MiniMap GUI Placement (as % of screen)")]
    [Range(0f, 1f)] public float x = 0.03f;
    [Range(0f, 1f)] public float y = 0.05f;
    [Range(0f, 1f)] public float width = 0.18f;
    [Range(0f, 1f)] public float height = 0.28f;

/*     private float offset;

    void Awake()
    {
        offset = 10;
    } */

    void OnGUI()
    {
        Rect Map_Rectangle = new Rect(
            x * Screen.width,
            y * Screen.height,
            width * Screen.width,
            height * Screen.height
        );

        if (Event.current.type == EventType.Repaint)
        {
            Graphics.DrawTexture(Map_Rectangle, MiniMapTexture, MiniMapMaterial);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Draw a rectangle in the Scene view to preview minimap placement
        float screenW = UnityEditor.Handles.GetMainGameViewSize().x;
        float screenH = UnityEditor.Handles.GetMainGameViewSize().y;

        // Flip Y to match OnGUI's top-left origin
        float flippedY = screenH - (y * screenH) - (height * screenH);

        Rect rect = new Rect(
            x * screenW,
            flippedY,
            width * screenW,
            height * screenH
        );
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3(rect.x + rect.width / 2, rect.y + rect.height / 2, 0),
            new Vector3(rect.width, rect.height, 0)
        );
    }
#endif
}
