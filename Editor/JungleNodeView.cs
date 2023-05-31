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

        private const float MINIMUM_ACTIVE_DRAW_TIME = 0.5f; 
        
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
                //outputContainer.transform.position = new Vector3(0, -25, 0);
            }

            if (!isRootNode) HandleInputPortViews();
            HandleOutputPortViews(isRootNode);
            
            UpdateDrawActiveBar(false);
        }

        private bool HandleNodeObject(JungleNode reference)
        {
            NodeObject = reference;
            title = reference.TitleName;
            viewDataKey = reference.NodeProperties.guid;
            var graphPosition = reference.NodeProperties.position;
            style.left = graphPosition.x;
            style.top = graphPosition.y;
            
            return reference.GetType() == typeof(RootNode);
        }

        private void HandleInputPortViews()
        {
            var port = NodeObject.InputInfo;
            
            InputPortView = InstantiatePort(Orientation.Horizontal, Direction.Input,
                UnityEditor.Experimental.GraphView.Port.Capacity.Multi, port.PortType);
            InputPortView.portName = $"<size=10><b>({port.PortType.Name})</b></size> {port.PortName}";
            
            inputContainer.Add(InputPortView);
        }

        private void HandleOutputPortViews(bool isRootNode)
        {
            OutputPortViews = new List<UnityEditor.Experimental.GraphView.Port>();
            foreach (var port in NodeObject.OutputInfo)
            {
                var newPortView = InstantiatePort(Orientation.Horizontal, Direction.Output,
                    UnityEditor.Experimental.GraphView.Port.Capacity.Multi, port.PortType);
                newPortView.portName = $"{port.PortName} <size=10><b>({port.PortType.Name})</b></size>";

                OutputPortViews.Add(newPortView);
                outputContainer.Add(newPortView);
            }
        }
        
        public override void SetPosition(Rect position)
        {
            if (NodeObject == null)
            {
                JungleDebug.Log("JungleNodeView", "Failed to set position!", null);
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

        public override void OnSelected()
        {
            base.OnSelected();
            NodeSelectedCallback?.Invoke(this);
        }

        public void UpdateDrawActiveBar(bool active)
        {
            var element = mainContainer.Q<VisualElement>("active-bar");
            if (!Application.isPlaying)
            {
                element.transform.scale = Vector3.zero;
                return;
            }
            
            // This is so that any nodes that finish execution immediately will still visualize
            // when they're active for a set period of time
            if (EditorApplication.timeSinceStartup - _lastDrawTime < MINIMUM_ACTIVE_DRAW_TIME && !active)
            {
                return;
            }
            _lastDrawTime = (float)EditorApplication.timeSinceStartup;
                
            element.transform.scale = active
                ? Vector3.one
                : Vector3.zero;
        }
    }
}
