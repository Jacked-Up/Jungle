using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes.Time
{
    [Node(ViewName = "Wait For Frames", Category = "Time", Color = NodeColor.Yellow, OutputPortNames = new []{"Elapsed"})]
    public class WaitForFramesNode : BaseNode
    {
        #region Variables

        [SerializeField] 
        private int frames = 100;

        [NonSerialized]
        private int _frameIndex;

        #endregion

        private void OnValidate()
        {
            if (frames < 0) frames = 0;
        }

        public override void Initialize()
        {
            _frameIndex = 0;
        }

        public override Verdict Execute()
        {
            _frameIndex++;
            if (_frameIndex < frames)
            {
                return new Verdict(false);
            }
            return new Verdict(true, new List<int>{0});
        }
    }
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(WaitForFramesNode))]
    public class WaitForFramesNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private UnityEditor.SerializedProperty _frames;

        #endregion

        private void OnEnable()
        {
            _frames = serializedObject.FindProperty("frames");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            UnityEditor.EditorGUILayout.PropertyField(_frames);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}