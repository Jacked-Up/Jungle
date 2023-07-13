using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Scene
{
    [NodeProperties(
        Title = "Get Active Scene",
        Category = "Scene",
        Color = Teal,
        InputPortName = "Get",
        OutputPortNames = new []{ "Found" },
        OutputPortTypes = new []{ typeof(UnityEngine.SceneManagement.Scene) }
    )]
    public class GetActiveSceneNode : JungleNode
    {
        #region Variables
        
        [SerializeField] 
        private bool cacheScene = true;
        
        [NonSerialized] 
        private UnityEngine.SceneManagement.Scene _scene;

        #endregion
        
        public override void Initialize(in object inputValue)
        {
            if (cacheScene && _scene.IsValid())
            {
                return;
            }
            _scene = SceneManager.GetActiveScene();
            
#if UNITY_EDITOR
            if (!_scene.IsValid())
            {
                Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, Tree,
                    $"[{name}] Failed to get active scene.");
            }
#endif
        }

        public override bool Execute(out PortCall[] call)
        {
            if (_scene.IsValid())
            {
                call = new[]
                {
                    new PortCall(0, _scene)
                };
                return true;
            }
            call = Array.Empty<PortCall>();
            return false;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(GetActiveSceneNode))]
    public class GetActiveSceneNodeEditor : UnityEditor.Editor
    {
        #region Variables
        
        private SerializedProperty _cacheScene;

        #endregion

        private void OnEnable()
        {
            _cacheScene = serializedObject.FindProperty("cacheScene");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
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
