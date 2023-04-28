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
            _nodeInspector = UnityEditor.Editor.CreateEditor(nodeView.Node);
            var container = new IMGUIContainer(() =>
            {
                if (_nodeInspector.target != null)
                {
                    var node = (BaseNode)_nodeInspector.target;
                    var properties = node.NodeProperties;
                    GUILayout.Label("Comments:");
                    var comments = EditorGUILayout.TextArea(properties.comments);
                    var context = EditorGUILayout.TextField("Name", properties.name);
                    node.NodeProperties = new NodeProperties
                    {
                        guid = properties.guid,
                        name = context,
                        comments = comments,
                        position = properties.position
                    };
                    
                    GUILayout.Space(1);
                    var lineRect = EditorGUILayout.GetControlRect(false, 1);
                    EditorGUI.DrawRect(lineRect, new Color(1f, 1f, 1f, 0.15f));
                    GUILayout.Space(5);
                    
                    _nodeInspector.OnInspectorGUI();
                }
            });
            Add(container);
        }
    }
}
