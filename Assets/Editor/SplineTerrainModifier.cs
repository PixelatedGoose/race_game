#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

namespace Unity.Splines.Examples
{
    public class SplineTerrainModifier : EditorWindow
    {
        #region Data Structures
        
        private struct SplinePoint
        {
            public Vector3 worldPosition;
            public Vector3 rightDirection;
            public float height;
            
            public SplinePoint(Vector3 worldPos, Vector3 right, float height)
            {
                this.worldPosition = worldPos;
                this.rightDirection = right;
                this.height = height;
            }
        }
        
        private struct TerrainModification
        {
            public float targetHeight;
            public float blendFactor;
            public float distanceToCenter;
            public bool isRoadArea; // New field to mark road area
            
            public TerrainModification(float height, float blend, float distance, bool isRoad = false)
            {
                this.targetHeight = height;
                this.blendFactor = blend;
                this.distanceToCenter = distance;
                this.isRoadArea = isRoad;
            }
        }
        
        #endregion

        #region Configuration
        
        [System.Serializable]
        private class TerrainModifierSettings
        {
            [Header("References")]
            public SplineContainer splineContainer;
            public Terrain terrain;

            [Header("Height Settings")]
            public float roadWidth = 8f;
            public float blendWidth = 4f;
            public int samplesPerMeter = 2;
            public float heightOffset = -0.1f;

            [Header("Texture Settings")]
            public bool paintTexture = true;
            public int roadLayerIndex = 0;
            public float textureBlendWidth = 2f;
            
            public bool IsValid => splineContainer != null && terrain != null;
            public float TotalWidth => roadWidth + blendWidth;
            public float TotalTextureWidth => roadWidth + textureBlendWidth;
        }
        
        private TerrainModifierSettings settings = new TerrainModifierSettings();
        private string[] layerNames;
        
        #endregion

        #region Editor Window Setup

        [MenuItem("Tools/Spline Terrain Modifier")]
        public static void ShowWindow()
        {
            GetWindow<SplineTerrainModifier>("Spline Terrain Modifier");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Spline Terrain Modifier", EditorStyles.largeLabel);
            EditorGUILayout.Space(10);
            
            DrawReferences();
            DrawHeightSettings();
            DrawTextureSettings();
            DrawActionButtons();
        }

        #endregion

        #region GUI Components

        private void DrawReferences()
        {
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
            settings.splineContainer = (SplineContainer)EditorGUILayout.ObjectField(
                "Spline Container", settings.splineContainer, typeof(SplineContainer), true);
            settings.terrain = (Terrain)EditorGUILayout.ObjectField(
                "Terrain", settings.terrain, typeof(Terrain), true);
            EditorGUILayout.Space();
        }

        private void DrawHeightSettings()
        {
            EditorGUILayout.LabelField("Height Settings", EditorStyles.boldLabel);
            settings.roadWidth = EditorGUILayout.FloatField("Road Width", settings.roadWidth);
            settings.blendWidth = EditorGUILayout.FloatField("Blend Width", settings.blendWidth);
            settings.samplesPerMeter = EditorGUILayout.IntSlider("Samples Per Meter", settings.samplesPerMeter, 1, 10);
            settings.heightOffset = EditorGUILayout.FloatField("Height Offset", settings.heightOffset);
            EditorGUILayout.Space();
        }

        private void DrawTextureSettings()
        {
            EditorGUILayout.LabelField("Texture Settings", EditorStyles.boldLabel);
            settings.paintTexture = EditorGUILayout.Toggle("Paint Texture", settings.paintTexture);

            if (!settings.paintTexture || settings.terrain == null)
            {
                EditorGUILayout.Space();
                return;
            }

            UpdateLayerNames();
            DrawLayerSelection();
            EditorGUILayout.Space();
        }
        
        private void DrawLayerSelection()
        {
            if (layerNames?.Length > 0)
            {
                settings.roadLayerIndex = EditorGUILayout.Popup("Road Layer", settings.roadLayerIndex, layerNames);
                settings.textureBlendWidth = EditorGUILayout.FloatField("Texture Blend Width", settings.textureBlendWidth);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No terrain layers found. Add terrain layers to the terrain first.",
                    MessageType.Warning);
            }
        }

