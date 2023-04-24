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

        private const string JUNGLE_EDITOR_STYLE_PATH = "Packages/com.jackedupstudios.jungle/Editor/UI/JungleEditorStyle.uss";
        
        public Action<JungleNodeView> OnNodeSelected;
        
        public EditorWindow EditorWindow;
        private JungleEditor _jungleEditor;
        private JungleNodeView _selectedNodeView;
        private JungleSearchWindow _searchWindow;
        
        private NodeTree _tree;

        #endregion

        public JungleGraphView()
        {
            Insert(0, new GridBackground());
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            RegisterCallback<ExecuteCommandEvent>(OnExecuteCommand);
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(JUNGLE_EDITOR_STYLE_PATH);
            styleSheets.Add(styleSheet);
            
            _searchWindow = ScriptableObject.CreateInstance<JungleSearchWindow>();
            _searchWindow.Initialize(this);
            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
            };

            Undo.undoRedoPerformed += () =>
            {
                PopulateView(_tree);
                AssetDatabase.SaveAssets();
            };
        }

        public void SetJungleEditor(JungleEditor editor)
        {
            _jungleEditor = editor;
        }
        
        private void OnExecuteCommand(ExecuteCommandEvent evt)
        {
            if (panel.GetCapturingElement(PointerId.mousePointerId) != null || _selectedNodeView == null)
                return;
            if (evt.commandName == "Duplicate")
            {
                DuplicateNode(_selectedNodeView);
                evt.StopPropagation();
            }
            if (!evt.isPropagationStopped || evt.imguiEvent == null)
                return;
            evt.imguiEvent.Use();
        }

        public new class UxmlFactory : UxmlFactory<JungleGraphView, UxmlTraits> {}

        private JungleNodeView FindNodeView(BaseNode baseNode)
        {
            return GetNodeByGuid(baseNode.nodeProperties.guid) as JungleNodeView;
        }

        public void PopulateView(NodeTree nodeTree)
        {
            _tree = nodeTree;

            graphViewChanged -= GraphViewChangedCallback;
            DeleteElements(graphElements);
            graphViewChanged += GraphViewChangedCallback;

            if (_tree.rootNode == null)
            {
                _tree.rootNode = _tree.CreateNode(typeof(RootNode), new Vector2(50f, 50f)) as RootNode;
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }

            // Creates node view
            _tree.nodes.ForEach(CreateNodeView);

            // Creates edges
            _tree.nodes.ForEach(node =>
            {
                if (node.ports == null || node.ports.Count != node.PortNames.Count)
                {
                    node.ports = new List<NodePort>();
                    node.PortNames.ForEach(_ =>
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
                        _tree.DeleteNode(nodeView.Node);
                        return;
                    }
                    rootNodeView = element;
                }
                if (element is Edge edge)
                {
                    var parentView = edge.output.node as JungleNodeView;
                    var childView = edge.input.node as JungleNodeView;
                    _tree.RemoveChild(parentView.Node, childView.Node);
                }
            });
            graphViewChange.edgesToCreate?.ForEach(edge =>
            {
                var parentView = edge.output.node as JungleNodeView;
                var childView = edge.input.node as JungleNodeView;
                var nodeIndex = parentView.OutputPorts.IndexOf(edge.output);
                _tree.AddChild(parentView.Node, childView.Node, nodeIndex);
            });
            if (rootNodeView != null)
            {
                graphViewChange.elementsToRemove.Remove(rootNodeView);
            }
            return graphViewChange;
        }

        public void SelectedNodeViewCallback(JungleNodeView nodeView, JungleEditor treeEditor)
        {
            _selectedNodeView = nodeView;
            _jungleEditor = treeEditor;
        }
        
        public void CreateNode(Type nodeType, Vector2 position)
        {
            var node = _tree.CreateNode(nodeType, position);
            CreateNodeView(node);
        }

        public void DuplicateNode(JungleNodeView reference)
        {
            if (reference.Node is RootNode) return;
            
            var nodeOriginal = reference.Node;
            var nodePosition = reference.Node.nodeProperties.graphPosition + new Vector2(25f, 25f);
            var node = _tree.DuplicateNode(nodeOriginal, nodePosition);
            CreateNodeView(node);
        }
        
        private void CreateNodeView(BaseNode baseNode)
        {
            var nodeView = new JungleNodeView(baseNode)
            {
                NodeSelected = OnNodeSelected
            };
            AddElement(nodeView);
            if (_jungleEditor != null)
            {
                _jungleEditor.JungleInspectorView.UpdateSelection(nodeView);
            }
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {}
    }
}
