using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Object.GameObject
{
    [Node(
        TitleName = "Get Game Object",
        Category = "Object",
        Color = Color.Teal,
        InputPortName = "Find",
        OutputPortNames = new []{ "Found" },
        OutputPortTypes = new []{ typeof(UnityEngine.GameObject) }
    )]
    public class GetGameObjectNode : JungleNode
    {
        #region Variables

        [SerializeField] 
        private FindMethod findMethod = FindMethod.ByName;
        
        [SerializeField] 
        private string gameObjectName;

        [SerializeField] 
        private string gameObjectTag;
        
        [SerializeField] 
        private bool cacheGameObject = true;
        
        [NonSerialized] 
        private UnityEngine.GameObject _gameObject;

        private enum FindMethod
        {
            ByName,
            ByTag
        }
        
        #endregion
        
        public override void Initialize(in object inputValue)
        {
            if (cacheGameObject && _gameObject != null)
            {
                return;
            }
            
            if (findMethod == FindMethod.ByName)
            {
                if (string.IsNullOrEmpty(gameObjectName))
                {
#if UNITY_EDITOR
                    Debug.LogError($"[{name}] Failed to find game object by name because the game object" +
                                   " name to find was null/empty.");
#endif
                    return;
                }
                _gameObject = UnityEngine.GameObject.Find(gameObjectName);
#if UNITY_EDITOR
                if (_gameObject == null)
                {
                    Debug.LogError($"[{name}] Failed to find game object by name \"{gameObjectName}\"");
                }
#endif
            }
            else if (findMethod == FindMethod.ByTag)
            {
                if (string.IsNullOrEmpty(gameObjectTag))
                {
#if UNITY_EDITOR
                    Debug.LogError($"[{name}] Failed to find game object by tag because the game object" +
                                   " tag to find was null/empty.");
#endif
                    return;
                }
                _gameObject = UnityEngine.GameObject.FindWithTag(gameObjectTag);
#if UNITY_EDITOR
                if (_gameObject == null)
                {
                    Debug.LogError($"[{name}] Failed to find game object with tag \"{gameObjectTag}\"");
                }
#endif
            }
        }

        public override bool Execute(out PortCall[] call)
        {
            call = Array.Empty<PortCall>();
            if (_gameObject != null)
            {
                call = new[]
                {
                    new PortCall(0, _gameObject)
                };
            }
            return true;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(GetGameObjectNode))]
    public class GetGameObjectNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private SerializedProperty _findMethod;
        private SerializedProperty _gameObjectName;
        private SerializedProperty _gameObjectTag;
        private SerializedProperty _cacheGameObject;

        #endregion

        private void OnEnable()
        {
            _findMethod = serializedObject.FindProperty("findMethod");
            _gameObjectName = serializedObject.FindProperty("gameObjectName");
            _gameObjectTag = serializedObject.FindProperty("gameObjectTag");
            _cacheGameObject = serializedObject.FindProperty("cacheGameObject");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_findMethod);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            if (_findMethod.enumValueFlag == 0)
            {
                EditorGUILayout.PropertyField(_gameObjectName);
            }
            else
            {
                EditorGUILayout.PropertyField(_gameObjectTag);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(_cacheGameObject);
            if (!_cacheGameObject.boolValue)
            {
                EditorGUILayout.HelpBox("Not caching the game object can lead to slower execution if this node " +
                                        "is called multiple times.", MessageType.Warning);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
