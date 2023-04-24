using System;
using System.Collections.Generic;
using System.Linq;
using Jungle.Nodes;
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
            viewDataKey = node.nodeProperties.guid;

            var graphPosition = node.nodeProperties.position;
            style.left = graphPosition.x;
            style.top = graphPosition.y;

            if (Node is RootNode)
            {
                AddToClassList("root");
            }
            else if (Node is StopTreeNode or StopNodeNode)
            {
                AddToClassList("stop");
            }
            else if (Node is WhileLoopNode or ForLoopNode)
            {
                AddToClassList("special");
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
                    InputPorts.portName = "Perform";
                    inputContainer.Add(InputPorts);
                }
            }
        }

        private void CreateOutputPorts()
        {
            var portNames = Node.PortNames.ToList();
            foreach (var portName in portNames)
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
                guid = Node.nodeProperties.guid,
                position = new Vector2(position.xMin, position.yMin)
            };
            Node.nodeProperties = nodeProperties;
            EditorUtility.SetDirty(Node);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            NodeSelected?.Invoke(this);
        }
    }
}
