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

        private JungleNodeView FindNodeView(Node node)
        {
            return GetNodeByGuid(node.NodeProperties.guid) as JungleNodeView;
        }

        public void PopulateView(NodeTree nodeTree)
        {
            if (nodeTree == null) return;
            _selectedNodeTree = nodeTree;

            //graphViewChanged -= GraphViewChangedCallback;
            //DeleteElements(graphElements);
            graphViewChanged += GraphViewChangedCallback;

            if (_selectedNodeTree.rootNode == null)
            {
                _selectedNodeTree.rootNode = _selectedNodeTree.CreateNode(typeof(RootNode), new Vector2(120f, 120f)) as RootNode;
                EditorUtility.SetDirty(_selectedNodeTree);
                AssetDatabase.SaveAssets();
            }
            
            // Creates node view and edges
            _selectedNodeTree.nodes.ToList().ForEach(node =>
            {
                CreateNodeView(node);
                node.EnsureInitialized();
                
                var parentView = FindNodeView(node);
                var portIndex = 0;
                node.OutputPorts.ToList().ForEach(port =>
                {
                    var outputPort = parentView.OutputPorts[portIndex];
                    portIndex++;

                    if (outputPort != null)
                    {
                        port.Connections.ToList().ForEach(child =>
                        {
                            var edge = outputPort.ConnectTo(FindNodeView(child).InputPorts);
                            AddElement(edge);
                        });
                    }
                });
            });
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
                        _selectedNodeTree.DeleteNode(nodeView.Node);
                        return;
                    }
                    rootNodeView = element;
                }
                if (element is Edge edge)
                {
                    var parentView = edge.output.node as JungleNodeView;
                    var childView = edge.input.node as JungleNodeView;
                    _selectedNodeTree.RemoveConnection(parentView.Node, childView.Node);
                }
            });
            graphViewChange.edgesToCreate?.ForEach(edge =>
            {
                var parentView = edge.output.node as JungleNodeView;
                var childView = edge.input.node as JungleNodeView;
                var nodeIndex = parentView.OutputPorts.IndexOf(edge.output);
                _selectedNodeTree.CreateConnection(parentView.Node, childView.Node, nodeIndex);
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
