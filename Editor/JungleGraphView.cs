using System;
using System.Collections.Generic;
using System.Linq;
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
        
        
        
        private NodeTree _selectedNodeTree;
        private JungleNodeView _selectedNodeView;

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
                PopulateView(_selectedNodeTree);
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
        
        private void OnExecuteCommand(ExecuteCommandEvent evt)
        {
            if (panel.GetCapturingElement(PointerId.mousePointerId) != null || _selectedNodeView == null) return;
            if (evt.commandName == "Duplicate")
            {
                DuplicateNode(_selectedNodeView);
                evt.StopPropagation();
            }
            if (!evt.isPropagationStopped || evt.imguiEvent == null) return;
            evt.imguiEvent.Use();
        }

        private JungleNodeView FindNodeView(BaseNode baseNode)
        {
            return GetNodeByGuid(baseNode.NodeProperties.guid) as JungleNodeView;
        }

        public void PopulateView(NodeTree nodeTree)
        {
            if (nodeTree == null) return;
            _selectedNodeTree = nodeTree;

            graphViewChanged -= GraphViewChangedCallback;
            DeleteElements(graphElements);
            graphViewChanged += GraphViewChangedCallback;

            if (_selectedNodeTree.rootNode == null)
            {
                _selectedNodeTree.rootNode = _selectedNodeTree.CreateNode(typeof(RootNode), new Vector2(50f, 100f)) as RootNode;
                EditorUtility.SetDirty(_selectedNodeTree);
                AssetDatabase.SaveAssets();
            }

            // Creates node view
            _selectedNodeTree.nodes.ForEach(CreateNodeView);

            // Creates edges
            _selectedNodeTree.nodes.ForEach(node =>
            {
                if (node.ports == null || node.ports.Count != node.OutputPortNames.Length)
                {
                    node.ports = new List<NodePort>();
                    node.OutputPortNames.ToList().ForEach(_ =>
                    {
                        node.ports.Add(new NodePort(new List<BaseNode>()));
                    });
                    EditorUtility.SetDirty(node);
                }
                
                var parentView = FindNodeView(node);
                var portIndex = 0;
                node.ports.ForEach(port =>
                {
                    var outputPort = parentView.OutputPorts[portIndex];
                    portIndex++;

                    if (outputPort != null)
                    {
                        port.children.ForEach(child =>
                        {
                            var edge = outputPort.ConnectTo(FindNodeView(child).InputPorts);
                            AddElement(edge);
                        });
                    }
                });
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter _)
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
                    if (nodeView.Node is not RootNode)
                    {
                        _selectedNodeTree.DeleteNode(nodeView.Node);
                        return;
                    }
                    rootNodeView = element;
                }
                if (element is Edge edge)
                {
                    var parentView = edge.output.node as JungleNodeView;
                    var childView = edge.input.node as JungleNodeView;
                    _selectedNodeTree.RemoveChild(parentView.Node, childView.Node);
                }
            });
            graphViewChange.edgesToCreate?.ForEach(edge =>
            {
                var parentView = edge.output.node as JungleNodeView;
                var childView = edge.input.node as JungleNodeView;
                var nodeIndex = parentView.OutputPorts.IndexOf(edge.output);
                _selectedNodeTree.AddChild(parentView.Node, childView.Node, nodeIndex);
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
            var node = _selectedNodeTree.CreateNode(nodeType, position);
            CreateNodeView(node);
        }

        public void DuplicateNode(JungleNodeView nodeView)
        {
            if (nodeView.Node is RootNode) return;
            var nodeReference = nodeView.Node;
            var nodeCopy = _selectedNodeTree.DuplicateNode(nodeReference);
            CreateNodeView(nodeCopy);
        }
        
        private void CreateNodeView(BaseNode baseNode)
        {
            var nodeView = new JungleNodeView(baseNode)
            {
                NodeSelected = OnNodeSelected
            };
            AddElement(nodeView);
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {}
    }
}
