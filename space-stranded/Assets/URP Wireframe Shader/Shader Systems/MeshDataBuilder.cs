using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace URP_Wireframe_Shader.Shader_Systems
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshDataBuilder : MonoBehaviour
    {
        public ProcessedMeshData processedData;
        private bool isInitialized = false;
        private Mesh generatedMesh;

        private void OnEnable()
        {
            if (!isInitialized && processedData != null)
            {
                ApplyProcessedData();
            }
        }

        private void Start()
        {
            if (!isInitialized && processedData != null)
            {
                ApplyProcessedData();
            }
        }

        private void OnDisable()
        {
            if (generatedMesh != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedMesh);
                else
                    DestroyImmediate(generatedMesh);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        isInitialized = false;
                        ApplyProcessedData();
                    }
                };
            }
        }
#endif

        private void ApplyProcessedData()
        {
            if (processedData == null)
            {
                Debug.LogError($"[MeshDataBuilder] No processed data assigned on {gameObject.name}!");
                return;
            }

            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogError($"[MeshDataBuilder] No MeshFilter component found on {gameObject.name}!");
                return;
            }

            // Clean up old mesh if it exists
            if (generatedMesh != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedMesh);
                else
                    DestroyImmediate(generatedMesh);
            }

            // Create new mesh
            generatedMesh = new Mesh();
            generatedMesh.name = "ProcessedMesh_" + gameObject.name;

            // Apply vertex data
            generatedMesh.vertices = processedData.vertices;
            generatedMesh.triangles = processedData.triangles;

            // Apply UVs if they exist
            if (processedData.uv != null && processedData.uv.Length > 0)
            {
                generatedMesh.uv = processedData.uv;
            }

            // Apply normals if they exist
            if (processedData.normals != null && processedData.normals.Length > 0)
            {
                generatedMesh.normals = processedData.normals;
            }
            else
            {
                generatedMesh.RecalculateNormals();
            }

            // Apply colors for barycentric coordinates
            generatedMesh.colors32 = processedData.colors;

            // Ensure proper bounds
            generatedMesh.RecalculateBounds();

            // Assign the mesh
            meshFilter.sharedMesh = generatedMesh;
            isInitialized = true;

            //Debug.Log($"[MeshDataBuilder] Successfully applied processed mesh data on {gameObject.name}. Vertices: {generatedMesh.vertexCount}");
        }

        public void ForceReprocess()
        {
            isInitialized = false;
            ApplyProcessedData();
        }

        private void OnDestroy()
        {
            // Clean up generated mesh
            if (generatedMesh != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedMesh);
                else
                    DestroyImmediate(generatedMesh);
            }
        }
    }
}