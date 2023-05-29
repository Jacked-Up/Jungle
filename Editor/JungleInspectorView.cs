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
            if (_nodeInspector == null)
            {
                return;
            }
            var container = new IMGUIContainer(() =>
            {
                if (_nodeInspector.target == null)
                {
                    return;
                }
                
                var node = (JungleNode)_nodeInspector.target;
                var properties = node.NodeProperties;

                GUI.enabled = _nodeInspector.target is not RootNode;
                GUILayout.Label("Comments:");
                var notes = GUILayout.TextArea(properties.comments, 300);
                node.NodeProperties = new NodeProperties
                {
                    guid = properties.guid,
                    comments = notes,
                    position = properties.position
                };
                GUI.enabled = true;
                
                GUILayout.Space(1);
                var lineRect = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(lineRect, EditorGUIUtility.isProSkin 
                    ? new Color(0.7f, 0.7f, 0.7f, 0.5f) 
                    : new Color(0.3f, 0.3f, 0.3f, 0.5f));
                GUILayout.Space(5);

                if (Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("Any and all changes made in play mode will not revert!"
                        , MessageType.Warning);
                }
                _nodeInspector.OnInspectorGUI();
            });
            Add(container);
        }
    }
}
