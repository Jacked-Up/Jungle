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

        public Action<JungleNodeView> OnNodeSelected;
        private NodeTree _selectedTree;
        private JungleNodeView _selectedNodeView;
        private readonly Vector2 defaultRootNodePosition = new(120, 120);
        
        #endregion

        public JungleGraphView()
        {
            Insert(0, new GridBackground());
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            RegisterCallback<ExecuteCommandEvent>(OnExecuteCommand);
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(JungleEditor.STYLE_SHEET_PATH);
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += () =>
            {
                PopulateView(_selectedTree);
                AssetDatabase.SaveAssets();
            };
        }

        public new class UxmlFactory : UxmlFactory<JungleGraphView, UxmlTraits> {}

        public void SetupSearchWindow(JungleSearchWindow searchWindow)
        {
            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            };
        }
        
        private void OnExecuteCommand(ExecuteCommandEvent arg)
        {
            if (panel.GetCapturingElement(PointerId.mousePointerId) != null || _selectedNodeView == null) return;
            if (arg.commandName == "Duplicate")
            {
                DuplicateNode(_selectedNodeView);
                arg.StopPropagation();
            }
            if (!arg.isPropagationStopped || arg.imguiEvent == null) return;
            arg.imguiEvent.Use();
        }

        private JungleNodeView GetNodeView(Node node)
        {
            return GetNodeByGuid(node.NodeProperties.guid) as JungleNodeView;
        }

        public void PopulateView(NodeTree nodeTree)
        {
            if (nodeTree == null) return;
            _selectedTree = nodeTree;

            graphViewChanged -= GraphViewChangedCallback;
            DeleteElements(graphElements);
            graphViewChanged += GraphViewChangedCallback;

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
                CreateNodeView(node);
                for (var i = 0; i < node.OutputPorts.Length; i++)
                {
                    foreach (var connection in node.OutputPorts[i].Connections)
                    {
                        var outputNodeView = GetNodeView(node);
                        var inputNodeView = GetNodeView(connection);
                        if (outputNodeView.OutputPorts[i] != null)
                        {
                            var edgeView = outputNodeView.OutputPorts[i].ConnectTo(inputNodeView.InputPorts);
                            AddElement(edgeView);
                        }
                    }
                }
            }
        }

        public override List<UnityEditor.Experimental.GraphView.Port> GetCompatiblePorts(UnityEditor.Experimental.GraphView.Port startPort, NodeAdapter _)
        {
            return ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
        }
        
        private GraphViewChange GraphViewChangedCallback(GraphViewChange graphViewChange)
        {
            GraphElement rootNodeView = null;
            graphViewChange.elementsToRemove?.ForEach(element =>
            {
                if (element is JungleNodeView nodeView)
                {
                    if (nodeView.Node.GetType() != typeof(RootNode))
                    {
                        _selectedTree.DeleteNode(nodeView.Node);
                        return;
                    }
                    rootNodeView = element;
                }
                if (element is Edge edge)
                {
                    var parentView = edge.output.node as JungleNodeView;
                    var childView = edge.input.node as JungleNodeView;
                    _selectedTree.RemoveConnection(parentView.Node, childView.Node);
                }
            });
            graphViewChange.edgesToCreate?.ForEach(edge =>
            {
                var parentView = edge.output.node as JungleNodeView;
                var childView = edge.input.node as JungleNodeView;
                var nodeIndex = parentView.OutputPorts.IndexOf(edge.output);
                _selectedTree.CreateConnection(parentView.Node, childView.Node, nodeIndex);
            });
            if (rootNodeView != null)
            {
                graphViewChange.elementsToRemove.Remove(rootNodeView);
            }
            return graphViewChange;
        }

        public void UpdateSelection(JungleNodeView nodeView)
        {
            _selectedNodeView = nodeView;
        }
        
        public void CreateNode(Type nodeType, Vector2 position)
        {
            var node = _selectedTree.CreateNode(nodeType, position);
            CreateNodeView(node);
        }

        public void DuplicateNode(JungleNodeView nodeView)
        {
            if (nodeView.Node is RootNode) return;
            var nodeReference = nodeView.Node;
            var nodeCopy = _selectedTree.DuplicateNode(nodeReference);
            CreateNodeView(nodeCopy);
        }
        
        private void CreateNodeView(Node node)
        {
            var nodeView = new JungleNodeView(node)
            {
                NodeSelected = OnNodeSelected
            };
            AddElement(nodeView);
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {}
    }
}
