using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Object.Transform
{
    [NodeProperties(
        Title = "Set Scale",
        Category = "Object/Transform",
        Color = Yellow,
        InputPortName = "Set",
        InputPortType = typeof(UnityEngine.Transform),
        OutputPortNames = new []{ "Finished" },
        OutputPortTypes = new []{ typeof(UnityEngine.Transform) }
    )]
    public class SetScaleNode : JungleNode
    {
        #region Variables
        
        [SerializeField] 
        private Vector3 scale = Vector3.one;

        [SerializeField] 
        private bool overTime;

        [SerializeField] 
        private Method method = Method.Lerp;

        [SerializeField]
        private float rate = 1f;
        
        [SerializeField] 
        private bool scaledTime = true;
        
        [NonSerialized] 
        private UnityEngine.Transform _transform;

        #endregion
        
        public override void Initialize(in object inputValue)
        {
            _transform = inputValue as UnityEngine.Transform;
        }

        public override bool Execute(out PortCall[] call)
        {                
            call = Array.Empty<PortCall>();
            if (!overTime)
            {
                _transform.localScale = scale;
                call = new[]
                {
                    new PortCall(0, _transform)
                };
                return true;
            }
            
            var distance = Vector3.Distance(_transform.localScale, scale);
            if (distance < 0.01f)
            {
                call = new[]
                {
                    new PortCall(0, _transform)
                };
                return true;
            }

            var deltaTime = scaledTime
                ? UnityEngine.Time.deltaTime
                : UnityEngine.Time.unscaledDeltaTime;
            switch (method)
            {
                case Method.Lerp:
                    _transform.localScale = Vector3.Lerp(_transform.localScale, scale, deltaTime * rate);
                    break;
                case Method.Slerp:
                    _transform.localScale = Vector3.Slerp(_transform.localScale, scale, deltaTime * rate);
                    break;
                case Method.MoveTowards:
                    _transform.localScale = Vector3.MoveTowards(_transform.localScale, scale, deltaTime * rate);
                    break;
            }
            return false;
        }
        
        private void OnValidate()
        {
            if (rate < 0.01f)
            {
                rate = 0.01f;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SetScaleNode))]
    public class SetScaleNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private SerializedProperty _scale;
        private SerializedProperty _overTime;
        private SerializedProperty _method;
        private SerializedProperty _rate;
        private SerializedProperty _scaledTime;

        #endregion

        private void OnEnable()
        {
            _scale = serializedObject.FindProperty("scale");
            _overTime = serializedObject.FindProperty("overTime");
            _method = serializedObject.FindProperty("method");
            _rate = serializedObject.FindProperty("rate");
            _scaledTime = serializedObject.FindProperty("scaledTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_scale);
            EditorGUILayout.PropertyField(_overTime);
            if (_overTime.boolValue)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                GUILayout.BeginVertical();
                EditorGUILayout.PropertyField(_method);
                EditorGUILayout.PropertyField(_rate);
                EditorGUILayout.PropertyField(_scaledTime);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
