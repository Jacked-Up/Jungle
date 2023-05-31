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
    public class JungleNodeView : Node
    {
        #region Variables

        public JungleNode NodeObject;

        public UnityEditor.Experimental.GraphView.Port InputPortView { get; private set; }

        public List<UnityEditor.Experimental.GraphView.Port> OutputPortViews { get; private set; }

        public Action<JungleNodeView> NodeSelectedCallback;

        private static string UIFileAssetPath 
            => AssetDatabase.GetAssetPath(Resources.Load("JungleNodeView"));
        
        private float _lastDrawTime;
        
        #endregion

        public JungleNodeView(JungleNode nodeReference) : base(UIFileAssetPath)
        {
            // NEED TO DETECT THIS ERROR
            if (nodeReference == null)
            {
                return;
            }
            
            // Sets the node object to the reference and returns true if the node reference
            // is of type RootNode
            var isRootNode = HandleNodeObject(nodeReference);

            // Set color of node in the Jungle Editor
            AddToClassList(nodeReference.NodeColor.ToString().ToLower());
            
            if (isRootNode)
            {
                outputContainer.transform.position = new Vector3(0, -25, 0);
            }
            if (!isRootNode) HandleInputPortViews();
            HandleOutputPortViews(isRootNode);
        }

        private bool HandleNodeObject(JungleNode reference)
        {
            NodeObject = reference;
            title = reference.TitleName;
            viewDataKey = reference.tree.GetData(reference).guid;
            var graphPosition = reference.tree.GetData(reference).graphPosition;
            style.left = graphPosition.x;
            style.top = graphPosition.y;
            
            return reference.GetType() == typeof(RootNode);
        }

        private void HandleInputPortViews()
        {
            var port = NodeObject.InputInfo;
            
            InputPortView = InstantiatePort(Orientation.Horizontal, Direction.Input,
                UnityEditor.Experimental.GraphView.Port.Capacity.Multi, port.PortType);
            InputPortView.portName = $"{port.PortName} <size=10><b>({port.PortType.Name})</b></size>";
            
            inputContainer.Add(InputPortView);
        }

        private void HandleOutputPortViews(bool isRootNode)
        {
            OutputPortViews = new List<UnityEditor.Experimental.GraphView.Port>();
            foreach (var port in NodeObject.OutputInfo)
            {
                var newPortView = InstantiatePort(Orientation.Horizontal, Direction.Output,
                    UnityEditor.Experimental.GraphView.Port.Capacity.Multi, port.PortType);
                newPortView.portName = $"<size=10><b>({port.PortType.Name})</b></size> ";
                newPortView.portName += !isRootNode 
                    ? port.PortName 
                    : string.Empty;

                OutputPortViews.Add(newPortView);
                outputContainer.Add(newPortView);
            }
        }
        
        public override void SetPosition(Rect position)
        {
            if (NodeObject == null)
            {
                JungleDebug.Log("JungleNodeView", "Failed to set position!");
                return;
            }
            
            base.SetPosition(position);
            Undo.RecordObject(NodeObject.tree, $"Set {NodeObject.name} position");
            var node = NodeObject;
            var data = node.tree.GetData(node);
            node.tree.nodes[node.tree.nodes.ToList().IndexOf(node.tree.GetData(node))] = new JungleTree.NodeData
            {
                node = data.node,
                name = data.name,
                guid = data.guid,
                comments = data.comments,
                graphPosition = new Vector2(position.xMin, position.yMin)
            };
        }

        public override void OnSelected()
        {
            base.OnSelected();
            NodeSelectedCallback?.Invoke(this);
        }

        public void UpdateDrawActiveBar(bool state)
        {
            var element = mainContainer.Q<VisualElement>("active-bar");
            if (element == null)
            {
                return;
            }
            if (!Application.isPlaying)
            {
                element.transform.scale = Vector3.zero;
                return;
            }
            
            if (EditorApplication.timeSinceStartup - _lastDrawTime < 0.1f && !state)
            {
                return;
            }
            _lastDrawTime = (float)EditorApplication.timeSinceStartup;
                
            element.transform.scale = state
                ? Vector3.one
                : Vector3.zero;
        }
    }
}
