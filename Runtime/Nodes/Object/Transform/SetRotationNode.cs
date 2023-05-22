using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Object.Transform
{
    [Node(
        TitleName = "Set Rotation",
        Category = "Object/Transform",
        Color = Color.Yellow,
        InputPortName = "Set",
        InputPortType = typeof(UnityEngine.Transform),
        OutputPortNames = new []{ "Finished" },
        OutputPortTypes = new []{ typeof(UnityEngine.Transform) }
    )]
    public class SetRotationNode : JungleNode
    {
        #region Variables
        
        [SerializeField] 
        private Vector3 rotation;
        
        [SerializeField] 
        private Space space = Space.World;

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
                if (space == Space.World)
                {
                    _transform.eulerAngles = rotation;
                }
                else
                {
                    _transform.localEulerAngles = rotation;
                }
                call = new[]
                {
                    new PortCall(0, _transform)
                };
                return true;
            }
            
            var distance = Vector3.Distance(space == Space.World 
                ? _transform.eulerAngles 
                : _transform.localEulerAngles
                , rotation);
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
                    if (space == Space.World)
                    {
                        _transform.eulerAngles = Vector3.Lerp(_transform.eulerAngles, rotation, deltaTime * rate);
                    }
                    else
                    {
                        _transform.localEulerAngles = Vector3.Lerp(_transform.localEulerAngles, rotation, deltaTime * rate);
                    }
                    break;
                case Method.Slerp:
                    if (space == Space.World)
                    {
                        _transform.eulerAngles = Vector3.Slerp(_transform.eulerAngles, rotation, deltaTime * rate);
                    }
                    else
                    {
                        _transform.localEulerAngles = Vector3.Slerp(_transform.localEulerAngles, rotation, deltaTime * rate);
                    }
                    break;
                case Method.MoveTowards:
                    if (space == Space.World)
                    {
                        _transform.eulerAngles = Vector3.MoveTowards(_transform.eulerAngles, rotation, deltaTime * rate);
                    }
                    else
                    {
                        _transform.localEulerAngles = Vector3.MoveTowards(_transform.localEulerAngles, rotation, deltaTime * rate);
                    }
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
    [CustomEditor(typeof(SetRotationNode))]
    public class SetRotationNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private SerializedProperty _rotation;
        private SerializedProperty _space;
        private SerializedProperty _overTime;
        private SerializedProperty _method;
        private SerializedProperty _rate;
        private SerializedProperty _scaledTime;

        #endregion

        private void OnEnable()
        {
            _rotation = serializedObject.FindProperty("rotation");
            _space = serializedObject.FindProperty("space");
            _overTime = serializedObject.FindProperty("overTime");
            _method = serializedObject.FindProperty("method");
            _rate = serializedObject.FindProperty("rate");
            _scaledTime = serializedObject.FindProperty("scaledTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_rotation);
            EditorGUILayout.PropertyField(_space);
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
