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
        private NodeTree _selectedTree;

        #endregion

        public JungleGraphView()
        {
            Insert(0, new GridBackground());
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            RegisterCallback<ExecuteCommandEvent>(ExecuteCommandCallback);
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(JungleEditor.STYLE_SHEET_PATH);
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
                NodeSelected = OnNodeSelected
            };
            AddElement(nodeView);
        }
        
        public void PopulateGraphView(NodeTree nodeTree)
        {
            if (nodeTree == null) return;
            _selectedTree = nodeTree;

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
                var nodeView = new JungleNodeView(node)
                {
                    NodeSelected = OnNodeSelected
                };
                AddElement(nodeView);
                
                var outputNodePortViews = GetNodeView(node)?.OutputPortViews;
                if (outputNodePortViews == null) continue;
                foreach (var outputPort in node.OutputPorts)
                {
                    foreach (var connection in outputPort.Connections)
                    {
                        var inputNodePortView = GetNodeView(connection)?.InputPortView;
                        if (inputNodePortView == null) continue;
                        AddElement(outputNodePortViews[outputPort.Connections.ToList().IndexOf(connection)].ConnectTo(inputNodePortView));
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

        private JungleNodeView GetNodeView(Node node)
        {
            return GetNodeByGuid(node.NodeProperties.guid) as JungleNodeView;
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
                if (SelectedNodeView.NodeInstance is RootNode)
                {
                    return;
                }
                var nodeView = new JungleNodeView(_selectedTree.DuplicateNode(SelectedNodeView.NodeInstance))
                {
                    NodeSelected = OnNodeSelected
                };
                AddElement(nodeView);
                arg.StopPropagation();
            }
            if (!arg.isPropagationStopped || arg.imguiEvent == null) return;
            arg.imguiEvent.Use();
        }
        
        private GraphViewChange GraphViewChangedCallback(GraphViewChange graphViewChange)
        {
            GraphElement rootNodeView = null;
            graphViewChange.elementsToRemove ??= new List<GraphElement>();
            foreach (var element in graphViewChange.elementsToRemove)
            {
                if (element is JungleNodeView nodeView)
                {
                    if (nodeView.NodeInstance.GetType() != typeof(RootNode))
                    {
                        _selectedTree.DeleteNode(nodeView.NodeInstance);
                    }
                    else rootNodeView = element;
                }
                else if (element is Edge edge)
                {
                    if (edge.output.node is JungleNodeView parentView && edge.input.node is JungleNodeView childView)
                    {
                        _selectedTree.RemoveConnection(parentView.NodeInstance, childView.NodeInstance);
                    }
                }
            }
            // Deleting the root node is forbidden
            if (rootNodeView != null)
            {
                graphViewChange.elementsToRemove.Remove(rootNodeView);
            }

            graphViewChange.edgesToCreate ??= new List<Edge>();
            foreach (var edge in graphViewChange.edgesToCreate)
            {
                if (edge.output.node is JungleNodeView parentView && edge.input.node is JungleNodeView childView)
                {
                    var nodeIndex = parentView.OutputPortViews.IndexOf(edge.output);
                    _selectedTree.CreateConnection(parentView.NodeInstance, childView.NodeInstance, nodeIndex);
                }
            }
            
            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {}
        
        public override List<UnityEditor.Experimental.GraphView.Port> GetCompatiblePorts(UnityEditor.Experimental.GraphView.Port startPort, NodeAdapter _)
        {
            return ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
        }
    }
}
