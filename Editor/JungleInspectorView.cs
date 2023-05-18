using Jungle.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JungleInspectorView : VisualElement
    {
        #region Variables

        private UnityEditor.Editor _nodeInspector;
        
        #endregion

        public new class UxmlFactory : UxmlFactory<JungleInspectorView, UxmlTraits> {}

        public void UpdateSelection(JungleNodeView nodeView)
        {
            Clear();
            Object.DestroyImmediate(_nodeInspector);
            _nodeInspector = UnityEditor.Editor.CreateEditor(nodeView.NodeObject);
            var container = new IMGUIContainer(() =>
            {
                if (_nodeInspector.target == null) return;
                
                var node = (Node)_nodeInspector.target;
                var properties = node.NodeProperties;
                GUILayout.Label("Notes:");
                var notes = GUILayout.TextArea(properties.notes, 300);
                node.NodeProperties = new NodeProperties
                {
                    guid = properties.guid,
                    notes = notes,
                    position = properties.position
                };
                    
                GUILayout.Space(1);
                var lineRect = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(lineRect, EditorGUIUtility.isProSkin 
                    ? new Color(0.7f, 0.7f, 0.7f, 0.5f) 
                    : new Color(0.3f, 0.3f, 0.3f, 0.5f));
                GUILayout.Space(5);

                if (Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("All changes made in play-mode will not revert!", MessageType.Warning);
                }
                _nodeInspector.OnInspectorGUI();
            });
            Add(container);
        }
    }
}
