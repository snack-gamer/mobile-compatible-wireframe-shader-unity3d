using UnityEngine;

namespace URP_Wireframe_Shader.Shader_Systems
{
    [CreateAssetMenu(fileName = "WireframeMeshData", menuName = "Mesh/Wireframe Mesh Data")]
    public class ProcessedMeshData : ScriptableObject
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uv;
        public Vector3[] normals;
        public Color32[] colors;
        
    }
}