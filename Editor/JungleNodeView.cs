﻿using System;
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
        private string LIGHT_MODE_TEXT_HEX_CODE = "#222222";
        private string DARK_MODE_TEXT_HEX_CODE = "#D4D4D4";
        
        public JungleNode Node;

        public Port InputPortView { get; private set; }
        
        public List<Port> OutputPortViews { get; private set; }

        public event SelectionCallback OnNodeSelected;
        public event SelectionCallback OnNodeUnselected;
        public delegate void SelectionCallback(JungleNodeView context);

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

            if (nodeReference.GetType() != typeof(StartNode))
            {
                HandleInputPortViews();
            }
            HandleOutputPortViews();
            
            // Set color of node in the Jungle Editor
            AddToClassList(nodeReference.NodeColor.ToString().ToLower());
            AddToClassList(EditorGUIUtility.isProSkin
                ? "dark"
                : "light"
            );
        }
        
        public void UpdateNodeView()
        {
            UpdateActiveBar();
            UpdateErrorIcon();
        }
        
        private void HandleNodeObject(JungleNode reference)
        {
            Node = reference;
            title = reference.TitleName;
            tooltip = reference.Tooltip;
            viewDataKey = reference.NodeProperties.guid;
            var graphPosition = reference.NodeProperties.position;
            style.left = graphPosition.x;
            style.top = graphPosition.y;
        }

        private void HandleInputPortViews()
        {
            var port = Node.InputInfo;
            
            InputPortView = InstantiatePort(Orientation.Horizontal, Direction.Input,
                Port.Capacity.Multi, port.PortType);
            
            var portTypeName = port.PortType != typeof(Error)
                ? port.PortType.Name
                : nameof(Error).ToUpper();

            InputPortView.portName = 
                $"<color={(EditorGUIUtility.isProSkin ? DARK_MODE_TEXT_HEX_CODE : LIGHT_MODE_TEXT_HEX_CODE)}>";
            InputPortView.portName += $"<b><size=10><i>({portTypeName})</i></size> {port.PortName}</b>";
            InputPortView.portName += "</color>";
            inputContainer.Add(InputPortView);
        }

        private void HandleOutputPortViews()
        {
            OutputPortViews = new List<Port>();
            foreach (var port in Node.OutputInfo)
            {
                var newPortView = InstantiatePort(Orientation.Horizontal, Direction.Output, 
                    Port.Capacity.Multi, port.PortType);

                var portTypeName = port.PortType != typeof(Error)
                    ? port.PortType.Name
                    : nameof(Error).ToUpper();
                
                newPortView.portName = 
                    $"<color={(EditorGUIUtility.isProSkin ? DARK_MODE_TEXT_HEX_CODE : LIGHT_MODE_TEXT_HEX_CODE)}>";
                newPortView.portName += $"<b>{port.PortName} <size=10><i>({portTypeName})</i></size></b>";
                newPortView.portName += "</color>";
                
                OutputPortViews.Add(newPortView);
                outputContainer.Add(newPortView);
            }
        }

        private void UpdateActiveBar()
        {
            var activeBarElement = mainContainer.Q<VisualElement>("glow");
            if (activeBarElement == null)
            {
                return;
                //throw new NullReferenceException("Jungle Node View is missing active bar element");
            }
            
            // The node cannot be active while in play mode
            if (!Application.isPlaying)
            {
                activeBarElement.visible = false;
                return;
            }
            
            // This is so that any nodes that finish execution immediately will still visualize
            // when they're active for a set period of time
            if (_lastDrawTime == -1f && Node.IsRunning)
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
            if (element == null)
            {
                return;
            }
            
            if (Application.isPlaying)
            {
                element.visible = false;
                return;
            }
            element.visible = true;
        }

        public override void SetPosition(Rect position)
        {
            if (Node == null)
            {
                return;
            }
            
            base.SetPosition(position);
            Undo.RecordObject(Node, $"Set {Node.name} position");
            var nodeProperties = new NodeProperties
            {
                guid = Node.NodeProperties.guid,
                comments = Node.NodeProperties.comments,
                position = new Vector2(position.xMin, position.yMin)
            };
            Node.NodeProperties = nodeProperties;
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
            OnNodeSelected?.Invoke(this);
        }
        
        public override void OnUnselected()
        {
            base.OnUnselected();
            OnNodeUnselected?.Invoke(this);
        }
    }
}