        private void DrawActionButtons()
        {
            EditorGUI.BeginDisabledGroup(!settings.IsValid);
            
            if (GUILayout.Button("Modify Terrain Height", GUILayout.Height(30)))
            {
                ExecuteTerrainOperation(OperationType.HeightOnly);
            }

            EditorGUI.BeginDisabledGroup(!settings.paintTexture);
            if (GUILayout.Button("Paint Road Texture", GUILayout.Height(25)))
            {
                ExecuteTerrainOperation(OperationType.TextureOnly);
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Modify Height + Paint Texture", GUILayout.Height(35)))
            {
                ExecuteTerrainOperation(OperationType.Both);
            }
            
            EditorGUI.EndDisabledGroup();
        }

        #endregion

        #region Operations

        private enum OperationType { HeightOnly, TextureOnly, Both }

        private void ExecuteTerrainOperation(OperationType operationType)
        {
            if (!ValidateInputs()) return;

            bool modifyHeight = operationType == OperationType.HeightOnly || operationType == OperationType.Both;
            bool modifyTexture = operationType == OperationType.TextureOnly || operationType == OperationType.Both;

            string undoName = GetUndoName(operationType);
            RegisterTerrainUndo(undoName, modifyHeight, modifyTexture);

            if (modifyHeight)
                ModifyTerrainHeight();
            
            if (modifyTexture)
                PaintRoadTexture();
        }

        private string GetUndoName(OperationType operationType)
        {
            return operationType switch
            {
                OperationType.HeightOnly => "Modify Terrain Height",
                OperationType.TextureOnly => "Paint Road Texture",
                OperationType.Both => "Modify Terrain and Paint",
                _ => "Terrain Operation"
            };
        }

        #endregion

        #region Validation & Utilities

        private bool ValidateInputs()
        {
            if (!settings.IsValid)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Spline Container and Terrain.", "OK");
                return false;
            }
            return true;
        }

        private void UpdateLayerNames()
        {
            var terrainLayers = settings.terrain?.terrainData?.terrainLayers;
            if (terrainLayers?.Length > 0)
            {
                layerNames = new string[terrainLayers.Length];
                for (int i = 0; i < terrainLayers.Length; i++)
                {
                    layerNames[i] = terrainLayers[i] != null 
                        ? $"{i}: {terrainLayers[i].name}" 
                        : $"{i}: (Empty)";
                }

                settings.roadLayerIndex = Mathf.Clamp(settings.roadLayerIndex, 0, terrainLayers.Length - 1);
            }
            else
            {
                layerNames = null;
            }
        }

