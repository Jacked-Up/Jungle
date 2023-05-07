﻿using System;
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

        public readonly Node NodeInstance;

        public readonly List<UnityEditor.Experimental.GraphView.Port> OutputPortViews = new();
        /*
        {
            get
            {
                var convert = new List<UnityEditor.Experimental.GraphView.Port>();
                outputContainer.contentContainer.Children().ToList().ForEach(element =>
                {
                    convert.Add((UnityEditor.Experimental.GraphView.Port)element);
                });
                return convert;
            }
        }
        */

        public readonly UnityEditor.Experimental.GraphView.Port InputPortView;

        // DONT LISTEN TO IT. IT DOES EXIST. PROMISE. LOVE YOU.
        private static string UIFileAssetPath => AssetDatabase.GetAssetPath(Resources.Load("JungleNodeView"));
        
        #endregion

        public JungleNodeView(Node nodeReference) : base(UIFileAssetPath)
        {
            NodeInstance = nodeReference;
            var isRootNodeType = NodeInstance.GetType() == typeof(RootNode);
            
            // Set title and reference GUID properties of node
            title = nodeReference.TitleName;
            viewDataKey = nodeReference.NodeProperties.guid;

            // Set position of node in Jungle Editor graph view
            var graphPosition = nodeReference.NodeProperties.position;
            style.left = graphPosition.x;
            style.top = graphPosition.y;
            
            // Set color of node in the Jungle Editor
            AddToClassList(nodeReference.NodeColor.ToString().ToLower());

            if (!isRootNodeType)
            {
                var nameLabel = mainContainer.Q<Label>("context-label");
                var nodeViewName = nodeReference.NodeProperties.viewName;
                nameLabel.text = nodeViewName.Length < 26 
                    ? nodeViewName 
                    : $"{nodeViewName[..23]}...";
            }
            // Special stylization for root node type
            else
            {
                mainContainer.Q<Label>("context-label").RemoveFromHierarchy();
                outputContainer.transform.position = new Vector3(0, -25, 0);
            }

            // Create input port view
            if (!isRootNodeType)
            {
                var inputPortView = InstantiatePort(Orientation.Horizontal, Direction.Input,
                    UnityEditor.Experimental.GraphView.Port.Capacity.Multi, NodeInstance.InputInfo.PortType);
                if (inputPortView != null)
                {
                    inputPortView.portName = NodeInstance.InputInfo.PortName;
                    inputContainer.Add(inputPortView);
                }
                InputPortView = inputPortView;
            }

            // Create output port views
            //var outputPortViews = new List<UnityEditor.Experimental.GraphView.Port>(NodeInstance.OutputInfo.Length);
            for (var i = 0; i < nodeReference.OutputInfo.Length; i++)
            {
                var portView = InstantiatePort(Orientation.Horizontal, Direction.Output,
                    UnityEditor.Experimental.GraphView.Port.Capacity.Multi, nodeReference.OutputInfo[i].PortType);
                if (!isRootNodeType)
                {
                    portView.portName = nodeReference.OutputInfo[i].PortName;
                }
                // Special stylization for root node type
                else
                {
                    portView.portName = string.Empty;
                }
                OutputPortViews.Add(portView);
                outputContainer.Add(portView);
            }
        }

        public override void SetPosition(Rect position)
        {
            base.SetPosition(position);
            Undo.RecordObject(NodeInstance, $"Set {NodeInstance.name} position");
            var nodeProperties = new NodeProperties
            {
                guid = NodeInstance.NodeProperties.guid,
                viewName = NodeInstance.NodeProperties.viewName,
                position = new Vector2(position.xMin, position.yMin)
            };
            NodeInstance.NodeProperties = nodeProperties;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            NodeSelected?.Invoke(this);
        }
    }
}
