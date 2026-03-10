using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BillboardPlacer : EditorWindow
{
    private GameObject prefabToPlace;
    private float brushRadius = 10.5f;
    private float minSpacing = 3f;
    private float heightOffset = 7.62f;
    private bool alignToNormal = false;
    private float randomRotationY = 0f;
    private bool useRandomScale = false;
    private float randomScaleMin = 1f;
    private float randomScaleMax = 1f;
    
    // Base rotation offset for prefabs
    private Vector3 baseRotationOffset = new Vector3(90f, 0f, 0f);
    
    // Layer mask for raycasting
    private LayerMask raycastLayers = ~0;
    
    private bool isPainting = false;
    private Transform parentContainer;
    
    private List<Vector3> placedPositions = new List<Vector3>();
    
    [MenuItem("Tools/Billboard Placer")]
    public static void ShowWindow()
    {
        GetWindow<BillboardPlacer>("Billboard Placer");
    }
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        RefreshPlacedPositions();
    }
    
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Billboard Placer Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Prefab selection
        prefabToPlace = (GameObject)EditorGUILayout.ObjectField("Prefab to Place", prefabToPlace, typeof(GameObject), false);
        
        // Parent container
        parentContainer = (Transform)EditorGUILayout.ObjectField("Parent Container", parentContainer, typeof(Transform), true);
        
        // Layer mask
        raycastLayers = EditorGUILayout.MaskField("Raycast Layers", raycastLayers, GetLayerNames());
        
        EditorGUILayout.Space();
        GUILayout.Label("Brush Settings", EditorStyles.boldLabel);
        
        brushRadius = EditorGUILayout.Slider("Brush Radius", brushRadius, 1f, 50f);
        minSpacing = EditorGUILayout.Slider("Min Spacing", minSpacing, 0.5f, 20f);
        heightOffset = EditorGUILayout.Slider("Height Offset", heightOffset, -10f, 10f);
        
        EditorGUILayout.Space();
        GUILayout.Label("Rotation Settings", EditorStyles.boldLabel);
        
        // Base rotation offset
        baseRotationOffset = EditorGUILayout.Vector3Field("Base Rotation Offset", baseRotationOffset);
        EditorGUILayout.HelpBox("Use this to correct prefab orientation.\nExample: X=90 for trees modeled with Z-up.", MessageType.None);
        
        alignToNormal = EditorGUILayout.Toggle("Align to Surface Normal", alignToNormal);
        randomRotationY = EditorGUILayout.Slider("Random Y Rotation", randomRotationY, 0f, 360f);
        
        EditorGUILayout.Space();
        GUILayout.Label("Random Scale", EditorStyles.boldLabel);
        
        useRandomScale = EditorGUILayout.Toggle("Use Random Scale", useRandomScale);
        
        EditorGUI.BeginDisabledGroup(!useRandomScale);
        randomScaleMin = EditorGUILayout.Slider("Scale Min", randomScaleMin, 0.1f, 2f);
        randomScaleMax = EditorGUILayout.Slider("Scale Max", randomScaleMax, 0.1f, 3f);
        EditorGUI.EndDisabledGroup();
        
        if (randomScaleMin > randomScaleMax)
            randomScaleMin = randomScaleMax;
        
        EditorGUILayout.Space();
        
        // Painting toggle
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = isPainting ? Color.green : Color.white;
        
        if (GUILayout.Button(isPainting ? "Stop Painting (P)" : "Start Painting (P)", GUILayout.Height(30)))
        {
            TogglePainting();
        }
        
        GUI.backgroundColor = originalColor;
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Instructions:\n" +
            "• Press P or click the button to toggle painting mode\n" +
            "• Left-click and drag to paint prefabs\n" +
            "• Hold Shift + Left-click to erase prefabs\n" +
            "• Scroll wheel to adjust brush size while painting",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Clear All Placed Objects"))
        {
            if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to remove all placed objects?", "Yes", "No"))
            {
                ClearAllPlacedObjects();
            }
        }
        
        if (GUILayout.Button("Refresh Placed Positions Cache"))
        {
            RefreshPlacedPositions();
        }
    }
    
    private string[] GetLayerNames()
    {
        string[] layers = new string[32];
        for (int i = 0; i < 32; i++)
        {
            layers[i] = LayerMask.LayerToName(i);
            if (string.IsNullOrEmpty(layers[i]))
                layers[i] = "Layer " + i;
        }
        return layers;
    }
    
    private void TogglePainting()
    {
        isPainting = !isPainting;
        
        if (isPainting)
        {
            RefreshPlacedPositions();
            Tools.current = Tool.None;
        }
        
        SceneView.RepaintAll();
        Repaint();
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        
        // Toggle painting with P key
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.P)
        {
            TogglePainting();
            e.Use();
            Repaint();
        }
        
        if (!isPainting || prefabToPlace == null)
            return;
        
        // Get mouse position on terrain/surface - use regular raycast for brush position
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        
        // Use regular raycast for brush visualization (hits everything)
        if (Physics.Raycast(ray, out RaycastHit brushHit, Mathf.Infinity, raycastLayers))
        {
            // Draw brush circle
            Handles.color = e.shift ? new Color(1f, 0f, 0f, 0.3f) : new Color(0f, 1f, 0f, 0.3f);
            Handles.DrawSolidDisc(brushHit.point, brushHit.normal, brushRadius);
            Handles.color = e.shift ? Color.red : Color.green;
            Handles.DrawWireDisc(brushHit.point, brushHit.normal, brushRadius);
            
            // Scroll to adjust brush size
            if (e.type == EventType.ScrollWheel)
            {
                brushRadius = Mathf.Clamp(brushRadius - e.delta.y * 0.5f, 1f, 50f);
                e.Use();
                Repaint();
            }
            
            // Paint on left mouse button
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                if (e.shift)
                {
                    // Erase mode - use the brush hit point directly
                    EraseObjectsInRadius(brushHit.point);
                }
                else
                {
                    // Paint mode - use filtered raycast
                    if (RaycastIgnoringPlacedObjects(ray, out RaycastHit placementHit))
                    {
                        PaintObjectsInRadius(placementHit.point, placementHit.normal);
                    }
                }
                
                e.Use();
            }
            
            // Prevent selection
            if (e.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            
            sceneView.Repaint();
        }
    }
    
    private bool RaycastIgnoringPlacedObjects(Ray ray, out RaycastHit hit)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, raycastLayers);
        
        hit = default;
        float closestDistance = Mathf.Infinity;
        bool foundValidHit = false;
        
        foreach (RaycastHit h in hits)
        {
            // Skip if this object is a child of the parent container
            if (parentContainer != null && h.transform.IsChildOf(parentContainer))
                continue;
            
            if (h.distance < closestDistance)
            {
                closestDistance = h.distance;
                hit = h;
                foundValidHit = true;
            }
        }
        
        return foundValidHit;
    }
    
    private void PaintObjectsInRadius(Vector3 center, Vector3 normal)
    {
        // Try to place multiple objects within brush radius
        int attempts = Mathf.CeilToInt(brushRadius * 2);
        
        for (int i = 0; i < attempts; i++)
        {
            // Random position within brush radius
            Vector2 randomCircle = Random.insideUnitCircle * brushRadius;
            Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
            Vector3 testPosition = center + randomOffset;
            
            // Raycast down to find actual terrain position
            Ray ray = new Ray(testPosition + Vector3.up * 100f, Vector3.down);
            
            if (RaycastIgnoringPlacedObjects(ray, out RaycastHit hit))
            {
                // Check spacing from other placed objects
                if (IsPositionValid(hit.point))
                {
                    PlaceObject(hit.point, hit.normal);
                }
            }
        }
    }
    
    private bool IsPositionValid(Vector3 position)
    {
        foreach (Vector3 placedPos in placedPositions)
        {
            if (Vector3.Distance(position, placedPos) < minSpacing)
            {
                return false;
            }
        }
        return true;
    }
    
    private void PlaceObject(Vector3 position, Vector3 normal)
    {
        // Apply height offset
        Vector3 finalPosition = position + Vector3.up * heightOffset;
        
        // Start with base rotation offset (corrects prefab orientation)
        Quaternion baseRotation = Quaternion.Euler(baseRotationOffset);
        
        // Determine which axis is "up" for this prefab after base rotation
        Vector3 prefabUpAxis = baseRotation * Vector3.up;
        
        // Calculate final rotation
        Quaternion rotation;
        if (alignToNormal)
        {
            // Align the prefab's up axis to surface normal
            Quaternion normalAlignment = Quaternion.FromToRotation(prefabUpAxis, normal);
            Quaternion randomY = Quaternion.AngleAxis(Random.Range(0f, randomRotationY), normal);
            rotation = randomY * normalAlignment * baseRotation;
        }
        else
        {
            // Just random Y rotation (world up) and base rotation
            Quaternion randomY = Quaternion.Euler(0, Random.Range(0f, randomRotationY), 0);
            rotation = randomY * baseRotation;
        }
        
        // Create the object
        GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPlace);
        newObject.transform.position = finalPosition;
        newObject.transform.rotation = rotation;
        
        // Apply random scale only if enabled
        if (useRandomScale)
        {
            float randomScale = Random.Range(randomScaleMin, randomScaleMax);
            newObject.transform.localScale = Vector3.one * randomScale;
        }
        
        // Set parent
        if (parentContainer != null)
        {
            newObject.transform.SetParent(parentContainer);
        }
        
        // Register undo
        Undo.RegisterCreatedObjectUndo(newObject, "Place Billboard Object");
        
        // Add to placed positions cache
        placedPositions.Add(position);
    }
    
    private void EraseObjectsInRadius(Vector3 center)
    {
        if (parentContainer == null)
        {
            Debug.LogWarning("Set a parent container to enable erasing.");
            return;
        }
        
        List<GameObject> toDestroy = new List<GameObject>();
        
        foreach (Transform child in parentContainer)
        {
            // Use XZ distance to ignore height differences from offset
            Vector3 childPosFlat = new Vector3(child.position.x, 0, child.position.z);
            Vector3 centerFlat = new Vector3(center.x, 0, center.z);
            
            if (Vector3.Distance(childPosFlat, centerFlat) <= brushRadius)
            {
                toDestroy.Add(child.gameObject);
            }
        }
        
        foreach (GameObject obj in toDestroy)
        {
            Undo.DestroyObjectImmediate(obj);
        }
        
        RefreshPlacedPositions();
    }
    
    private void RefreshPlacedPositions()
    {
        placedPositions.Clear();
        
        if (parentContainer != null)
        {
            foreach (Transform child in parentContainer)
            {
                placedPositions.Add(child.position);
            }
        }
    }
    
    private void ClearAllPlacedObjects()
    {
        if (parentContainer != null)
        {
            while (parentContainer.childCount > 0)
            {
                Undo.DestroyObjectImmediate(parentContainer.GetChild(0).gameObject);
            }
        }
        
        placedPositions.Clear();
    }
}