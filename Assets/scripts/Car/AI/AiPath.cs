using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor; // Required for Handles and editor-specific functionality
#endif

public class AiPath : MonoBehaviour
{
    public Color LineColor;

    public List<Transform> nodes = new List<Transform>();

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = LineColor;

        Transform[] pathTransform = GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for (int i = 0; i < pathTransform.Length; i++)
        {
            if (pathTransform[i] != transform)
            {
                nodes.Add(pathTransform[i]);
            }
        }

        for (int i = 0; i < nodes.Count; i++) {
            Vector3 currentNode = nodes[i].position;
            Vector3 nextNode = nodes[(i + 1) % nodes.Count].position;

            // Draw the line between nodes
            Gizmos.DrawLine(currentNode, nextNode);

            // Draw the sphere at the current node
            Gizmos.DrawSphere(currentNode, 0.3f);

            // Make the sphere clickable in the editor
            if (Handles.Button(currentNode, Quaternion.identity, 0.4f, 0.4f, Handles.SphereHandleCap))
            {
                
                // Explicitly select the child node's GameObject
                Selection.activeGameObject = nodes[i].gameObject;

                // Focus the Scene View camera on the selected node
                SceneView.lastActiveSceneView.Frame(nodes[i].gameObject.GetComponent<Renderer>()?.bounds ?? new Bounds(currentNode, Vector3.one), false);
            }
        }
    }
#endif
}
