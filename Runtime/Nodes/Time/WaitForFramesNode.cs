using System;
using UnityEngine;

namespace Jungle.Nodes.Time
{
    [NodeProperties(
        Title = "Wait For Frames", 
        Tooltip = "Waits for the specified amount of frames.",
        Category = "Time",
        Color = Orange
    )]
    [IdentityNode(
        InputPortName = "Begin",
        OutputPortName = "Elapsed"
    )]
    public class WaitForFramesNode : IdentityNode
    {
        #region Variables

        [SerializeField] 
        private int frames = 100;

        [NonSerialized]
        private int _elapsedFrames;

        #endregion
        
        public override void OnStart()
        {
            _elapsedFrames = 0;
        }

        public override void OnUpdate()
        {
            _elapsedFrames++;
            if (_elapsedFrames < frames)
            {
                return;
            }
            CallAndStop();
        }
        
        private void OnValidate()
        {
            frames = (int)Mathf.Clamp(frames, 1, Mathf.Infinity);
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