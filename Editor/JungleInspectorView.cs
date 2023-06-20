using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    /// <summary>
    /// Jungle Inspector View.
    /// </summary>
    public class JungleInspectorView : VisualElement
    {
        #region Variables

        private JungleEditor _jungleEditor;
        private UnityEditor.Editor nodeInspector;
        
        public new class UxmlFactory : UxmlFactory<JungleInspectorView, UxmlTraits> {}
        
        #endregion
        
        /// <summary>
        /// Initialize with Jungle editor.
        /// </summary>
        /// <param name="editor">Jungle editor reference.</param>
        public void Initialize(JungleEditor editor)
        {
            _jungleEditor = editor;
            RepaintGUIContainer();
        }
        
        /// <summary>
        /// Repaint inspector view with selected nodes editor.
        /// </summary>
        /// <param name="nodeView">Node view reference. Can be null.</param>
        public void UpdateSelection(JungleNodeView nodeView)
        {
            // Create nodes inspector IF possible
            if (nodeView != null && nodeView.NodeObject != null)
            {
                nodeInspector = UnityEditor.Editor.CreateEditor(nodeView.NodeObject);
            }
            else nodeInspector = null;
            RepaintGUIContainer();
        }
        
        private void RepaintGUIContainer()
        {
            Clear();
            Add(new IMGUIContainer(() =>
            {
                DrawInspectorHeader();
                DrawInspectorBody();
                DrawInspectorFooter();
            }));
        }
        
        private void DrawInspectorHeader()
        {
            var selectedNode = nodeInspector != null
                ? nodeInspector.target as JungleNode
                : null;
            var properties = selectedNode != null
                ? selectedNode.NodeProperties
                : new NodeProperties();
            
            GUILayout.Label("Comments:");
            if (selectedNode != null)
            {
                selectedNode.NodeProperties = new NodeProperties
                {
                    guid = properties.guid,
                    comments = GUILayout.TextArea(properties.comments, 300),
                    position = properties.position
                };
            }
            else
            {
                GUI.enabled = false;
                GUILayout.TextArea(string.Empty, 300);
                GUI.enabled = true;
            }
            
            GUILayout.Space(1);
            var lineRect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(lineRect, EditorGUIUtility.isProSkin 
                ? new Color(0.7f, 0.7f, 0.7f, 0.5f) 
                : new Color(0.3f, 0.3f, 0.3f, 0.5f));
            GUILayout.Space(5);
        }
        
        private void DrawInspectorBody()
        {
            if (nodeInspector != null && nodeInspector.target != null)
            {
                nodeInspector.OnInspectorGUI();
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Label("Select a node to edit...");               
                GUI.enabled = true;
            }
        }
        
        private void DrawInspectorFooter()
        {
            var label = _jungleEditor.rootVisualElement.Q<Label>("status-label");
            //var icon = _jungleEditor.rootVisualElement.Q("status-icon");

            if (Application.isPlaying)
            {
                //icon.Q<Image>("background-image").image = Resources.Load("Icons/JungleIssueIcon") as Texture;
                label.text = "Any changes made during play mode will persist.";
            }
            else
            {
                //icon.Q<Image>("background-image").image = Resources.Load("Icons/JungleIssueIcon") as Texture;
                label.text = "Validated with no issues.";
            }
        }
    }
}