        private void RegisterTerrainUndo(string undoName, bool includeHeightmap, bool includeAlphamap)
        {
            var terrainData = settings.terrain.terrainData;

            Undo.SetCurrentGroupName(undoName);
            int undoGroup = Undo.GetCurrentGroup();
            Undo.RegisterCompleteObjectUndo(terrainData, undoName);

            if (includeAlphamap)
            {
                var alphamapTextures = terrainData.alphamapTextures;
                foreach (var texture in alphamapTextures)
                {
                    if (texture != null)
                        Undo.RegisterCompleteObjectUndo(texture, undoName);
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
        }

        #endregion

        #region Spline Evaluation

        private SplinePoint EvaluateSplinePoint(Spline spline, float t)
        {
            SplineUtility.Evaluate(spline, t, out float3 localPos, out float3 localTangent, out _);

            var worldPos = settings.splineContainer.transform.TransformPoint(localPos);
            var worldTangent = settings.splineContainer.transform.TransformDirection(localTangent).normalized;

            // Calculate horizontal right vector
            var tangentHorizontal = new Vector3(worldTangent.x, 0, worldTangent.z).normalized;
            var worldRight = tangentHorizontal.sqrMagnitude < 0.001f 
                ? Vector3.right 
                : Vector3.Cross(Vector3.up, tangentHorizontal).normalized;

            return new SplinePoint(worldPos, worldRight, worldPos.y);
        }

        #endregion

        #region Coordinate Conversion

        private bool TryGetTerrainCoordinates(Vector3 worldPos, out Vector2 normalizedCoords)
        {
            var terrainPos = settings.terrain.transform.position;
            var terrainSize = settings.terrain.terrainData.size;

            normalizedCoords = new Vector2(
                (worldPos.x - terrainPos.x) / terrainSize.x,
                (worldPos.z - terrainPos.z) / terrainSize.z
            );

            return normalizedCoords.x >= 0 && normalizedCoords.x <= 1 && 
                   normalizedCoords.y >= 0 && normalizedCoords.y <= 1;
        }

        private static float CalculateBlendFactor(float distFromCenter, float innerWidth, float outerBlendWidth)
        {
            if (distFromCenter <= innerWidth)
                return 1f;
            
            return Mathf.Clamp01(1f - (distFromCenter - innerWidth) / outerBlendWidth);
        }

        #endregion

        #region Height Modification

        private void ModifyTerrainHeight()
        {
            var terrainData = settings.terrain.terrainData;
            int resolution = terrainData.heightmapResolution;
            var heights = terrainData.GetHeights(0, 0, resolution, resolution);

            var heightUpdates = CollectHeightUpdates(resolution);
            ApplyHeightUpdates(heights, heightUpdates);

            terrainData.SetHeights(0, 0, heights);
            EditorUtility.SetDirty(terrainData);
        }

        private Dictionary<Vector2Int, TerrainModification> CollectHeightUpdates(int resolution)
        {
            var heightUpdates = new Dictionary<Vector2Int, TerrainModification>();
            var terrainPos = settings.terrain.transform.position;
            var terrainSize = settings.terrain.terrainData.size;

            // First pass: collect all road segments to calculate flat height
            var roadSegments = CollectRoadSegments();
            
            foreach (var spline in settings.splineContainer.Splines)
            {
                ProcessSplineForHeight(spline, heightUpdates, terrainPos, terrainSize, resolution, roadSegments);
            }

            return heightUpdates;
        }

        private Dictionary<Vector2Int, List<float>> CollectRoadSegments()
        {
            var roadPoints = new Dictionary<Vector2Int, List<float>>();
            var terrainPos = settings.terrain.transform.position;
            var terrainSize = settings.terrain.terrainData.size;
            int resolution = settings.terrain.terrainData.heightmapResolution;

            foreach (var spline in settings.splineContainer.Splines)
            {
                float length = spline.GetLength();
                int samples = Mathf.CeilToInt(length * settings.samplesPerMeter);

                for (int i = 0; i <= samples; i++)
                {
                    float t = (float)i / samples;
                    var splinePoint = EvaluateSplinePoint(spline, t);
                    
                    // Only collect points within the road width
                    int roadWidthSamples = Mathf.CeilToInt(settings.roadWidth * 2);
                    for (int w = -roadWidthSamples; w <= roadWidthSamples; w++)
                    {
                        float widthOffset = (w / (float)roadWidthSamples) * settings.roadWidth;
                        if (Mathf.Abs(widthOffset) > settings.roadWidth * 0.5f) continue;
                        
                        var samplePos = splinePoint.worldPosition + splinePoint.rightDirection * widthOffset;

                        if (!TryGetTerrainCoordinates(samplePos, out var normalizedCoords))
                            continue;

                        var heightmapCoord = new Vector2Int(
                            Mathf.RoundToInt(normalizedCoords.x * (resolution - 1)),
                            Mathf.RoundToInt(normalizedCoords.y * (resolution - 1))
                        );

                        if (!roadPoints.ContainsKey(heightmapCoord))
                            roadPoints[heightmapCoord] = new List<float>();

                        roadPoints[heightmapCoord].Add(splinePoint.height + settings.heightOffset);
                    }
                }
            }

            return roadPoints;
        }

        private void ProcessSplineForHeight(Spline spline, Dictionary<Vector2Int, TerrainModification> heightUpdates,
            Vector3 terrainPos, Vector3 terrainSize, int resolution, Dictionary<Vector2Int, List<float>> roadSegments)
        {
            float length = spline.GetLength();
            int samples = Mathf.CeilToInt(length * settings.samplesPerMeter);
            int widthSamples = Mathf.CeilToInt(settings.TotalWidth * 2);

            for (int i = 0; i <= samples; i++)
            {
                float t = (float)i / samples;
                var splinePoint = EvaluateSplinePoint(spline, t);

                ProcessSplinePointForHeight(splinePoint, heightUpdates, terrainPos, terrainSize, 
                    resolution, widthSamples, roadSegments);
            }
        }

        private void ProcessSplinePointForHeight(SplinePoint splinePoint, 
            Dictionary<Vector2Int, TerrainModification> heightUpdates,
            Vector3 terrainPos, Vector3 terrainSize, int resolution, int widthSamples,
            Dictionary<Vector2Int, List<float>> roadSegments)
        {
            for (int w = -widthSamples; w <= widthSamples; w++)
            {
                float widthOffset = (w / (float)widthSamples) * settings.TotalWidth;
                var samplePos = splinePoint.worldPosition + splinePoint.rightDirection * widthOffset;

                if (!TryGetTerrainCoordinates(samplePos, out var normalizedCoords))
                    continue;

                var heightmapCoord = new Vector2Int(
                    Mathf.RoundToInt(normalizedCoords.x * (resolution - 1)),
                    Mathf.RoundToInt(normalizedCoords.y * (resolution - 1))
                );

                bool isRoadArea = Mathf.Abs(widthOffset) <= settings.roadWidth * 0.5f;
                float targetHeight;

                if (isRoadArea)
                {
                    // For road area, use the lowest height found across the road width at this point
                    if (roadSegments.TryGetValue(heightmapCoord, out var heights))
                    {
                        targetHeight = GetLowestHeight(heights);
                    }
                    else
                    {
                        targetHeight = splinePoint.height + settings.heightOffset;
                    }
                    targetHeight = (targetHeight - terrainPos.y) / terrainSize.y;
                }
                else
                {
                    // For blend area, use original height
                    targetHeight = (splinePoint.height + settings.heightOffset - terrainPos.y) / terrainSize.y;
                }

                float blendFactor = CalculateBlendFactor(Mathf.Abs(widthOffset), settings.roadWidth, settings.blendWidth);
                float distanceToCenter = Mathf.Abs(widthOffset);

                UpdateHeightMap(heightUpdates, heightmapCoord, targetHeight, blendFactor, distanceToCenter, isRoadArea);
            }
        }

        private float GetLowestHeight(List<float> heights)
        {
            float minHeight = heights[0];
            for (int i = 1; i < heights.Count; i++)
            {
                if (heights[i] < minHeight)
                    minHeight = heights[i];
            }
            return minHeight;
        }

        private static void UpdateHeightMap(Dictionary<Vector2Int, TerrainModification> heightUpdates,
            Vector2Int coord, float targetHeight, float blendFactor, float distanceToCenter, bool isRoadArea)
        {
            if (!heightUpdates.TryGetValue(coord, out var existing))
            {
                heightUpdates[coord] = new TerrainModification(targetHeight, blendFactor, distanceToCenter, isRoadArea);
                return;
            }

            // Road area always takes priority
            if (isRoadArea && !existing.isRoadArea)
            {
                heightUpdates[coord] = new TerrainModification(targetHeight, blendFactor, distanceToCenter, isRoadArea);
                return;
            }
            
            // Both road areas - use the lowest height to ensure flat road
            if (isRoadArea && existing.isRoadArea)
            {
                float lowestHeight = Mathf.Min(targetHeight, existing.targetHeight);
                heightUpdates[coord] = new TerrainModification(lowestHeight, 1f, distanceToCenter, isRoadArea);
                return;
            }

            // Neither are road areas - use original logic
            if (!isRoadArea && !existing.isRoadArea)
            {
                // Higher blend factor takes priority
                if (blendFactor > existing.blendFactor)
                {
                    heightUpdates[coord] = new TerrainModification(targetHeight, blendFactor, distanceToCenter, isRoadArea);
                }
                // Same blend, closer distance wins
                else if (Mathf.Approximately(blendFactor, existing.blendFactor))
                {
                    if (distanceToCenter < existing.distanceToCenter)
                    {
                        heightUpdates[coord] = new TerrainModification(targetHeight, blendFactor, distanceToCenter, isRoadArea);
                    }
                    else if (Mathf.Approximately(distanceToCenter, existing.distanceToCenter))
                    {
                        // Average heights for same distance
                        float avgHeight = (existing.targetHeight + targetHeight) * 0.5f;
                        heightUpdates[coord] = new TerrainModification(avgHeight, blendFactor, distanceToCenter, isRoadArea);
                    }
                }
            }
        }

        private static void ApplyHeightUpdates(float[,] heights, Dictionary<Vector2Int, TerrainModification> heightUpdates)
        {
            foreach (var kvp in heightUpdates)
            {
                var coord = kvp.Key;
                var modification = kvp.Value;
                
                float currentHeight = heights[coord.y, coord.x];
                
                // For road areas, always use full blend to ensure completely flat surface
                float finalBlendFactor = modification.isRoadArea ? 1f : modification.blendFactor;
                
                heights[coord.y, coord.x] = Mathf.Lerp(currentHeight, modification.targetHeight, finalBlendFactor);
            }
        }

        #endregion

        #region Texture Painting

        private void PaintRoadTexture()
        {
            var terrainData = settings.terrain.terrainData;
            int layerCount = terrainData.alphamapLayers;

            if (layerCount == 0 || settings.roadLayerIndex >= layerCount)
            {
                EditorUtility.DisplayDialog("Error", "Invalid terrain layer configuration.", "OK");
                return;
            }

            int alphamapWidth = terrainData.alphamapWidth;
            int alphamapHeight = terrainData.alphamapHeight;
            var alphamaps = terrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);

            var paintData = CollectPaintData(alphamapWidth, alphamapHeight);
            ApplyPaintData(alphamaps, paintData, layerCount);

            terrainData.SetAlphamaps(0, 0, alphamaps);
            EditorUtility.SetDirty(terrainData);
        }

        private Dictionary<Vector2Int, float> CollectPaintData(int alphamapWidth, int alphamapHeight)
        {
            var paintData = new Dictionary<Vector2Int, float>();

            foreach (var spline in settings.splineContainer.Splines)
            {
                ProcessSplineForTexture(spline, paintData, alphamapWidth, alphamapHeight);
            }

            return paintData;
        }

        private void ProcessSplineForTexture(Spline spline, Dictionary<Vector2Int, float> paintData,
            int alphamapWidth, int alphamapHeight)
        {
            float length = spline.GetLength();
            int samples = Mathf.CeilToInt(length * settings.samplesPerMeter * 2);
            int widthSamples = Mathf.CeilToInt(settings.TotalTextureWidth * 4);

            for (int i = 0; i <= samples; i++)
            {
                float t = (float)i / samples;
                var splinePoint = EvaluateSplinePoint(spline, t);

                ProcessSplinePointForTexture(splinePoint, paintData, alphamapWidth, alphamapHeight, widthSamples);
            }
        }

        private void ProcessSplinePointForTexture(SplinePoint splinePoint, Dictionary<Vector2Int, float> paintData,
            int alphamapWidth, int alphamapHeight, int widthSamples)
        {
            for (int w = -widthSamples; w <= widthSamples; w++)
            {
                float widthOffset = (w / (float)widthSamples) * settings.TotalTextureWidth;
                var samplePos = splinePoint.worldPosition + splinePoint.rightDirection * widthOffset;

                if (!TryGetTerrainCoordinates(samplePos, out var normalizedCoords))
                    continue;

                var alphamapCoord = new Vector2Int(
                    Mathf.RoundToInt(normalizedCoords.y * (alphamapWidth - 1)),
                    Mathf.RoundToInt(normalizedCoords.x * (alphamapHeight - 1))
                );

                float blendFactor = CalculateBlendFactor(Mathf.Abs(widthOffset), settings.roadWidth, settings.textureBlendWidth);
                blendFactor = Mathf.SmoothStep(0f, 1f, blendFactor);

                if (!paintData.ContainsKey(alphamapCoord) || paintData[alphamapCoord] < blendFactor)
                {
                    paintData[alphamapCoord] = blendFactor;
                }
            }
        }

        private void ApplyPaintData(float[,,] alphamaps, Dictionary<Vector2Int, float> paintData, int layerCount)
        {
            foreach (var kvp in paintData)
            {
                var coord = kvp.Key;
                float blendFactor = kvp.Value;

                if (coord.x < 0 || coord.x >= alphamaps.GetLength(0) || 
                    coord.y < 0 || coord.y >= alphamaps.GetLength(1))
                    continue;

                BlendLayerWeights(alphamaps, coord, blendFactor, layerCount);
            }
        }

        private void BlendLayerWeights(float[,,] alphamaps, Vector2Int coord, float blendFactor, int layerCount)
        {
            // Blend towards target weights
            for (int layer = 0; layer < layerCount; layer++)
            {
                float currentWeight = alphamaps[coord.x, coord.y, layer];
                float targetWeight = (layer == settings.roadLayerIndex) ? 1f : 0f;
                alphamaps[coord.x, coord.y, layer] = Mathf.Lerp(currentWeight, targetWeight, blendFactor);
            }

            // Normalize weights to sum to 1
            NormalizeLayerWeights(alphamaps, coord, layerCount);
        }

        private static void NormalizeLayerWeights(float[,,] alphamaps, Vector2Int coord, int layerCount)
        {
            float totalWeight = 0f;
            for (int layer = 0; layer < layerCount; layer++)
            {
                totalWeight += alphamaps[coord.x, coord.y, layer];
            }

            if (totalWeight > 0f)
            {
                for (int layer = 0; layer < layerCount; layer++)
                {
                    alphamaps[coord.x, coord.y, layer] /= totalWeight;
                }
            }
        }

        #endregion
    }
}
#endif