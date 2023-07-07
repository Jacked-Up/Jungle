using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Application
{
    [Node(
        Title = "Open URL",
        Category = "Application", 
        Color = JungleNodeColors.Violet,
        InputPortName = "Open"
    )]
    public class OpenURLNode : JungleNode
    {
        #region Variables

        [SerializeField] [TextArea(1, 2)]
        private string urlToOpen = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        #endregion
        
        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
        {
            if (!string.IsNullOrEmpty(urlToOpen))
            {
                UnityEngine.Application.OpenURL(urlToOpen);
            }
            call = new[]
            {
                new PortCall(0, true)
            };
            return true;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(OpenURLNode))]
    public class OpenURLNodeEditor : UnityEditor.Editor
    {
        #region Variables

        private SerializedProperty _urlToOpen;

        #endregion

        private void OnEnable()
        {
            _urlToOpen = serializedObject.FindProperty("urlToOpen");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_urlToOpen, new GUIContent("URL To Open"));
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}