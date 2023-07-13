using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Object.Transform
{
    [NodeProperties(
        Title = "Set Position",
        Tooltip = "Sets a transforms position.",
        Category = "Object/Transform",
        Color = Yellow
    )]
    [BranchNode(
        InputPortName = "Set",
        InputPortType = typeof(UnityEngine.Transform),
        OutputPortNames = new []{ "Finished" },
        OutputPortTypes = new []{ typeof(UnityEngine.Transform) }
    )]
    public class SetPositionNode : BranchNode
    {
        #region Variables
        
        [SerializeField] 
        private Vector3 position;
        
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

        [NonSerialized]
        private Vector3 _originalPosition;
        
        #endregion

        public override void OnStart(in object inputValue)
        {
            _transform = inputValue as UnityEngine.Transform;
            _originalPosition = space == Space.World 
                ? _transform.position
                : _transform.localPosition;
            Tree.AddRevertAction(RevertPosition);
        }
        
        public override void OnUpdate()
        {
            if (!overTime)
            {
                if (space == Space.World)
                {
                    _transform.position = position;
                }
                else
                {
                    _transform.localPosition = position;
                }
                CallAndStop(new[]
                {
                    new PortCall(0, _transform)
                });
                return;
            }
            
            var distance = Vector3.Distance(space == Space.World 
                ? _transform.position 
                : _transform.localPosition
                , position);
            if (distance < 0.01f)
            {
                CallAndStop(new[]
                {
                    new PortCall(0, _transform)
                });
                return;
            }

            var deltaTime = scaledTime
                ? UnityEngine.Time.deltaTime
                : UnityEngine.Time.unscaledDeltaTime;
            switch (method)
            {
                case Method.Lerp:
                    if (space == Space.World)
                    {
                        _transform.position = Vector3.Lerp(_transform.position, position, deltaTime * rate);
                    }
                    else
                    {
                        _transform.localPosition = Vector3.Lerp(_transform.localPosition, position, deltaTime * rate);
                    }
                    break;
                case Method.Slerp:
                    if (space == Space.World)
                    {
                        _transform.position = Vector3.Slerp(_transform.position, position, deltaTime * rate);
                    }
                    else
                    {
                        _transform.localPosition = Vector3.Slerp(_transform.localPosition, position, deltaTime * rate);
                    }
                    break;
                case Method.MoveTowards:
                    if (space == Space.World)
                    {
                        _transform.position = Vector3.MoveTowards(_transform.position, position, deltaTime * rate);
                    }
                    else
                    {
                        _transform.localPosition = Vector3.MoveTowards(_transform.localPosition, position, deltaTime * rate);
                    }
                    break;
            }
        }

        private void RevertPosition()
        {
            if (space == Space.World)
            {
                _transform.position = _originalPosition;
            }
            else if (space == Space.Local)
            {
                _transform.localPosition = _originalPosition;
            }
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
    [CustomEditor(typeof(SetPositionNode))]
    public class SetPositionNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private SerializedProperty _position;
        private SerializedProperty _space;
        private SerializedProperty _overTime;
        private SerializedProperty _method;
        private SerializedProperty _rate;
        private SerializedProperty _scaledTime;

        #endregion

        private void OnEnable()
        {
            _position = serializedObject.FindProperty("position");
            _space = serializedObject.FindProperty("space");
            _overTime = serializedObject.FindProperty("overTime");
            _method = serializedObject.FindProperty("method");
            _rate = serializedObject.FindProperty("rate");
            _scaledTime = serializedObject.FindProperty("scaledTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_position);
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
