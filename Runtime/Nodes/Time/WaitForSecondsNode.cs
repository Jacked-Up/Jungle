using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Time
{
    [NodeProperties(
        Title = "Wait For Seconds",
        Tooltip = "Waits for the specified amount of seconds.",
        Category = "Time",
        Color = Orange
    )]
    [IdentityNode(
        InputPortName = "Begin",
        OutputPortName = "Elapsed"
    )]
    public class WaitForSecondsNode : IdentityNode
    {
        #region Variables

        [SerializeField] 
        private float duration = 1f;

        [SerializeField]
        private bool scaledTime = true;

        [NonSerialized]
        private float _startTime;

        #endregion

        public override void OnStart()
        {
            _startTime = scaledTime
                ? UnityEngine.Time.time
                : UnityEngine.Time.unscaledTime;
        }
        
        public override void OnUpdate()
        {
            var currentTime = scaledTime
                ? UnityEngine.Time.time
                : UnityEngine.Time.unscaledTime;
            if (currentTime - _startTime < duration)
            {
                return;
            }
            CallAndStop();
        }

        private void OnValidate()
        {
            duration = Mathf.Clamp(duration, 0.001f, Mathf.Infinity);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WaitForSecondsNode))]
    public class WaitForSecondsNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private SerializedProperty _duration;
        private SerializedProperty _scaledTime;

        #endregion

        private void OnEnable()
        {
            _duration = serializedObject.FindProperty("duration");
            _scaledTime = serializedObject.FindProperty("scaledTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_duration);
            EditorGUILayout.PropertyField(_scaledTime);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
