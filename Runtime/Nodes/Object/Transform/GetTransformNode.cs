using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Object.Transform
{
    [Node(
        TitleName = "Get Transform",
        Category = "Object/Transform",
        Color = Color.Teal,
        InputPortName = "Find",
        InputPortType = typeof(GameObject),
        OutputPortNames = new []{ "Found" },
        OutputPortTypes = new []{ typeof(UnityEngine.Transform) }
    )]
    public class GetTransformNode : Node
    {
        #region Variables

        [SerializeField] 
        private bool cacheTransform = true;
        
        [NonSerialized] 
        private UnityEngine.Transform _transform;
        
        #endregion
        
        public override void Initialize(in object inputValue)
        {
            var gameObject = inputValue as GameObject;
            if (gameObject == null)
            {
                return;
            }
            if (cacheTransform && _transform != null)
            {
                return;
            }
            _transform = gameObject.transform;
        }

        public override bool Execute(out PortCall[] call)
        {
            call = new[]
            {
                new PortCall(0, _transform)
            };
            return true;
        }
    }
    
    public enum Space
    {
        World,
        Local
    }

    public enum Method
    {
        Lerp,
        Slerp,
        MoveTowards
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GetTransformNode))]
    public class GetTransformNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private SerializedProperty _cacheTransform;

        #endregion

        private void OnEnable()
        {
            _cacheTransform = serializedObject.FindProperty("cacheTransform");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_cacheTransform);
            if (!_cacheTransform.boolValue)
            {
                EditorGUILayout.HelpBox("Not caching the transform can lead to slower execution if this node " +
                                        "is called multiple times.", MessageType.Warning);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
