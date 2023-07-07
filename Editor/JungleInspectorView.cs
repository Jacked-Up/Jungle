using Jungle.Nodes.Scene;
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

        public JungleNode InspectingNode =>
            nodeInspector != null
                ? nodeInspector.target as JungleNode
                : null;

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
            var editName = selectedNode != null
                ? selectedNode.name
                : string.Empty;
            GUI.enabled = selectedNode != null;
            editName = GUILayout.TextField(editName, 100);
            if (selectedNode != null)
            {
                selectedNode.name = !string.IsNullOrEmpty(editName) 
                    ? editName 
                    : "Untitled Node";
            }
            GUI.enabled = true;
            
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
