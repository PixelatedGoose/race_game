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
    public class hedgemaxing : MonoBehaviour
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

        [SerializeField, Tooltip("Scales the V coordinate of the UVs along the spline length.")]
        float m_TextureScaleU = 1f;

        [SerializeField, Tooltip("Scales the V coordinate of the UVs along the height.")]
        float m_TextureScaleV = 1f;

        [SerializeField, Tooltip("Vertical offset from the spline. Use to sink the base into the ground.")]
        float m_VerticalOffset = 0f;

        [SerializeField, Tooltip("If true, the bottom edge follows the spline exactly and the mesh grows upward. If false, the mesh is centered on the spline.")]
        bool m_GrowUpward = true;

        public IReadOnlyList<Spline> StripSplines
        {
            get
            {
                if (m_Spline == null)
                    m_Spline = GetComponent<SplineContainer>();

                if (m_Spline == null)
                {
                    Debug.LogError("Cannot loft billboard strip because Spline reference is null");
                    return null;
                }

                return m_Spline.Splines;
            }
        }

        public Mesh StripMesh
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

        List<Vector3> m_Positions = new List<Vector3>();
        List<Vector3> m_Normals = new List<Vector3>();
        List<Vector2> m_Textures = new List<Vector2>();
        List<int> m_Indices = new List<int>();
        bool m_RebuildRequested = false;

        void OnValidate()
        {
            m_RebuildRequested = true;
        }

        void Update()
        {
            if (m_RebuildRequested)
            {
                RebuildAllStrips();
                m_RebuildRequested = false;
            }
        }

        public void OnEnable()
        {
            if (m_Mesh != null)
                m_Mesh = null;

            if (m_Spline == null)
                m_Spline = GetComponent<SplineContainer>();

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
            StripMesh.Clear();
            m_Positions.Clear();
            m_Normals.Clear();
            m_Textures.Clear();
            m_Indices.Clear();
            m_Positions.Capacity = 0;
            m_Normals.Capacity = 0;
            m_Textures.Capacity = 0;
            m_Indices.Capacity = 0;

            for (var i = 0; i < StripSplines.Count; i++)
                BuildStrip(StripSplines[i], i);

            StripMesh.SetVertices(m_Positions);
            StripMesh.SetNormals(m_Normals);
            StripMesh.SetUVs(0, m_Textures);
            StripMesh.subMeshCount = 1;
            StripMesh.SetIndices(m_Indices, MeshTopology.Triangles, 0);
            StripMesh.RecalculateBounds();
            StripMesh.UploadMeshData(false);

            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
        }

        void BuildStrip(Spline spline, int heightDataIndex)
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

            // 2 vertices per step (bottom + top), single sided — shader handles double-sided
            var vertexCount = steps * 2;
            // 2 triangles per segment = 6 indices per segment
            var triangleCount = segments * 6;
            var prevVertexCount = m_Positions.Count;

            m_Positions.Capacity += vertexCount;
            m_Normals.Capacity += vertexCount;
            m_Textures.Capacity += vertexCount;
            m_Indices.Capacity += triangleCount;

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

                m_Indices.Add(b);  m_Indices.Add(tp);  m_Indices.Add(ntp);
                m_Indices.Add(b);  m_Indices.Add(ntp); m_Indices.Add(nb);
            }
        }
    }
}