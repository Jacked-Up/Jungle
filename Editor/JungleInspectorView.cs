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

        public JungleNode InspectingNode
        {
            get
            {
                if (nodeInspector == null)
                {
                    return null;
                }
                return nodeInspector.target as JungleNode;
            }
        }

        private UnityEditor.Editor nodeInspector;

        public new class UxmlFactory : UxmlFactory<JungleInspectorView, UxmlTraits> {}
        
        #endregion
        
        /// <summary>
        /// Initialize with Jungle editor.
        /// </summary>
        public void Initialize()
        {
            RepaintGUIContainer();
        }

        /// <summary>
        /// Repaint inspector view with selected nodes editor.
        /// </summary>
        /// <param name="nodeView">Node view reference. Can be null.</param>
        public void UpdateSelection(JungleNodeView nodeView)
        {
            // Create nodes inspector IF possible
            if (nodeView != null && nodeView.Node != null)
            {
                nodeInspector = UnityEditor.Editor.CreateEditor(nodeView.Node);
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
            }));
        }
        
        private void DrawInspectorHeader()
        {
            var selectedNode = nodeInspector != null
                ? nodeInspector.target as JungleNode
                : null;
            
            if (selectedNode != null)
            {
                selectedNode.name = GUILayout.TextField(selectedNode.name, 100);
            }
            else
            {
                GUI.enabled = false;
                GUILayout.TextField(string.Empty);
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
    }
}
