using System;
using System.Collections.Generic;
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
        
        private const float MINIMUM_ACTIVE_BAR_TIME = 0.5f; 
        
        public JungleNode NodeObject;

        public Port InputPortView { get; private set; }
        
        public List<Port> OutputPortViews { get; private set; }

        public JungleEditor JungleEditor { get; set; }

        public JungleGraphView JungleGraphView { get; set; }

        private float _lastDrawTime = -1f;

        #endregion

        public JungleNodeView(JungleNode nodeReference) 
            : base(AssetDatabase.GetAssetPath(Resources.Load("JungleNodeView")))
        {
            // NEED TO DETECT THIS ERROR
            if (nodeReference == null)
            {
                return;
            }
            
            // Sets the node object to the reference and returns true if the node reference
            // is of type RootNode
            HandleNodeObject(nodeReference);
            
            // Set color of node in the Jungle Editor
            AddToClassList(nodeReference.NodeColor.ToString().ToLower());
            
            if (nodeReference.GetType() != typeof(StartNode))
            {
                HandleInputPortViews();
            }
            HandleOutputPortViews();
        }
        
        public void UpdateNodeView()
        {
            UpdateActiveBar();
            UpdateErrorIcon();
        }
        
        private void HandleNodeObject(JungleNode reference)
        {
            NodeObject = reference;
            title = reference.TitleName;
            tooltip = reference.Tooltip;
            viewDataKey = reference.NodeProperties.guid;
            var graphPosition = reference.NodeProperties.position;
            style.left = graphPosition.x;
            style.top = graphPosition.y;
        }

        private void HandleInputPortViews()
        {
            var port = NodeObject.InputInfo;
            
            InputPortView = InstantiatePort(Orientation.Horizontal, Direction.Input,
                Port.Capacity.Multi, port.PortType);
            
            var portTypeName = port.PortType != typeof(Error)
                ? port.PortType.Name
                : nameof(Error).ToUpper();
            
            InputPortView.portName = $"<b><size=10><i>({portTypeName})</i></size> {port.PortName}</b>";
            inputContainer.Add(InputPortView);
        }

        private void HandleOutputPortViews()
        {
            OutputPortViews = new List<Port>();
            foreach (var port in NodeObject.OutputInfo)
            {
                var newPortView = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi,
                    port.PortType);

                var portTypeName = port.PortType != typeof(Error)
                    ? port.PortType.Name
                    : nameof(Error).ToUpper();
                
                newPortView.portName = $"<b>{port.PortName} <size=10><i>({portTypeName})</i></size></b>";
                newPortView.AddManipulator(new EdgeConnector<Edge>(JungleGraphView));
                OutputPortViews.Add(newPortView);
                outputContainer.Add(newPortView);
            }
        }

        private void UpdateActiveBar()
        {
            var activeBarElement = mainContainer.Q<VisualElement>("active-bar");
            
            // The node cannot be active while in play mode
            if (!Application.isPlaying)
            {
                activeBarElement.visible = false;
                return;
            }
            
            // This is so that any nodes that finish execution immediately will still visualize
            // when they're active for a set period of time
            if (_lastDrawTime == -1f && NodeObject.IsRunning)
            {
                _lastDrawTime = (float)EditorApplication.timeSinceStartup;
                activeBarElement.visible = true;
            }
            if (EditorApplication.timeSinceStartup - _lastDrawTime < MINIMUM_ACTIVE_BAR_TIME)
            {
                return;
            }
            _lastDrawTime = -1f;
            activeBarElement.visible = false;
        }

        private void UpdateErrorIcon()
        {
            var element = mainContainer.Q<VisualElement>("error-icon");
            if (Application.isPlaying)
            {
                element.visible = false;
                return;
            }
            
            element.visible = true;
        }

        public override void SetPosition(Rect position)
        {
            if (NodeObject == null)
            {
                return;
            }
            
            base.SetPosition(position);
            Undo.RecordObject(NodeObject, $"Set {NodeObject.name} position");
            var nodeProperties = new NodeProperties
            {
                guid = NodeObject.NodeProperties.guid,
                comments = NodeObject.NodeProperties.comments,
                position = new Vector2(position.xMin, position.yMin)
            };
            NodeObject.NodeProperties = nodeProperties;
        }
        
        public override Port InstantiatePort(
            Orientation orientation,
            Direction direction,
            Port.Capacity capacity,
            Type type)
        {
            return JunglePortView.Create<Edge>(orientation, direction, capacity, type);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (JungleEditor != null)
            {
                JungleEditor.OnSelectedNode(this);
            }
        }
        
        public override void OnUnselected()
        {
            base.OnUnselected();
            if (JungleEditor != null)
            {
                JungleEditor.OnDeselectedNode();
            }
        }
    }
}
