using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Application
{
    [NodeProperties(
        Title = "Open URL",
        Category = "Application", 
        Color = Violet
    )]
    [IdentityNode(
        InputPortName = "Open",
        OutputPortName = "Next"
    )]
    public class OpenURLNode : IdentityNode
    {
        #region Variables

        [SerializeField] [TextArea(1, 2)]
        private string urlToOpen = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        #endregion

        public override void OnStart(in object inputValue)
        {
            if (!string.IsNullOrEmpty(urlToOpen))
            {
                UnityEngine.Application.OpenURL(urlToOpen);
            }
            CallAndStop();
        }
        
        public override void OnUpdate()
        {
            
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