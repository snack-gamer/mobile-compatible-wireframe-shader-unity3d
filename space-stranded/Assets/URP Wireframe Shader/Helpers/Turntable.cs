using UnityEngine;

namespace URP_Wireframe_Shader.Helpers
{
    public class Turntable : MonoBehaviour
    {
        [Header("Rotation Settings")]
        public float rotationSpeed = 90f;
        public Vector3 rotationAxis = Vector3.up;
        public bool isRotating = true;
        [Header("Optional Settings")]
        public bool normalizeAxis = true;
        public bool autoStart = true;
    
        private void Start()
        {
            if (normalizeAxis)
            {
                rotationAxis.Normalize();
            }
        
            if (!autoStart)
            {
                isRotating = false;
            }
        }
    
        private void Update()
        {
            if (isRotating)
            {
                transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
            }
        }
        
        public void StartRotation()
        {
            isRotating = true;
        }
        
        public void StopRotation()
        {
            isRotating = false;
        }
        
        public void ToggleRotation()
        {
            isRotating = !isRotating;
        }
        
        public void SetSpeed(float speed)
        {
            rotationSpeed = speed;
        }
        
        public void SetAxis(Vector3 axis)
        {
            rotationAxis = normalizeAxis ? axis.normalized : axis;
        }
    }
}