using System;
using System.Collections.Generic;
using Jungle.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JungleNodeView : UnityEditor.Experimental.GraphView.Node
    {
        #region Variables
        
        public Action<JungleNodeView> NodeSelected;
        public readonly Node Node;
        public UnityEditor.Experimental.GraphView.Port InputPorts;
        public readonly List<UnityEditor.Experimental.GraphView.Port> OutputPorts = new List<UnityEditor.Experimental.GraphView.Port>();
        
        #endregion

        public JungleNodeView(Node node) : base(AssetDatabase.GetAssetPath(Resources.Load("JungleNodeView")))
        {
            Node = node;
            title = node.TitleName;
            viewDataKey = node.NodeProperties.guid;

            var graphPosition = node.NodeProperties.position;
            style.left = graphPosition.x;
            style.top = graphPosition.y;
            AddToClassList(node.NodeColor.ToString().ToLower());

            if (node.GetType() != typeof(RootNode))
            {
                var nameLabel = mainContainer.Q<Label>("context-label");
                var nodeViewName = node.NodeProperties.viewName;
                nameLabel.text = nodeViewName.Length < 26 
                    ? nodeViewName 
                    : $"{nodeViewName.Substring(0, 23)}...";
            }
            else
            {
                mainContainer.Q<Label>("context-label").RemoveFromHierarchy();
                outputContainer.transform.position = new Vector3(0, -25, 0);
            }

            CreateInputPort();
            CreateOutputPorts();
        }

        private void CreateInputPort()
        {
            if (Node.GetType() != typeof(RootNode))
            {
                InputPorts = InstantiatePort(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Multi, typeof(bool));
                if (InputPorts != null)
                {
                    InputPorts.portName = Node.InputInfo.PortName;
                    inputContainer.Add(InputPorts);
                }
            }
        }

        private void CreateOutputPorts()
        {
            foreach (var portInfo in Node.OutputInfo)
            {
                var portView = InstantiatePort(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Multi, typeof(bool));
                portView.portName = Node.GetType() != typeof(RootNode) 
                    ? portInfo.PortName
                    : string.Empty;
                OutputPorts.Add(portView);
                outputContainer.Add(portView);
            }
        }

        public override void SetPosition(Rect position)
        {
            base.SetPosition(position);
            Undo.RecordObject(Node, $"Set {Node.name} position");
            var nodeProperties = new NodeProperties
            {
                guid = Node.NodeProperties.guid,
                viewName = Node.NodeProperties.viewName,
                position = new Vector2(position.xMin, position.yMin)
            };
            Node.NodeProperties = nodeProperties;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            NodeSelected?.Invoke(this);
        }
    }
}
