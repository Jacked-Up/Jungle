using System;
using System.Collections.Generic;
using System.IO;
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
        
        #endregion

        public JungleNodeView(JungleNode nodeReference) : base(UIFileAssetPath)
        {
            // NEED TO DETECT THIS ERROR
            if (nodeReference == null) return;
            
            // Sets the node object to the reference and returns true if the node reference
            // is of type RootNode
            var isRootNode = HandleNodeObject(nodeReference);

            // Set color of node in the Jungle Editor
            AddToClassList(nodeReference.NodeColor.ToString().ToLower());

            // A wonderful nest of grossness :)
            var notesLabel = mainContainer.Q<Label>("notes-label");
            if (!isRootNode)
            {
                var nodeNotes = nodeReference.NodeProperties.comments;
                if (!string.IsNullOrEmpty(nodeNotes))
                {
                    using var reader = new StringReader(nodeNotes);
                    var firstLine = reader.ReadLine();
                    if (!string.IsNullOrEmpty(firstLine))
                    {
                        notesLabel.text = firstLine.Length < 26 
                            ? firstLine 
                            : $"{firstLine[..23]}...";
                    }
                }
                else notesLabel.RemoveFromHierarchy();
            }
            // Special stylization for root node type
            else
            {
                outputContainer.transform.position = new Vector3(0, -25, 0);
                notesLabel.RemoveFromHierarchy();
            }

            if (!isRootNode) HandleInputPortViews();
            HandleOutputPortViews(isRootNode);
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
    }
}
