using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Editor
{
    [Node(
        Title = "Debug Draw",
        Category = "Editor", 
        Color = JungleNodeColors.White,
        InputPortName = "Draw"
    )]
    public class DebugDrawNode : JungleNode
    {
        #region Variables

        [SerializeField] 
        private float duration = 1f;
        
        [SerializeField] 
        private UnityEngine.Color color = UnityEngine.Color.red;
        
        [SerializeField] 
        private Type type = Type.Line;

        [SerializeField] 
        private Vector3 startPosition;

        [SerializeField] 
        private Vector3 endPosition;
        
        [SerializeField] 
        private Vector3 direction = Vector3.forward;
        
        private enum Type
        {
            Line,
            Ray
        }

        #endregion
        
        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
        {
#if UNITY_EDITOR
            switch (type)
            {
                case Type.Line:
                    Debug.DrawLine(startPosition, endPosition, color, duration);
                    break;
                case Type.Ray:
                    Debug.DrawRay(startPosition, direction, color, duration);
                    break;
            }
#endif
            call = new[]
            {
                new PortCall(0, true)
            };
            return true;
        }

        private void OnValidate()
        {
            if (duration < 0.01f)
            {
                duration = 0.01f;
            }
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(DebugDrawNode))]
    public class DebugDrawNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private SerializedProperty _duration;
        private SerializedProperty _color;
        private SerializedProperty _type;
        private SerializedProperty _startPosition;
        private SerializedProperty _endPosition;
        private SerializedProperty _direction;

        #endregion

        private void OnEnable()
        {
            _duration = serializedObject.FindProperty("duration");
            _color = serializedObject.FindProperty("color");
            _type = serializedObject.FindProperty("type");
            _startPosition = serializedObject.FindProperty("startPosition");
            _endPosition = serializedObject.FindProperty("endPosition");
            _direction = serializedObject.FindProperty("direction");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_duration);
            EditorGUILayout.PropertyField(_color);
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(_type);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            EditorGUILayout.PropertyField(_startPosition);
            if (_type.enumValueFlag == 0)
            {
                EditorGUILayout.PropertyField(_endPosition);
            }
            else
            {
                EditorGUILayout.PropertyField(_direction);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}