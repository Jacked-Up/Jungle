using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes.Time
{
    [Node(ViewName = "Wait For Seconds", Category = "Time", Color = NodeColor.Yellow, OutputPortNames = new []{"Elapsed"})]
    public class WaitForSecondsNode : BaseNode
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

        public override void Initialize()
        {
            _startTime = scaledTime
                ? UnityEngine.Time.time
                : UnityEngine.Time.unscaledTime;
        }

        public override Verdict Execute()
        {
            var currentTime = scaledTime
                ? UnityEngine.Time.time
                : UnityEngine.Time.unscaledTime;

            if (currentTime - _startTime >= duration)
            {
                return new Verdict(true, new List<int> {0});
            }
            return new Verdict(false);
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
