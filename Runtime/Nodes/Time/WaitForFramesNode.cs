using System;
using UnityEngine;

namespace Jungle.Nodes.Time
{
    [Node(TitleName = "Wait For Frames", 
        Category = "Time",
        Color = Color.Purple, 
        OutputPortNames = new []{"Elapsed"}
    )]
    public class WaitForFramesNode : JungleNode
    {
        #region Variables

        [SerializeField] 
        private int frames = 100;

        [NonSerialized]
        private int _frameIndex;

        #endregion

        public override void Initialize(in object inputValue)
        {
            _frameIndex = 0;
        }

        public override bool Execute(out PortCall[] call)
        {
            _frameIndex++;
            if (_frameIndex < frames)
            {
                call = Array.Empty<PortCall>();
                return false;
            }
            call = new[] {new PortCall(0, true)};
            return true;
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