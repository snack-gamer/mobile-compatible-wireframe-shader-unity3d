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

            Mesh mesh = new Mesh();
            mesh.vertices = processedData.vertices;
            mesh.triangles = processedData.triangles;
            mesh.colors32 = processedData.colors;

            meshFilter.sharedMesh = mesh;
            isInitialized = true;

            //Debug.Log($"[MeshDataBuilder] Successfully applied processed mesh data on {gameObject.name}. Vertices: {mesh.vertexCount}");
            
        }

        public void ForceReprocess()
        {
            isInitialized = false;
            ApplyProcessedData();
        }
    }
}