using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Jungle.Editor
{
    public class JungleNodeView : Node
    {
        #region Variables
        
        public Action<JungleNodeView> NodeSelected;
        public readonly BaseNode Node;
        public Port InputPorts;
        public readonly List<Port> OutputPorts = new();
        
        #endregion

        public JungleNodeView(BaseNode node) : base(AssetDatabase.GetAssetPath(Resources.Load("JungleNodeView")))
        {
            Node = node;
            title = node.ViewName;
            viewDataKey = node.NodeProperties.guid;

            var graphPosition = node.NodeProperties.position;
            style.left = graphPosition.x;
            style.top = graphPosition.y;
            
            switch (node.NodeColor)
            {
                case NodeColor.Red:
                    AddToClassList("red");
                    break;
                case NodeColor.Orange:
                    AddToClassList("orange");
                    break;
                case NodeColor.Yellow:
                    AddToClassList("yellow");
                    break;
                case NodeColor.Green:
                    AddToClassList("green");
                    break;
                case NodeColor.Blue:
                    AddToClassList("blue");
                    break;
                case NodeColor.Purple:
                    AddToClassList("purple");
                    break;
                case NodeColor.Violet:
                    AddToClassList("violet");
                    break;
                case NodeColor.Grey:
                    AddToClassList("grey");
                    break;
            }

            CreateInputPort();
            CreateOutputPorts();
        }

        private void CreateInputPort()
        {
            if (Node is not RootNode)
            {
                InputPorts = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
                if (InputPorts != null)
                {
                    InputPorts.portName = Node.InputPortName;
                    inputContainer.Add(InputPorts);
                }
            }
        }

        private void CreateOutputPorts()
        {
            var outputPortNamesList = Node.OutputPortNames.ToList();
            foreach (var portName in Node.OutputPortNames)
            {
                var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
                port.portName = portName;
                OutputPorts.Add(port);
                outputContainer.Add(port);
            }
        }

        public override void SetPosition(Rect position)
        {
            base.SetPosition(position);
            Undo.RecordObject(Node, $"Set {Node.name} position");
            var nodeProperties = new NodeProperties
            {
                guid = Node.NodeProperties.guid,
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
