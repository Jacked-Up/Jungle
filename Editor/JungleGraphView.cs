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

        public JungleNodeView SelectedNodeView { get; set; }
        public Action<JungleNodeView> OnNodeSelected { get; set; }
        
        private readonly Vector2 defaultRootNodePosition = new(100, 120);
        private JungleTree _selectedTree;

        #endregion

        public JungleGraphView()
        {
            Insert(0, new GridBackground());
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            RegisterCallback<ExecuteCommandEvent>(ExecuteCommandCallback);
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(JungleEditor.STYLE_SHEET_FILE_PATH);
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += () =>
            {
                PopulateGraphView(_selectedTree);
                AssetDatabase.SaveAssets();
            };
        }

        public new class UxmlFactory : UxmlFactory<JungleGraphView, UxmlTraits> {}
        
        public void CreateNodeAndView(Type nodeType, Vector2 position)
        {
            var nodeView = new JungleNodeView(_selectedTree.CreateNode(nodeType, position))
            {
                NodeSelectedCallback = OnNodeSelected
            };
            AddElement(nodeView);
        }

        public void PopulateGraphView(JungleTree tree)
        {
            if (tree == null)
            {
                return;
            }
            _selectedTree = tree;

            graphViewChanged -= GraphViewChangedCallback;
            DeleteElements(graphElements);
            graphViewChanged += GraphViewChangedCallback;

            // Add root node if one does not already exist
            if (_selectedTree.rootNode == null)
            {
                var root = _selectedTree.CreateNode(typeof(RootNode), defaultRootNodePosition) as RootNode;
                _selectedTree.rootNode = root;
                EditorUtility.SetDirty(_selectedTree);
                AssetDatabase.SaveAssets();
            }
            
            // Creates node view and edges
            foreach (var node in _selectedTree.nodes)
            {
                if (node == null)
                {
                    continue;
                }
                var nodeView = new JungleNodeView(node)
                {
                    NodeSelectedCallback = OnNodeSelected
                };
                AddElement(nodeView);
            }
            foreach (var node in _selectedTree.nodes)
            {
                var nodeView = GetNodeView(node);
                // This is a case the only occurs when the nodes
                // script has been deleted
                if (nodeView == null) continue;
                
                for (var i = 0; i < nodeView.OutputPortViews.Count; i++)
                {
                    // If this condition is met, it means the node has no connections
                    // Therefore it can be safely skipped
                    if (nodeView.OutputPortViews.Count != nodeView.NodeObject.OutputPorts.Length)
                    {
                        continue;
                    }
                    var outputPortView = nodeView.OutputPortViews[i];
                    var outputPort = nodeView.NodeObject.OutputPorts[i];
                    foreach (var connection in outputPort.connections)
                    {
                        var connectionView = GetNodeView(connection);
                        if (connectionView == null) continue;
                        var inputPortView = connectionView.InputPortView;
                        AddElement(outputPortView.ConnectTo(inputPortView));
                    }
                }
            }
        }
        
        public void SetupSearchWindow(JungleSearchWindow searchWindow)
        {
            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            };
        }

        public void UpdateDrawActiveBar()
        {
            foreach (var node in _selectedTree.nodes)
            {
                if (node == null)
                {
                    continue;
                }
                GetNodeView(node).UpdateDrawActiveBar(_selectedTree.ExecutingNodes.Contains(node));
            }
        }

        private JungleNodeView GetNodeView(JungleNode node)
        {
            if (node == null)
            {
                return null;
            }
            var nodeView = GetNodeByGuid(node.NodeProperties.guid);
            return nodeView as JungleNodeView;
        }
        
        private void ExecuteCommandCallback(ExecuteCommandEvent arg)
        {
            if (panel.GetCapturingElement(PointerId.mousePointerId) != null || SelectedNodeView == null)
            {
                return;
            }
            if (arg.commandName == "Duplicate")
            {
                // Duplicating the root node is forbidden
                if (SelectedNodeView.NodeObject is RootNode)
                {
                    return;
                }
                var nodeView = new JungleNodeView(_selectedTree.DuplicateNode(SelectedNodeView.NodeObject))
                {
                    NodeSelectedCallback = OnNodeSelected
                };
                AddElement(nodeView);
                arg.StopPropagation();
            }
            if (!arg.isPropagationStopped || arg.imguiEvent == null) return;
            arg.imguiEvent.Use();
        }
        
        private GraphViewChange GraphViewChangedCallback(GraphViewChange graphViewChange)
        {
            // Remove all requested nodes
            if (graphViewChange.elementsToRemove != null)
            {
                GraphElement rootNodeView = null;
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is JungleNodeView nodeView)
                    {
                        if (nodeView.NodeObject.GetType() != typeof(RootNode))
                        {
                            _selectedTree.DeleteNode(nodeView.NodeObject);
                        }
                        else rootNodeView = element;
                    }
                    else if (element is Edge edge)
                    {
                        if (edge.output.node is JungleNodeView parentView && edge.input.node is JungleNodeView childView)
                        {
                            var index = (byte)parentView.OutputPortViews.IndexOf(edge.output);
                            _selectedTree.DisconnectNodes(parentView.NodeObject, childView.NodeObject, index);
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
                        _selectedTree.ConnectNodes(parentView.NodeObject, childView.NodeObject, nodeIndex);
                    }
                }
            }
            
            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {}
        
        public override List<UnityEditor.Experimental.GraphView.Port> GetCompatiblePorts(UnityEditor.Experimental.GraphView.Port startPort, NodeAdapter _)
        {
            // If the port type is null, this means that the node has some kind of issue internally.
            // It is safest to just not allow any connections until the problem is fixed
            if (startPort.portType == typeof(Error))
            {
                return null;
            }
            
            // Otherwise the compatible port must not be the same connection direction and the same
            // connection type
            var compatiblePorts = ports.ToList().Where(endPort => endPort.direction != startPort.direction 
                                                                  && endPort.node != startPort.node 
                                                                  && endPort.portType == startPort.portType);
            return compatiblePorts.ToList();
        }
    }
}
