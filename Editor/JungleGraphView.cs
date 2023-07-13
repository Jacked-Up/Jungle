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
        private const float MINIMUM_ZOOM = 0.5f;
        private const float MAXIMUM_ZOOM = 1.5f;

        /// <summary>
        /// 
        /// </summary>
        public static JungleGraphView Singleton => _singleton;
        private static JungleGraphView _singleton;
        
        /// <summary>
        /// 
        /// </summary>
        public List<JungleNodeView> SelectedNodeViews
        {
            get;
            private set;
        } = new();

        public new class UxmlFactory : UxmlFactory<JungleGraphView, UxmlTraits> {}
        
        #endregion

        public JungleGraphView()
        {
            _singleton = this;
            
            Insert(0, new GridBackground());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            SetupZoom
            (
                MINIMUM_ZOOM,
                MAXIMUM_ZOOM,
                ContentZoomer.DefaultScaleStep,
                ContentZoomer.DefaultReferenceScale
            );

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(JungleEditor.STYLE_SHEET_FILE_PATH));
            AddToClassList(EditorGUIUtility.isProSkin ? "dark" : "light");
            
            Undo.undoRedoPerformed += () =>
            {
                UpdateGraphView();
                AssetDatabase.SaveAssets();
            };
            
            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), JungleEditor.Singleton.SearchView);
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateGraphView()
        {
            var jungleTree = JungleEditor.Singleton.EditTree;
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
            if (jungleTree.editorData.lastViewScale == Vector3.zero)
            {
                jungleTree.editorData.lastViewScale = Vector3.one;
            }
            viewTransform.scale = jungleTree.editorData.lastViewScale;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void UpdateNodeViews()
        {
            var jungleTree = JungleEditor.Singleton.EditTree;
            if (jungleTree == null)
            {
                return;
            }
            
            foreach (var node in jungleTree.nodes ??= Array.Empty<JungleNode>())
            {
                if (node == null)
                {
                    continue;
                }
                GetNodeView(node)?.UpdateNodeView();
            }

            // The view transform is the graph view "camera"
            if (jungleTree.editorData.lastViewPosition != viewTransform.position
                || jungleTree.editorData.lastViewScale != viewTransform.scale
                )
            {
                EditorUtility.SetDirty(jungleTree);
            }
            jungleTree.editorData.lastViewPosition = viewTransform.position;
            jungleTree.editorData.lastViewScale = viewTransform.scale;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="position"></param>
        public JungleNodeView CreateNode(Type nodeType, Vector2 position)
        {
            var jungleTree = JungleEditor.Singleton.EditTree;
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
            var nodeView = GetNodeByGuid(node.NodeEditorProperties.guid);
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
            JungleInspectorView.Singleton.UpdateSelection(selected);
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
                JungleInspectorView.Singleton.UpdateSelection(null);
            }
            // If the currently inspected node is the node that has been deselected,
            // select the last node in the selection list
            else if (JungleInspectorView.Singleton.InspectingNode == unselected.Node)
            {
                JungleInspectorView.Singleton.UpdateSelection(SelectedNodeViews[^1]);
            }
        }
        
        #region Editor Inheritance

        private GraphViewChange GraphViewChangedCallback(GraphViewChange graphViewChange)
        {
            var jungleTree = JungleEditor.Singleton.EditTree;
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
        
        public override List<Port> GetCompatiblePorts(Port selected, NodeAdapter _)
        {
            // If the port type is null, this means that the node has some kind of issue internally.
            // It is safest to just not allow any connections until the problem is fixed
            if (selected.portType == typeof(Unknown))
            {
                return new List<Port>();
            }
            // Otherwise the compatible port must not be the same connection direction and the same
            // connection type
            var compatible = ports.ToList().Where(
                other => 
                    other.direction != selected.direction 
                    && other.node != selected.node 
                    && other.portType == selected.portType);
            return compatible.ToList();
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent context)
        {
            if (context.shiftKey)
                Debug.Log("SHIFT");
            
            if (context.target is JungleGraphView)
            {
                context.menu.AppendAction("Add node", _ =>
                {
                    SearchWindow.Open(new SearchWindowContext(Vector2.zero), JungleEditor.Singleton.SearchView);
                });
                context.menu.AppendAction("Add sticky note", _ =>
                {
                    AddElement(new StickyNote());
                });
                context.menu.AppendSeparator();
                context.menu.AppendAction("Select all", _ =>
                {
                    nodes.ToList().ForEach(AddToSelection);
                });
                context.menu.AppendSeparator();
                context.menu.AppendAction("Preferences", _ =>
                {
                    JunglePreferences.OpenWindow();
                });
                context.menu.AppendAction("Recenter view", _ =>
                {
                    viewTransform.position = Vector3.zero;
                    viewTransform.scale = Vector3.one;
                });
                context.menu.AppendSeparator();
            }
            else if (context.target is JungleNodeView)
            {
                context.menu.AppendAction("Cut", ContextRequestCutNodeCallback);
                context.menu.AppendAction("Copy", ContextRequestCopyNodeCallback);
                context.menu.AppendAction("Paste", ContextRequestPasteNodeCallback, DropdownMenuAction.Status.Disabled);
                context.menu.AppendSeparator();
                context.menu.AppendAction("Duplicate", ContextRequestDuplicateNodeCallback);
                context.menu.AppendSeparator();
                context.menu.AppendAction("Delete", ContextRequestDeleteNodeCallback);
            }
            else if (context.target is Edge)
            {
                context.menu.AppendAction("Disconnect", ContextRequestDisconnectNodeCallback);
            }
        }

        private void ContextRequestCutNodeCallback(DropdownMenuAction obj)
        {
            
        }

        private void ContextRequestCopyNodeCallback(DropdownMenuAction obj)
        {
            
        }
        
        private void ContextRequestPasteNodeCallback(DropdownMenuAction obj)
        {
            
        }

        private void ContextRequestDuplicateNodeCallback(DropdownMenuAction obj)
        {
            
        }
        
        private void ContextRequestDeleteNodeCallback(DropdownMenuAction obj)
        {
            
        }
        
        private void ContextRequestDisconnectNodeCallback(DropdownMenuAction obj)
        {
            
        }

        #endregion
    }
}
