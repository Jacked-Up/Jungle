using System;
using System.Collections.Generic;
using System.Linq;
using Jungle.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JungleGraphView : GraphView
    {
        #region Variables

        private readonly Vector2 DEFAULT_START_NODE_POSITION = new(100, 120);
        
        /// <summary>
        /// 
        /// </summary>
        public List<JungleNodeView> SelectedNodeViews
        {
            get;
            private set;
        } = new();

        private JungleInspectorView _inspectorView;
        
        private JungleEditor _jungleEditor;
        
        public new class UxmlFactory : UxmlFactory<JungleGraphView, UxmlTraits> {}
        
        #endregion
        
        public JungleGraphView()
        {
            Insert(0, new GridBackground());
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            //RegisterCallback<ExecuteCommandEvent>(ExecuteCommandCallback);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(JungleEditor.STYLE_SHEET_FILE_PATH);
            styleSheets.Add(styleSheet);
            AddToClassList(EditorGUIUtility.isProSkin
                ? "dark"
                : "light"
            );
            
            Undo.undoRedoPerformed += () =>
            {
                UpdateGraphView();
                AssetDatabase.SaveAssets();
            };
        }
        
        public void Initialize(JungleEditor editor, JungleInspectorView inspectorView, JungleSearchView searchView)
        {
            _jungleEditor = editor;
            _inspectorView = inspectorView;
            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchView);
            };
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void UpdateGraphView()
        {
            var jungleTree = _jungleEditor.EditTree;
            if (jungleTree == null)
            {
                return;
            }
            
            graphViewChanged -= GraphViewChangedCallback;
            DeleteElements(graphElements);
            graphViewChanged += GraphViewChangedCallback;
            
            // Add root node if one does not already exist
            if (jungleTree.nodes == null || jungleTree.nodes.Length == 0)
            {
                var startNode = jungleTree.CreateNode(typeof(StartNode), DEFAULT_START_NODE_POSITION) as StartNode;
                jungleTree.nodes = new JungleNode[]
                {
                    startNode
                };
                EditorUtility.SetDirty(jungleTree);
                AssetDatabase.SaveAssets();
            }
            
            // Creates node view and edges
            foreach (var node in jungleTree.nodes)
            {
                if (node == null)
                {
                    continue;
                }
                var nodeView = new JungleNodeView(node);
                AddElement(nodeView);
            }
            foreach (var node in jungleTree.nodes)
            {
                var nodeView = GetNodeView(node);
                // This is a case the only occurs when the nodes
                // script has been deleted
                if (nodeView == null) continue;

                nodeView.OnNodeSelected += NodeSelectedCallback;
                nodeView.OnNodeUnselected += NodeUnselectedCallback;
                
                for (var i = 0; i < nodeView.OutputPortViews.Count; i++)
                {
                    // If this condition is met, it means the node has no connections
                    // Therefore it can be safely skipped
                    if (nodeView.OutputPortViews.Count != nodeView.Node.OutputPorts.Length)
                    {
                        continue;
                    }
                    var outputPortView = nodeView.OutputPortViews[i];
                    var outputPort = nodeView.Node.OutputPorts[i];
                    foreach (var connection in outputPort.connections)
                    {
                        var connectionView = GetNodeView(connection);
                        if (connectionView == null) continue;
                        var inputPortView = connectionView.InputPortView;
                        AddElement(outputPortView.ConnectTo(inputPortView));
                    }
                }
            }

            // The view transform is the graph view "camera"
            viewTransform.position = jungleTree.editorData.lastViewPosition;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void UpdateNodeViews()
        {
            var jungleTree = _jungleEditor.EditTree;
            if (jungleTree == null)
            {
                return;
            }
            
            foreach (var node in jungleTree.nodes ??= Array.Empty<JungleNode>())
            {
                if (node == null) continue;
                GetNodeView(node)?.UpdateNodeView();
            }

            // The view transform is the graph view "camera"
            jungleTree.editorData.lastViewPosition = viewTransform.position;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="position"></param>
        public JungleNodeView CreateNode(Type nodeType, Vector2 position)
        {
            var jungleTree = _jungleEditor.EditTree;
            if (jungleTree == null)
            {
                return null;
            }

            var nodeView = new JungleNodeView(jungleTree.CreateNode(nodeType, position));
            AddElement(nodeView);
            return nodeView;
        }

        public JungleNodeView GetNodeView(JungleNode node)
        {
            if (node == null)
            {
                return null;
            }
            var nodeView = GetNodeByGuid(node.NodeProperties.guid);
            return nodeView as JungleNodeView;
        }

        private void NodeSelectedCallback(JungleNodeView selected)
        {
            SelectedNodeViews ??= new List<JungleNodeView>();
            if (SelectedNodeViews.Contains(selected))
            {
                return;
            }
            SelectedNodeViews.Add(selected);
            _inspectorView.UpdateSelection(selected);
        }
        
        private void NodeUnselectedCallback(JungleNodeView unselected)
        {
            SelectedNodeViews ??= new List<JungleNodeView>();
            if (!SelectedNodeViews.Contains(unselected))
            {
                return;
            }
            SelectedNodeViews.Remove(unselected);
            
            // If no nodes views are selected, clear the node inspector
            if (SelectedNodeViews.Count == 0)
            {
                _inspectorView.UpdateSelection(null);
            }
            // If the currently inspected node is the node that has been deselected,
            // select the last node in the selection list
            else if (_inspectorView.InspectingNode == unselected.Node)
            {
                _inspectorView.UpdateSelection(SelectedNodeViews[^1]);
            }
        }
        
        #region Editor Inheritance

        private GraphViewChange GraphViewChangedCallback(GraphViewChange graphViewChange)
        {
            var jungleTree = _jungleEditor.EditTree;
            if (jungleTree == null)
            {
                return graphViewChange;
            }

            // Remove all requested nodes
            if (graphViewChange.elementsToRemove != null)
            {
                GraphElement rootNodeView = null;
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is JungleNodeView nodeView)
                    {
                        if (nodeView.Node.GetType() != typeof(StartNode))
                        {
                            jungleTree.DeleteNode(nodeView.Node);
                        }
                        else rootNodeView = element;
                    }
                    else if (element is Edge edge)
                    {
                        if (edge.output.node is JungleNodeView parentView && edge.input.node is JungleNodeView childView)
                        {
                            var index = (byte)parentView.OutputPortViews.IndexOf(edge.output);
                            jungleTree.DisconnectNodes(parentView.Node, childView.Node, index);
                        }
                    }
                }
                // Deleting the root node is forbidden
                if (rootNodeView != null)
                {
                    graphViewChange.elementsToRemove.Remove(rootNodeView);
                }
            }
            
            // Create requested edges
            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    if (edge.output.node is JungleNodeView parentView && edge.input.node is JungleNodeView childView)
                    {
                        var nodeIndex = (byte)parentView.OutputPortViews.ToList().IndexOf(edge.output);
                        jungleTree.ConnectNodes(parentView.Node, childView.Node, nodeIndex);
                    }
                }
            }
            
            return graphViewChange;
        }

        /*
        private void ExecuteCommandCallback(ExecuteCommandEvent arg)
        {
            var jungleTree = _jungleEditor.EditTree;
            if (jungleTree == null)
            {
                return;
            }

            if (panel.GetCapturingElement(PointerId.mousePointerId) != null || SelectedNodeView == null)
            {
                return;
            }
            if (arg.commandName == "Duplicate")
            {
                // Duplicating the root node is forbidden
                if (SelectedNodeView.Node is StartNode)
                {
                    return;
                }
                var nodeView = new JungleNodeView(jungleTree.DuplicateNode(SelectedNodeView.Node));
                _jungleEditor.OnSelectedNode(nodeView);
                AddElement(nodeView);
                arg.StopPropagation();
            }
            if (!arg.isPropagationStopped || arg.imguiEvent == null) return;
            arg.imguiEvent.Use();
        }
        */
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {}
        
        public override List<Port> GetCompatiblePorts(Port selected, NodeAdapter _)
        {
            // If the port type is null, this means that the node has some kind of issue internally.
            // It is safest to just not allow any connections until the problem is fixed
            if (selected.portType == typeof(Error))
            {
                return new List<Port>();
            }
            // Otherwise the compatible port must not be the same connection direction and the same
            // connection type
            var compatible = ports.ToList().Where(other => other.direction != selected.direction 
                                                                  && other.node != selected.node 
                                                                  && other.portType == selected.portType);
            return compatible.ToList();
        }

        #endregion
    }
}
