using System;
using UnityEngine;

namespace Jungle.Nodes.Time
{
    [Node(TitleName = "Wait For Seconds",
        Category = "Time",
        Color = NodeAttribute.NodeColor.Yellow,
        OutputPortNames = new []{"Elapsed"})]
    public class WaitForSecondsNode : Node
    {
        #region Variables

        [SerializeField] 
        private float duration = 1f;

        [SerializeField]
        private bool scaledTime = true;

        [NonSerialized]
        private float _startTime;

        #endregion

        private void OnValidate()
        {
            duration = Mathf.Clamp(duration, 0f, Mathf.Infinity);
        }

        public override void Initialize(in object inputValue)
        {
            _startTime = scaledTime
                ? UnityEngine.Time.time
                : UnityEngine.Time.unscaledTime;
        }

        public override bool Execute(out PortCall[] call)
        {
            var currentTime = scaledTime
                ? UnityEngine.Time.time
                : UnityEngine.Time.unscaledTime;

            if (currentTime - _startTime >= duration)
            {
                call = new[] {new PortCall(0, new Nothing())};
                return true;
            }
            call = Array.Empty<PortCall>();
            return false;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(WaitForSecondsNode))]
    public class WaitForSecondsNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private UnityEditor.SerializedProperty _duration;
        private UnityEditor.SerializedProperty _scaledTime;

        #endregion

        private void OnEnable()
        {
            _duration = serializedObject.FindProperty("duration");
            _scaledTime = serializedObject.FindProperty("scaledTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            UnityEditor.EditorGUILayout.PropertyField(_duration);
            UnityEditor.EditorGUILayout.PropertyField(_scaledTime);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
