using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Powerline : MonoBehaviour
{
    [Header("Powerline Settings")]
    [Tooltip("Names of the child objects that should be linked with wires")]
    [SerializeField] private List<string> connectorNames = new()
    { 
        "left connector 1", 
        "left connector 2", 
        "right connector 1", 
        "right connector 2" 
    };

    [Tooltip("How much the wire sags in the middle")]
    [SerializeField] private float sagAmount = 2.0f;
    
    [Tooltip("How smooth the curve is.")]
    [Range(3, 50)]
    [SerializeField] private int segments = 15;
    
    [Tooltip("The thickness of the generated wires")]
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float lineYOffset = 0.3f;
    
    [Tooltip("Material to apply to the generated wires")]
    [SerializeField] private Material wireMaterial;

    private Transform wireContainer;
    private List<LineRenderer> linePool = new List<LineRenderer>();

    [ContextMenu("Generate Wires")]
    public void GenerateWires()
    {
        if (wireContainer == null)
        {
            wireContainer = transform.Find("GeneratedWires");
            if (wireContainer == null)
            {
                GameObject go = new GameObject("GeneratedWires");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                wireContainer = go.transform;
            }
        }

        linePool.Clear();
        linePool.AddRange(wireContainer.GetComponentsInChildren<LineRenderer>(true));

        List<Transform> poles = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child != wireContainer && child.gameObject.activeInHierarchy)
            {
                poles.Add(child);
            }
        }

        int lineIndex = 0;

        for (int i = 0; i < poles.Count - 1; i++)
        {
            Transform currentPole = poles[i];
            Transform nextPole = poles[i + 1];

            foreach (string connName in connectorNames)
            {
                Transform currentConnector = FindChildByName(currentPole, connName);
                Transform nextConnector = FindChildByName(nextPole, connName);

                if (currentConnector != null && nextConnector != null)
                {
                    LineRenderer lr = GetOrCreateLine(lineIndex);
                    UpdateLinePoints(lr, new(currentConnector.position.x, currentConnector.position.y + lineYOffset, currentConnector.position.z), new(nextConnector.position.x, nextConnector.position.y + lineYOffset, nextConnector.position.z));
                    lineIndex++;
                }
            }
        }

        for (int i = lineIndex; i < linePool.Count; i++)
        {
            if (linePool[i] != null)
            {
                linePool[i].gameObject.SetActive(false);
            }
        }
    }

    private LineRenderer GetOrCreateLine(int index)
    {
        if (index < linePool.Count && linePool[index] != null)
        {
            LineRenderer lr = linePool[index];
            lr.gameObject.SetActive(true);
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            if (wireMaterial != null) lr.sharedMaterial = wireMaterial;
            return lr;
        }

        GameObject lineObj = new GameObject("Wire_" + index);
        lineObj.transform.SetParent(wireContainer);
        LineRenderer newLr = lineObj.AddComponent<LineRenderer>();
        
        newLr.startWidth = lineWidth;
        newLr.endWidth = lineWidth;
        if (wireMaterial != null) newLr.sharedMaterial = wireMaterial;
        newLr.useWorldSpace = true;
        
        linePool.Add(newLr);
        return newLr;
    }

    private void UpdateLinePoints(LineRenderer lr, Vector3 p1, Vector3 p2)
    {
        lr.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 point = Vector3.Lerp(p1, p2, t);
            
            point.y -= (4f * sagAmount) * t * (1f - t);
            
            lr.SetPosition(i, point);
        }
    }

    private Transform FindChildByName(Transform parent, string nameToFind)
    {
        foreach (Transform child in parent) if (child.name == nameToFind && child != null) return child;
        return null;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(Powerline))]
public class PowerlineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector variables
        DrawDefaultInspector();

        Powerline myScript = (Powerline)target;
        
        GUILayout.Space(10);
        
        // Create a large, easy-to-click button in the Inspector
        if (GUILayout.Button("Generate Wires", GUILayout.Height(30)))
        {
            myScript.GenerateWires();
            
            // Mark the scene as dirty so Unity knows to save these new wire changes
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(myScript.gameObject.scene);
            }
        }
    }
}
#endif