using System.Linq;
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
                var data = node.tree.GetData(node);

                GUI.enabled = _nodeInspector.target is not RootNode;
                GUILayout.Label("Comments:");
                var comments = GUILayout.TextArea(data.comments, 300);
                node.tree.nodes[node.tree.nodes.ToList().IndexOf(node.tree.GetData(node))] = new JungleTree.NodeData
                {
                    node = data.node,
                    name = data.name,
                    guid = data.guid,
                    comments = comments,
                    graphPosition = data.graphPosition
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
