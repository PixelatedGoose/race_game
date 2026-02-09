#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Splines;
#endif

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Interpolators = UnityEngine.Splines.Interpolators;

namespace Unity.Splines.Examples
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SplineContainer), typeof(MeshRenderer), typeof(MeshFilter))]
    public class grasspatch : MonoBehaviour
    {
        [SerializeField]
        List<SplineData<float>> m_Heights = new List<SplineData<float>>();

        public List<SplineData<float>> Heights
        {
            get
            {
                foreach (var height in m_Heights)
                {
                    if (height.DefaultValue == 0)
                        height.DefaultValue = 3f;
                }
                return m_Heights;
            }
        }

        [SerializeField]
        SplineContainer m_Spline;

        public SplineContainer Container
        {
            get
            {
                if (m_Spline == null)
                    m_Spline = GetComponent<SplineContainer>();
                return m_Spline;
            }
            set => m_Spline = value;
        }

        [SerializeField, Tooltip("Number of quad segments per meter of spline length.")]
        int m_SegmentsPerMeter = 1;

        [SerializeField]
        Mesh m_Mesh;

        [SerializeField, Tooltip("Scales the U coordinate of the UVs along the spline length for the sides.")]
        float m_TextureScaleU = 1f;

        [SerializeField, Tooltip("Scales the V coordinate of the UVs along the height for the sides.")]
        float m_TextureScaleV = 1f;

        [SerializeField, Tooltip("Vertical offset from the spline. Use to sink the base into the ground.")]
        float m_VerticalOffset = 0f;

        [SerializeField, Tooltip("If true, the bottom edge follows the spline exactly and the mesh grows upward. If false, the mesh is centered on the spline.")]
        bool m_GrowUpward = true;

        [Header("Top Cap Settings")]
        [SerializeField, Tooltip("Material for the top grass surface.")]
        Material m_TopMaterial;

        [SerializeField, Tooltip("Material for the sides (dirt/earth).")]
        Material m_SideMaterial;

        [SerializeField, Tooltip("How far the top surface is sunk below the top edge of the sides. Creates a slight inset lip effect.")]
        float m_TopSinkAmount = 0.1f;

        [SerializeField, Tooltip("UV scale for the top cap. UVs are based on world XZ position.")]
        float m_TopTextureScale = 1f;

        [SerializeField, Tooltip("Resolution of the top cap triangulation. Higher = more triangles for complex shapes.")]
        int m_TopCapResolution = 32;

        public IReadOnlyList<Spline> StripSplines
        {
            get
            {
                if (m_Spline == null)
                    m_Spline = GetComponent<SplineContainer>();

                if (m_Spline == null)
                {
                    Debug.LogError("Cannot build grass patch because Spline reference is null");
                    return null;
                }

                return m_Spline.Splines;
            }
        }

        public Mesh PatchMesh
        {
            get
            {
                if (m_Mesh != null)
                    return m_Mesh;

                m_Mesh = new Mesh();
                return m_Mesh;
            }
        }

        public int SegmentsPerMeter => Mathf.Clamp(m_SegmentsPerMeter, 1, 10);

        // Side mesh data (submesh 0)
        List<Vector3> m_Positions = new List<Vector3>();
        List<Vector3> m_Normals = new List<Vector3>();
        List<Vector2> m_Textures = new List<Vector2>();
        List<int> m_SideIndices = new List<int>();

        // Top cap data (submesh 1)
        List<int> m_TopIndices = new List<int>();

        bool m_RebuildRequested = false;

        void OnValidate()
        {
            m_RebuildRequested = true;
            ApplyMaterials();
        }

        void Update()
        {
            if (m_RebuildRequested)
            {
                RebuildAllStrips();
                m_RebuildRequested = false;
            }
        }

        void ApplyMaterials()
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer == null) return;

            var mats = new Material[2];
            mats[0] = m_SideMaterial;
            mats[1] = m_TopMaterial;
            renderer.sharedMaterials = mats;
        }

        public void OnEnable()
        {
            if (m_Mesh != null)
                m_Mesh = null;

            if (m_Spline == null)
                m_Spline = GetComponent<SplineContainer>();

            ApplyMaterials();
            RebuildAllStrips();

#if UNITY_EDITOR
            EditorSplineUtility.AfterSplineWasModified += OnAfterSplineWasModified;
            EditorSplineUtility.RegisterSplineDataChanged<float>(OnAfterSplineDataWasModified);
            Undo.undoRedoPerformed += RebuildAllStrips;
#endif

            SplineContainer.SplineAdded += OnSplineContainerAdded;
            SplineContainer.SplineRemoved += OnSplineContainerRemoved;
            SplineContainer.SplineReordered += OnSplineContainerReordered;
            Spline.Changed += OnSplineChanged;
        }

        public void OnDisable()
        {
#if UNITY_EDITOR
            EditorSplineUtility.AfterSplineWasModified -= OnAfterSplineWasModified;
            EditorSplineUtility.UnregisterSplineDataChanged<float>(OnAfterSplineDataWasModified);
            Undo.undoRedoPerformed -= RebuildAllStrips;
#endif

            if (m_Mesh != null)
#if UNITY_EDITOR
                DestroyImmediate(m_Mesh);
#else
                Destroy(m_Mesh);
#endif

            SplineContainer.SplineAdded -= OnSplineContainerAdded;
            SplineContainer.SplineRemoved -= OnSplineContainerRemoved;
            SplineContainer.SplineReordered -= OnSplineContainerReordered;
            Spline.Changed -= OnSplineChanged;
        }

        void OnSplineContainerAdded(SplineContainer container, int index)
        {
            if (container != m_Spline) return;

            if (m_Heights.Count < StripSplines.Count)
            {
                var delta = StripSplines.Count - m_Heights.Count;
                for (var i = 0; i < delta; i++)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(this, "Modifying Heights SplineData");
#endif
                    m_Heights.Add(new SplineData<float>() { DefaultValue = 3f });
                }
            }

            RebuildAllStrips();
        }

        void OnSplineContainerRemoved(SplineContainer container, int index)
        {
            if (container != m_Spline) return;

            if (index < m_Heights.Count)
            {
#if UNITY_EDITOR
                Undo.RecordObject(this, "Modifying Heights SplineData");
#endif
                m_Heights.RemoveAt(index);
            }

            RebuildAllStrips();
        }

        void OnSplineContainerReordered(SplineContainer container, int previousIndex, int newIndex)
        {
            if (container != m_Spline) return;
            RebuildAllStrips();
        }

        void OnAfterSplineWasModified(Spline s)
        {
            if (StripSplines == null) return;

            foreach (var spline in StripSplines)
            {
                if (s == spline)
                {
                    m_RebuildRequested = true;
                    break;
                }
            }
        }

        void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification)
        {
            OnAfterSplineWasModified(spline);
        }

        void OnAfterSplineDataWasModified(SplineData<float> splineData)
        {
            foreach (var height in m_Heights)
            {
                if (splineData == height)
                {
                    m_RebuildRequested = true;
                    break;
                }
            }
        }

        public void RebuildAllStrips()
        {
            PatchMesh.Clear();
            m_Positions.Clear();
            m_Normals.Clear();
            m_Textures.Clear();
            m_SideIndices.Clear();
            m_TopIndices.Clear();
            m_Positions.Capacity = 0;
            m_Normals.Capacity = 0;
            m_Textures.Capacity = 0;
            m_SideIndices.Capacity = 0;
            m_TopIndices.Capacity = 0;

            for (var i = 0; i < StripSplines.Count; i++)
            {
                BuildSideStrip(StripSplines[i], i);
                BuildTopCap(StripSplines[i], i);
            }

            PatchMesh.SetVertices(m_Positions);
            PatchMesh.SetNormals(m_Normals);
            PatchMesh.SetUVs(0, m_Textures);
            PatchMesh.subMeshCount = 2;
            PatchMesh.SetIndices(m_SideIndices, MeshTopology.Triangles, 0); // sides
            PatchMesh.SetIndices(m_TopIndices, MeshTopology.Triangles, 1);  // top cap
            PatchMesh.RecalculateBounds();
            PatchMesh.UploadMeshData(false);

            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
        }

        void BuildSideStrip(Spline spline, int heightDataIndex)
        {
            if (spline == null || spline.Count < 2)
                return;

            float length = spline.GetLength();
            if (length <= 0.001f)
                return;

            var segmentsPerLength = SegmentsPerMeter * length;
            var segments = Mathf.CeilToInt(segmentsPerLength);
            var segmentStepT = (1f / SegmentsPerMeter) / length;
            var steps = segments + 1;

            var vertexCount = steps * 2;
            var triangleCount = segments * 6;
            var prevVertexCount = m_Positions.Count;

            m_Positions.Capacity += vertexCount;
            m_Normals.Capacity += vertexCount;
            m_Textures.Capacity += vertexCount;
            m_SideIndices.Capacity += triangleCount;

            float accumulatedLength = 0f;
            float3 prevPos = float3.zero;

            var t = 0f;
            for (int i = 0; i < steps; i++)
            {
                SplineUtility.Evaluate(spline, t, out var pos, out var dir, out var up);

                if (math.length(dir) == 0)
                {
                    var nextPos = spline.GetPointAtLinearDistance(t, 0.01f, out _);
                    dir = math.normalizesafe(nextPos - pos);

                    if (math.length(dir) == 0)
                    {
                        nextPos = spline.GetPointAtLinearDistance(t, -0.01f, out _);
                        dir = -math.normalizesafe(nextPos - pos);
                    }

                    if (math.length(dir) == 0)
                        dir = new float3(0, 0, 1);
                }

                if (i > 0)
                    accumulatedLength += math.length(pos - prevPos);
                prevPos = pos;

                var faceNormal = math.normalizesafe(math.cross(new float3(0, 1, 0), dir));
                if (math.length(faceNormal) < 0.001f)
                    faceNormal = math.normalizesafe(math.cross(up, dir));

                var h = GetHeightAtT(spline, heightDataIndex, t);

                float3 bottomPos, topPos;
                if (m_GrowUpward)
                {
                    bottomPos = pos + new float3(0, m_VerticalOffset, 0);
                    topPos = pos + new float3(0, h + m_VerticalOffset, 0);
                }
                else
                {
                    bottomPos = pos + new float3(0, -h * 0.5f + m_VerticalOffset, 0);
                    topPos = pos + new float3(0, h * 0.5f + m_VerticalOffset, 0);
                }

                float u = accumulatedLength * m_TextureScaleU;

                m_Positions.Add(bottomPos);
                m_Positions.Add(topPos);
                m_Normals.Add(faceNormal);
                m_Normals.Add(faceNormal);
                m_Textures.Add(new Vector2(u, 0f));
                m_Textures.Add(new Vector2(u, 1f * m_TextureScaleV));

                t = math.min(1f, t + segmentStepT);
            }

            for (int i = 0, n = prevVertexCount; i < segments; i++, n += 2)
            {
                int b = n, tp = n + 1;
                int nb = n + 2, ntp = n + 3;

                m_SideIndices.Add(b);  m_SideIndices.Add(tp);  m_SideIndices.Add(ntp);
                m_SideIndices.Add(b);  m_SideIndices.Add(ntp); m_SideIndices.Add(nb);
            }
        }

        float GetHeightAtT(Spline spline, int heightDataIndex, float t)
        {
            var h = 3f;
            if (heightDataIndex < m_Heights.Count)
            {
                h = m_Heights[heightDataIndex].DefaultValue;
                if (m_Heights[heightDataIndex] != null && m_Heights[heightDataIndex].Count > 0)
                {
                    h = m_Heights[heightDataIndex].Evaluate(spline, t, PathIndexUnit.Normalized, new Interpolators.LerpFloat());
                    h = math.clamp(h, 0.01f, 10000f);
                }
            }
            return h;
        }

        void BuildTopCap(Spline spline, int heightDataIndex)
        {
            if (spline == null || spline.Count < 3)
                return;

            if (!spline.Closed)
                return; // top cap only makes sense for closed splines

            float length = spline.GetLength();
            if (length <= 0.001f)
                return;

            // Sample points around the spline boundary at the top height
            int perimeterSamples = Mathf.Max(m_TopCapResolution, 8);
            float stepT = 1f / perimeterSamples;

            List<Vector3> boundaryPoints = new List<Vector3>(perimeterSamples);
            List<float> boundaryHeights = new List<float>(perimeterSamples);

            for (int i = 0; i < perimeterSamples; i++)
            {
                float t = i * stepT;
                SplineUtility.Evaluate(spline, t, out var pos, out _, out _);
                var h = GetHeightAtT(spline, heightDataIndex, t);

                float topY;
                if (m_GrowUpward)
                    topY = pos.y + h + m_VerticalOffset - m_TopSinkAmount;
                else
                    topY = pos.y + h * 0.5f + m_VerticalOffset - m_TopSinkAmount;

                boundaryPoints.Add(new Vector3(pos.x, topY, pos.z));
                boundaryHeights.Add(topY);
            }

            // Triangulate the polygon using ear clipping
            var triangles = TriangulatePolygon(boundaryPoints);
            if (triangles == null || triangles.Count == 0)
                return;

            // Add vertices for the top cap
            int capVertexStart = m_Positions.Count;
            Vector3 upNormal = Vector3.up;

            for (int i = 0; i < boundaryPoints.Count; i++)
            {
                var p = boundaryPoints[i];
                m_Positions.Add(p);
                m_Normals.Add(upNormal);
                m_Textures.Add(new Vector2(p.x * m_TopTextureScale, p.z * m_TopTextureScale));
            }

            // Add triangle indices for submesh 1 (top cap)
            for (int i = 0; i < triangles.Count; i++)
            {
                m_TopIndices.Add(triangles[i] + capVertexStart);
            }
        }

        /// <summary>
        /// Simple ear-clipping triangulation for a 2D polygon projected onto XZ plane.
        /// Returns a list of triangle indices into the input points list.
        /// </summary>
        List<int> TriangulatePolygon(List<Vector3> points)
        {
            if (points.Count < 3)
                return null;

            var result = new List<int>();
            var indices = new List<int>(points.Count);
            
            // Determine winding order using signed area on XZ plane
            float signedArea = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];
                signedArea += (b.x - a.x) * (b.z + a.z);
            }

            // If clockwise, reverse so we get counter-clockwise (standard for ear clipping)
            if (signedArea > 0)
            {
                for (int i = points.Count - 1; i >= 0; i--)
                    indices.Add(i);
            }
            else
            {
                for (int i = 0; i < points.Count; i++)
                    indices.Add(i);
            }

            int safetyCounter = indices.Count * indices.Count;
            while (indices.Count > 2 && safetyCounter-- > 0)
            {
                bool earFound = false;

                for (int i = 0; i < indices.Count; i++)
                {
                    int prevIdx = (i - 1 + indices.Count) % indices.Count;
                    int nextIdx = (i + 1) % indices.Count;

                    int a = indices[prevIdx];
                    int b = indices[i];
                    int c = indices[nextIdx];

                    Vector2 pa = new Vector2(points[a].x, points[a].z);
                    Vector2 pb = new Vector2(points[b].x, points[b].z);
                    Vector2 pc = new Vector2(points[c].x, points[c].z);

                    // Check if this is a convex vertex (ear candidate)
                    float cross = Cross2D(pa, pb, pc);
                    if (cross <= 0f)
                        continue;

                    // Check no other vertex is inside this triangle
                    bool containsPoint = false;
                    for (int j = 0; j < indices.Count; j++)
                    {
                        if (j == prevIdx || j == i || j == nextIdx)
                            continue;

                        Vector2 pp = new Vector2(points[indices[j]].x, points[indices[j]].z);
                        if (PointInTriangle(pp, pa, pb, pc))
                        {
                            containsPoint = true;
                            break;
                        }
                    }

                    if (containsPoint)
                        continue;

                    // This is an ear — clip it
                    result.Add(a);
                    result.Add(b);
                    result.Add(c);

                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }

                if (!earFound)
                    break; // degenerate polygon
            }

            return result;
        }

        static float Cross2D(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }

        static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross2D(a, b, p);
            float d2 = Cross2D(b, c, p);
            float d3 = Cross2D(c, a, p);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }
    }
}