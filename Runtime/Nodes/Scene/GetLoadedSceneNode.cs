using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Scene
{
    [Node(
        Title = "Get Loaded Scene",
        Category = "Scene",
        Color = Color.Teal,
        InputPortName = "Get",
        OutputPortNames = new []{ "Found" },
        OutputPortTypes = new []{ typeof(UnityEngine.SceneManagement.Scene) }
    )]
    public class GetLoadedSceneNode : JungleNode
    {
        #region Variables

        [SerializeField] 
        private GetSceneMethod getMethod = GetSceneMethod.ByName;

        [SerializeField] 
        private string sceneName;

        [SerializeField] 
        private int sceneIndex;

        [SerializeField] 
        private bool cacheScene = true;
        
        [NonSerialized] 
        private UnityEngine.SceneManagement.Scene _scene;
        
        private enum GetSceneMethod
        {
            ByName,
            ByIndex
        }

        #endregion
        
        public override void Initialize(in object inputValue)
        {
            if (cacheScene && _scene.IsValid())
            {
                return;
            }
            switch (getMethod)
            {
                case GetSceneMethod.ByName:
                    _scene = SceneManager.GetSceneByName(sceneName);
#if UNITY_EDITOR
                    if (!_scene.IsValid())
                    {
                        Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, Tree,
                            $"[{name}] Failed to get scene by name \"{sceneName}\".");
                    }
#endif
                    break;
                case GetSceneMethod.ByIndex:
                    _scene = SceneManager.GetSceneAt(sceneIndex);
#if UNITY_EDITOR
                    if (!_scene.IsValid())
                    {
                        Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, Tree,
                            $"[{name}] Failed to get scene at index {sceneIndex.ToString()}.");
                    }
#endif
                    break;
            }
        }

        public override bool Execute(out PortCall[] call)
        {
            call = Array.Empty<PortCall>();
            if (_scene.IsValid())
            {
                call = new[]
                {
                    new PortCall(0, _scene)
                };
            }
            return true;
        }
        
        private void OnValidate()
        {
            sceneIndex = (int)Mathf.Clamp(sceneIndex, 0, Mathf.Infinity);
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(GetLoadedSceneNode))]
    public class GetLoadedSceneNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private SerializedProperty _getMethod;
        private SerializedProperty _sceneName;
        private SerializedProperty _sceneIndex;
        private SerializedProperty _cacheScene;

        #endregion

        private void OnEnable()
        {
            _getMethod = serializedObject.FindProperty("getMethod");
            _sceneName = serializedObject.FindProperty("sceneName");
            _sceneIndex = serializedObject.FindProperty("sceneIndex");
            _cacheScene = serializedObject.FindProperty("cacheScene");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_getMethod);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            if (_getMethod.enumValueFlag == 0)
            {
                EditorGUILayout.PropertyField(_sceneName);
            }
            else if (_getMethod.enumValueFlag == 1)
            {
                EditorGUILayout.PropertyField(_sceneIndex);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(_cacheScene);
            if (!_cacheScene.boolValue)
            {
                EditorGUILayout.HelpBox("Not caching the scene can lead to slower execution if this node " +
                                        "is called multiple times.", MessageType.Warning);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}