﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jungle
{
    /// <summary>
    /// The Jungle sequencer base node class
    /// </summary>
    [Serializable] [Node]
    public abstract class Node : ScriptableObject, INode
    {
        #region Variables

        /// <summary>
        /// Reference to the tree which this node is a child of
        /// </summary>
        [SerializeField] [HideInInspector] 
        public Tree tree;
        
        /// <summary>
        /// Array of the nodes output ports
        /// </summary>
        public Port[] OutputPorts => outputPorts;
        [SerializeField] [HideInInspector] 
        private Port[] outputPorts = Array.Empty<Port>();
        
        /// <summary>
        /// List of available node colors
        /// </summary>
        public enum Color
        {
            Red,
            Orange,
            Yellow,
            Green,
            Blue,
            Purple,
            Violet,
            Grey
        }
        
        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        public abstract void Initialize(in object inputValue);
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <returns></returns>
        public abstract bool Execute(out PortCall[] call);
        
#if UNITY_EDITOR
        /// <summary>
        /// Jungle editor properties of the node.
        /// *Strongly recommended to not touch this
        /// </summary>
        public NodeProperties NodeProperties
        {
            get => nodeProperties;
            set
            {
                nodeProperties = value;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        [SerializeField] [HideInInspector] 
        private NodeProperties nodeProperties;

        private NodeAttribute NodeInfo
            => (NodeAttribute) GetType().GetCustomAttributes(typeof(NodeAttribute), true)[0];

        /// <summary>
        /// Main name of the node displayed in big letters inside the Jungle editor graph view
        /// </summary>
        public string TitleName => NodeInfo.TitleName;

        /// <summary>
        /// Category placement of the node inside the create node window 
        /// </summary>
        public string Category => NodeInfo.Category;

        /// <summary>
        /// Displayed color of the node inside the Jungle editor graph view
        /// </summary>
        public Color NodeColor => NodeInfo.Color;

        /// <summary>
        /// Name and data type info about the input port
        /// </summary>
        public NodeAttribute.PortInfo InputInfo => NodeInfo.InputInfo;

        /// <summary>
        /// Name and data type info about the output ports
        /// </summary>
        public NodeAttribute.PortInfo[] OutputInfo => NodeInfo.OutputInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portIndex"></param>
        /// <param name="node"></param>
        public void MakeConnection(Node node, byte portIndex)
        {
            // Fix any mismatches with the port attributes
            // Output ports count error
            if (OutputPorts.Length != OutputInfo.Length)
            {
                if (OutputPorts.Length != 0)
                {
                    Debug.LogError($"[{name}] Output port count was changed was changed. All connections have " +
                                   "been lost to prevent type mismatch errors");
                }
                var repairedPortsList = OutputInfo.Select(info =>
                {
                    return new Port(Array.Empty<Node>(), info.PortType);
                }).ToArray();
                outputPorts = repairedPortsList;
            }
            // Output ports type mismatch error
            var newPortsList = new List<Port>();
            for (var i = 0; i < OutputInfo.Length; i++)
            {
                var portType = OutputInfo[i].PortType;
                if (OutputPorts[i].PortType != portType)
                {
                    Debug.LogError($"[{name}] Port by name \"{OutputInfo[i].PortName}\" was of type" +
                                   $" {OutputPorts[i].PortType} but is now of type {portType}." +
                                   " All port connections have been lost to prevent type mismatch errors");
                }
                var connections = i == portIndex 
                    ? new List<Node>(OutputPorts[i].connections) {node}.ToArray()
                    : OutputPorts[i].connections;
                newPortsList.Add(new Port(connections, portType));
            }
            outputPorts = newPortsList.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="node"></param>
        public void RemoveConnection(Node node)
        {
            Debug.Log("NEED TO IMPLEMENT REMOVING CONNECTIONS");
        }
#endif
    }

    /// <summary>
    /// Base port class. Used for both input and output ports
    /// </summary>
    [Serializable]
    public struct Port
    {
        /// <summary>
        /// List of the nodes that are connected to this port
        /// </summary>
        [SerializeField] [HideInInspector]
        public Node[] connections;

        /// <summary>
        /// The value type that can be called at this port
        /// *Default type is "Nothing". Use type nothing to signify the port sends/receives no data
        /// </summary>
        public Type PortType
        {
            get
            {
                if (string.IsNullOrEmpty(portType))
                {
                    portType = typeof(Error).AssemblyQualifiedName;
                    return typeof(Error);
                }
                return Type.GetType(portType);
            }
        }
        [SerializeField] [HideInInspector] 
        private string portType;

        public Port(Node[] connections, Type portType)
        {
            connections ??= Array.Empty<Node>();
            this.connections = connections;
            this.portType = portType.AssemblyQualifiedName;
        }
    }

    /// <summary>
    /// Container that stores the port ID and the value to send to the port
    /// </summary>
    public struct PortCall
    {
        /// <summary>
        /// ID of the port to send the value to 
        /// </summary>
        public byte PortID { get; private set; }

        /// <summary>
        /// Value to send out of the requested port
        /// </summary>
        public object Value { get; private set; }

        public PortCall(byte portID, object value)
        {
            PortID = portID;
            Value = value;
        }
    }

    /// <summary>
    /// Jungle node declaration attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        /// <summary>
        /// The name the node goes by
        /// </summary>
        public string TitleName { get; set; } = "Untitled Node";

        /// <summary>
        /// ...
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// The color of the node in the visual editor
        /// </summary>
        public Node.Color Color { get; set; } = Node.Color.Blue;

        /// <summary>
        /// 
        /// </summary>
        public string InputPortName { get; set; } = "Execute";

        /// <summary>
        /// 
        /// </summary>
        public Type InputPortType { get; set; } = typeof(bool);

        /// <summary>
        /// 
        /// </summary>
        public string[] OutputPortNames { get; set; } = {"Next"};

        /// <summary>
        /// 
        /// </summary>
        public Type[] OutputPortTypes { get; set; } = {typeof(bool)};

        /// <summary>
        /// The nodes input port
        /// </summary>
        public PortInfo InputInfo => new(InputPortName, InputPortType);

        /// <summary>
        /// List of the nodes output ports
        /// </summary>
        public PortInfo[] OutputInfo
        {
            get
            {
                // In the case the developer has set a number of output port names that does not equal the number of 
                // output port types, and vice versa, pick the largest array and create default name/type to
                // prevent errors
                var queryCount = OutputPortNames.Length == OutputPortTypes.Length
                    ? OutputPortNames.Length
                    : OutputPortNames.Length > OutputPortTypes.Length 
                        ? OutputPortNames.Length 
                        : OutputPortTypes.Length;
                var query = new List<PortInfo>();
                for (var i = 0; i < queryCount; i++)
                {
                    var portName = i < OutputPortNames.Length
                        ? OutputPortNames[i]
                        : "ERROR";
                    var portType = i < OutputPortTypes.Length
                        ? OutputPortTypes[i]
                        : typeof(Error);
                    query.Add(new PortInfo(portName, portType));
                }
                return query.ToArray();
            }
        }

        /// <summary>
        /// Contains info about the ports name and value type
        /// </summary>
        public struct PortInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public string PortName { get; private set; }
            
            /// <summary>
            /// 
            /// </summary>
            public Type PortType { get; private set; }

            public PortInfo(string portName, Type portType)
            {
                PortName = portName;
                PortType = portType;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct Error { }
    
#if UNITY_EDITOR
    /// <summary>
    /// Details about the node in the Jungle editor like GUID, position, and view name
    /// </summary>
    [Serializable]
    public struct NodeProperties
    {
        /// <summary>
        /// The unique GUID of the node
        /// </summary>
        public string guid;

        /// <summary>
        /// The contextual name of the node in the graph view
        /// </summary>
        public string notes;

        /// <summary>
        /// Position of the node inside the graph view
        /// </summary>
        public Vector2 position;
    }
#endif
}