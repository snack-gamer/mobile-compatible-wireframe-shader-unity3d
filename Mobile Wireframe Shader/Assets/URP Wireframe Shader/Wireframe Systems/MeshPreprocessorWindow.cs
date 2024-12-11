#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace URP_Wireframe_Shader.Wireframe_Systems
{
    public class MeshPreprocessorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private GameObject sourceParent;
        private string outputPath = "Assets/ProcessedMeshes";
        private bool autoAddComponents = true;
        private List<MeshFilter> foundMeshFilters = new List<MeshFilter>();
        private bool isProcessing = false;
        private GUIStyle headerStyle;

        [MenuItem("Tools/Wireframe Shader Tool/Mesh Preprocessor")]
        static void ShowWindow()
        {
            var window = GetWindow<MeshPreprocessorWindow>("<b>Mesh Preprocessor</b>");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }
        

        private void OnGUI()
        {
            InitializeStyleIfNeeded();
            EditorGUILayout.Space(10);
            DrawHeader();
            EditorGUILayout.Space(10);
            DrawSourceSelection();
            EditorGUILayout.Space(10);
            DrawOutputSettings();
            EditorGUILayout.Space(10);
            DrawProcessingOptions();
            EditorGUILayout.Space(10);
            DrawMeshList();
            EditorGUILayout.Space(10);
            DrawProcessButton();
            EditorGUILayout.Space(10);
            DrawStatusBar();

        }
        
        private void InitializeStyleIfNeeded()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }

        private void DrawHeader()
        {
            // Add null check for extra safety
            if (headerStyle == null)
            {
                InitializeStyleIfNeeded();
            }

            EditorGUILayout.BeginHorizontal();
            try 
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Mesh Preprocessor", headerStyle);
                GUILayout.FlexibleSpace();
            }
            finally 
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawSourceSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Source Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            sourceParent = EditorGUILayout.ObjectField("Source Parent", sourceParent, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                RefreshMeshList();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Mesh List"))
            {
                RefreshMeshList();
            }
            if (GUILayout.Button("Find All In Scene"))
            {
                ProcessAllInScene();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawOutputSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Output Settings", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        outputPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!Directory.Exists(outputPath))
            {
                EditorGUILayout.HelpBox("Output directory doesn't exist! It will be created during processing.", 
                    MessageType.Warning);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawProcessingOptions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Processing Options", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                autoAddComponents = EditorGUILayout.Toggle(
                    new GUIContent("Auto-Add Components", 
                        "Automatically add MeshDataBuilder and required components"),
                    autoAddComponents);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawMeshList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Found Meshes", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            if (foundMeshFilters.Count == 0)
            {
                EditorGUILayout.HelpBox("No meshes found. Select a parent object or use 'Find All In Scene'.", 
                    MessageType.Info);
            }
            else
            {
                string currentPath = "";
                foreach (var meshFilter in foundMeshFilters)
                {
                    if (meshFilter == null) continue;

                    // Show hierarchy path
                    string hierarchyPath = BuildHierarchyPath(meshFilter.transform);
                    string parentPath = Path.GetDirectoryName(hierarchyPath)?.Replace('\\', '/');

                    // If we've entered a new parent path, show it as a header
                    if (parentPath != currentPath)
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField(parentPath ?? "Root", EditorStyles.boldLabel);
                        currentPath = parentPath;
                    }

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    // Mesh info with indentation
                    EditorGUILayout.BeginVertical();
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.LabelField($"Mesh: {meshFilter.name}", EditorStyles.boldLabel);
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.LabelField($"Vertices: {meshFilter.sharedMesh?.vertexCount ?? 0}");
                            EditorGUILayout.LabelField($"Triangles: {(meshFilter.sharedMesh?.triangles.Length ?? 0) / 3}");
                            if (meshFilter.GetComponent<MeshRenderer>()?.sharedMaterial != null)
                            {
                                EditorGUILayout.LabelField($"Material: {meshFilter.GetComponent<MeshRenderer>().sharedMaterial.name}");
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(2);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawProcessButton()
        {
            EditorGUI.BeginDisabledGroup(isProcessing || foundMeshFilters.Count == 0);
            if (GUILayout.Button("Process Meshes", GUILayout.Height(30)))
            {
                ProcessMeshes();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawStatusBar()
        {
            if (isProcessing)
            {
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(0, 20), 0.5f, "Processing...");
            }
        }

        private void ProcessMeshes()
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            isProcessing = true;
            int processed = 0;

            try
            {
                foreach (var meshFilter in foundMeshFilters)
                {
                    if (meshFilter == null || meshFilter.sharedMesh == null) continue;

                    EditorUtility.DisplayProgressBar("Processing Meshes",
                        $"Processing {meshFilter.name} ({processed}/{foundMeshFilters.Count})",
                        processed / (float)foundMeshFilters.Count);

                    ProcessMeshFilter(meshFilter);
                    processed++;
                }

                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Success",
                    $"Successfully processed {processed} meshes.\nOutput saved to: {outputPath}",
                    "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error processing meshes: {e.Message}");
                EditorUtility.DisplayDialog("Error",
                    $"Error processing meshes: {e.Message}", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                isProcessing = false;
            }
        }

        private void ProcessMeshFilter(MeshFilter meshFilter)
        {
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogError($"Invalid mesh filter or missing mesh!");
                return;
            }

            Mesh originalMesh = meshFilter.sharedMesh;
            string fileName = GetProcessedFileName(meshFilter);
            string fullPath = Path.Combine(outputPath, fileName);

            ProcessedMeshData processedData = ScriptableObject.CreateInstance<ProcessedMeshData>();

            // Get the original mesh data
            Vector3[] originalVertices = originalMesh.vertices;
            int[] originalTriangles = originalMesh.triangles;
            Vector2[] originalUVs = originalMesh.uv;
            Vector3[] originalNormals = originalMesh.normals;

            // Create new arrays for the processed mesh
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector2> newUVs = new List<Vector2>();
            List<Vector3> newNormals = new List<Vector3>();
            List<Color32> newColors = new List<Color32>();
            List<int> newTriangles = new List<int>();

            // Process each triangle
            for (int i = 0; i < originalTriangles.Length; i += 3)
            {
                // Get indices for this triangle
                int idx0 = originalTriangles[i];
                int idx1 = originalTriangles[i + 1];
                int idx2 = originalTriangles[i + 2];

                // Add vertices
                newVertices.Add(originalVertices[idx0]);
                newVertices.Add(originalVertices[idx1]);
                newVertices.Add(originalVertices[idx2]);

                // Add UVs if they exist
                if (originalUVs != null && originalUVs.Length > 0)
                {
                    newUVs.Add(originalUVs[idx0]);
                    newUVs.Add(originalUVs[idx1]);
                    newUVs.Add(originalUVs[idx2]);
                }

                // Add normals if they exist
                if (originalNormals != null && originalNormals.Length > 0)
                {
                    newNormals.Add(originalNormals[idx0]);
                    newNormals.Add(originalNormals[idx1]);
                    newNormals.Add(originalNormals[idx2]);
                }

                // Add barycentric coordinates as colors
                newColors.Add(new Color32(255, 0, 0, 255));   // (1, 0, 0)
                newColors.Add(new Color32(0, 255, 0, 255));   // (0, 1, 0)
                newColors.Add(new Color32(0, 0, 255, 255));   // (0, 0, 1)

                // Add triangle indices
                int baseIndex = i;
                newTriangles.Add(baseIndex);
                newTriangles.Add(baseIndex + 1);
                newTriangles.Add(baseIndex + 2);
            }

            // Store the processed data
            processedData.vertices = newVertices.ToArray();
            processedData.triangles = newTriangles.ToArray();
            processedData.uv = newUVs.ToArray();
            processedData.normals = newNormals.ToArray();
            processedData.colors = newColors.ToArray();

            // Save the processed data
            AssetDatabase.CreateAsset(processedData, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (autoAddComponents)
            {
                EnsureComponentSetup(meshFilter, processedData);
            }
        }

        private string GetProcessedFileName(MeshFilter meshFilter)
        {
            // Get a unique identifier based on instance ID
            int instanceId = meshFilter.gameObject.GetInstanceID();
            
            // Create a safe name combining object name and unique ID
            string safeName = meshFilter.gameObject.name.Replace(' ', '_');
            return $"{safeName}_{instanceId}_ProcessedData.asset";
        }

        private void ProcessAllInScene()
        {
            foundMeshFilters.Clear();
            var allMeshFilters = Resources.FindObjectsOfTypeAll<MeshFilter>();
            foreach (var meshFilter in allMeshFilters)
            {
                if (meshFilter != null && meshFilter.gameObject.scene.isLoaded)
                {
                    foundMeshFilters.Add(meshFilter);
                }
            }
            Repaint();
        }

        private void EnsureComponentSetup(MeshFilter meshFilter, ProcessedMeshData processedData)
        {
            if (meshFilter == null || processedData == null)
            {
                Debug.LogError("Invalid meshFilter or processedData in EnsureComponentSetup!");
                return;
            }

            Undo.RecordObject(meshFilter.gameObject, "Setup Mesh Components");

            var existingBuilder = meshFilter.GetComponent<MeshDataBuilder>();
            if (existingBuilder != null)
            {
                Undo.DestroyObjectImmediate(existingBuilder);
            }

            var meshDataBuilder = Undo.AddComponent<MeshDataBuilder>(meshFilter.gameObject);
            if (meshDataBuilder != null)
            {
                meshDataBuilder.processedData = processedData;
                EditorUtility.SetDirty(meshFilter.gameObject);
                EditorUtility.SetDirty(meshDataBuilder);
            }
            else
            {
                Debug.LogError($"Failed to add MeshDataBuilder to {meshFilter.gameObject.name}");
            }
        }

        private void RefreshMeshList()
        {
            foundMeshFilters.Clear();

            if (sourceParent != null)
            {
                foundMeshFilters.AddRange(sourceParent.GetComponentsInChildren<MeshFilter>(true));
                foundMeshFilters.Sort((a, b) => String.Compare(GetFullPath(a.transform), GetFullPath(b.transform), StringComparison.Ordinal));
            }
            Repaint();
        }

        private string GetFullPath(Transform transform)
        {
            if (transform == null) return "";
            string path = transform.name;
            Transform parent = transform.parent;
            while (parent != null)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }
            return path;
        }

        private string BuildHierarchyPath(Transform transform)
        {
            List<string> pathParts = new List<string>();
            Transform current = transform;
            while (current != null && current != sourceParent)
            {
                pathParts.Add(current.name);
                current = current.parent;
            }
            pathParts.Reverse();
            return string.Join("/", pathParts);
        }
    }
}
#endif