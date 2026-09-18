using System.Collections.Generic;
using UnityEngine;

namespace AliZombieDrive
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public sealed class ProceduralRoad : MonoBehaviour
    {
        [SerializeField] private int segments = 220;
        [SerializeField] private float segmentLength = 10f;
        [SerializeField] private float halfWidth = 6.2f;
        [SerializeField] private float curveAmplitude = 18f;
        [SerializeField] private float curveFrequency = 0.028f;
        [SerializeField] private float elevationAmplitude = 2.5f;
        [SerializeField] private float elevationFrequency = 0.018f;

        public float Length => segments * segmentLength;

        private void Awake() => Build();

        [ContextMenu("Rebuild Road")]
        public void Build()
        {
            Mesh mesh = new Mesh { name = "Ali_ProceduralRoad" };
            var vertices = new List<Vector3>((segments + 1) * 2);
            var uvs = new List<Vector2>((segments + 1) * 2);
            var tris = new List<int>(segments * 6);

            for (int i = 0; i <= segments; i++)
            {
                float z = i * segmentLength;
                Vector3 center = CenterAt(z);
                Vector3 forward = (CenterAt(z + 1f) - CenterAt(Mathf.Max(0f, z - 1f))).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                vertices.Add(center - right * halfWidth);
                vertices.Add(center + right * halfWidth);
                float v = i / 6f;
                uvs.Add(new Vector2(0f, v));
                uvs.Add(new Vector2(1f, v));

                if (i < segments)
                {
                    int b = i * 2;
                    tris.Add(b); tris.Add(b + 2); tris.Add(b + 1);
                    tris.Add(b + 1); tris.Add(b + 2); tris.Add(b + 3);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        public Vector3 CenterAt(float z)
        {
            float x = Mathf.Sin(z * curveFrequency) * curveAmplitude + Mathf.Sin(z * curveFrequency * 0.41f + 1.2f) * curveAmplitude * 0.35f;
            float y = Mathf.Sin(z * elevationFrequency) * elevationAmplitude;
            return new Vector3(x, y, z);
        }
    }
}
