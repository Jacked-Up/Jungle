using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Jungle
{
    /// <summary>
    /// The Jungle sequencer base node class
    /// </summary>
    [Node]
    public abstract class Node : ScriptableObject, INode
    {
        #region Variables
        
        /// <summary>
        /// Reference to the tree which this node is a child of
        /// </summary>
        public NodeTree Tree
        {
            get => tree;
            set => tree = value;
        }
        [SerializeField] [HideInInspector] 
        private NodeTree tree;

        /// <summary>
        /// 
        /// </summary>
        public Port InputPort
        {
            get
            {
#if UNITY_EDITOR
                if (inputPort.PortType != InputInfo.PortType)
                {
                    Debug.LogError($"[{name}] Input port connections have been lost because the port type changed" +
                                   $"\nWas type {inputPort.PortType} and is now type {InputInfo.PortType}");
                    inputPort.PortType = InputInfo.PortType;
                    inputPort.Connections = Array.Empty<Node>();
                    UnityEditor.EditorUtility.SetDirty(this);
                }
#endif
                return inputPort;
            }
        }
        [SerializeField] [HideInInspector]
        private Port inputPort;

        /// <summary>
        /// Array of the nodes output ports
        /// </summary>
        public Port[] OutputPorts
        {
            get
            {
#if UNITY_EDITOR
                if (outputPorts.Length != OutputInfo.Length)
                {
                    outputPorts = new Port[OutputInfo.Length];
                    //UnityEditor.EditorUtility.SetDirty(this); <- Redundant
                }
                for (var i = 0; i < outputPorts.Length; i++)
                {
                    if (outputPorts[i].PortType != OutputInfo[i].PortType)
                    {
                        Debug.LogError($"[{name}] Output port {i.ToString()} connections have been lost because the port type changed" +
                                       $"\nWas type {outputPorts[i].PortType} and is now type {OutputInfo[i].PortType}");
                        outputPorts[i].PortType = OutputInfo[i].PortType;
                        outputPorts[i].Connections = Array.Empty<Node>();
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
#endif
                return outputPorts;
            }
        }
        [SerializeField] [HideInInspector]
        private Port[] outputPorts;

        /// <summary>
        /// True if the node has ever been run
        /// </summary>
        public bool Started { get; set; } = false;

        #endregion

        /// <summary>
        /// Invoked when the parent node finishes
        /// *Invoked before the update method
        /// </summary>
        /// <param name="inputValue">The input value returned by the parent node</param>
        public abstract void Initialize(in object inputValue);

        /// <summary>
        /// Invoked every frame just like the Unity update event method
        /// </summary>
        /// <param name="call">Ports to invoke during this update call</param>
        /// <returns>Returns true if the node has declared it is finished executing. Returns false if not</returns>
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
            => (NodeAttribute)GetType().GetCustomAttributes(typeof(NodeAttribute), true)[0];
        
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
        public NodeAttribute.NodeColor NodeColor => NodeInfo.Color;

        /// <summary>
        /// Name and data type info about the input port
        /// </summary>
        public NodeAttribute.PortInfo InputInfo => NodeInfo.InputInfo;
        
        /// <summary>
        /// Name and data type info about the output ports
        /// </summary>
        public NodeAttribute.PortInfo[] OutputInfo => NodeInfo.OutputInfo;
        
        /// <summary>
        /// Adds a child node to the specified output port index (EDITOR ONLY)
        /// </summary>
        /// <param name="node">Node to add as a connection</param>
        /// <param name="index">Output port index</param>
        public bool AddOutputConnection(Node node, int index)
        {
            if (OutputPorts.Length != OutputInfo.Length)
            {
                if (OutputPorts.Length != 0)
                {
                    Debug.Log($"[{name}] Outputs have been re-cached. This could mean a loss of connections");
                }
                outputPorts = new Port[OutputInfo.Length];
            }
            if (OutputPorts[index].Connections.Contains(node))
            {
                Debug.LogWarning($"[{name}] {node.name} attempted to make connection to output but is already connected");
                return false;
            }
            outputPorts[index].Connections = new List<Node>(outputPorts[index].Connections) {node}.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
            return true;
        }

        /// <summary>
        /// Removes the node as a child of this node
        /// </summary>
        /// <param name="node">Node to remove as a connection</param>
        public bool RemoveOutputConnection(Node node)
        {
            if (OutputPorts.Length == 0) return false;
            for (var i = 0; i < OutputPorts.Length; i++)
            {
                if (!OutputPorts[i].Connections.Contains(node))
                {
                    continue;
                }
                var query = new List<Node>(outputPorts[i].Connections);
                query.Remove(node);
                outputPorts[i].Connections = query.ToArray();
                UnityEditor.EditorUtility.SetDirty(this);
                return true;
            }
            Debug.LogWarning($"[{name}] Failed to remove node connection with name {node.name}");
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public bool AddInputConnection(Node node)
        {
            if (InputPort.Connections.Contains(node))
            {
                Debug.LogWarning($"[{name}] {node.name} attempted to make connection to input but is already connected");
                return false;
            }
            inputPort.Connections = new List<Node>(inputPort.Connections) {node}.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public bool RemoveInputConnection(Node node)
        {
            if (!InputPort.Connections.Contains(node))
            {
                return false;
            }
            var query = new List<Node>(inputPort.Connections);
            query.Remove(node);
            inputPort.Connections = query.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
            return true;
        }
#endif
    }

    /// <summary>
    /// Base port class. Used for both input and output ports
    /// </summary>
    [Serializable]
    public class Port
    {
        /// <summary>
        /// List of nodes connected to this port that are children/parents of the node
        /// </summary>
        public Node[] Connections
        {
            get => connections;
            set => connections = value;
        }
        [SerializeField] [HideInInspector]
        private Node[] connections = Array.Empty<Node>();
        
        /// <summary>
        /// The accepted/sent value type this port is capable of
        /// *Port type name is limited to 128 bytes of memory
        /// *Default type is "nothing"
        /// </summary>
        public Type PortType
        {
            get => Type.GetType(portType.ToString());
            set => portType = (FixedString128Bytes)value.AssemblyQualifiedName;
        }
        [SerializeField] [HideInInspector]
        private FixedString128Bytes portType = (FixedString128Bytes)typeof(Nothing).AssemblyQualifiedName;
    }

    /// <summary>
    /// Calls out a port at the index with a the object value
    /// *Property of name "Value" is sent to the connected port
    /// </summary>
    public struct PortCall
    {
        /// <summary>
        /// Index of the port to callout
        /// </summary>
        public byte Index { get; private set; }
        
        /// <summary>
        /// Value to send out of the requested port
        /// </summary>
        public object Value { get; private set; }

        public PortCall(byte index, object value)
        {
            Index = index;
            Value = value;
        }
    }
    
    /// <summary>
    /// Data value used to define a data-less port
    /// </summary>
    public struct Nothing {}
    
#if UNITY_EDITOR
    /// <summary>
    /// Details about the port used in the Jungle editor
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
        public string viewName;
        
        /// <summary>
        /// Position of the node inside the graph view
        /// </summary>
        public Vector2 position;
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
        public NodeColor Color { get; set; } = NodeColor.Blue;
        public enum NodeColor
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

        /// <summary>
        /// 
        /// </summary>
        public string InputPortName { get; set; } = "Execute";

        /// <summary>
        /// 
        /// </summary>
        public Type InputPortType { get; set; } = typeof(Nothing);

        /// <summary>
        /// 
        /// </summary>
        public string[] OutputPortNames { get; set; } = {"Next"};

        /// <summary>
        /// 
        /// </summary>
        public Type[] OutputPortTypes { get; set; } = {typeof(Nothing)};

        /// <summary>
        /// The nodes input port
        /// </summary>
        public PortInfo InputInfo => new PortInfo(InputPortName, InputPortType);

        /// <summary>
        /// List of the nodes output ports
        /// </summary>
        public PortInfo[] OutputInfo
        {
            get
            {
                if (OutputPortTypes.Length != 0 && OutputPortNames.Length != OutputPortTypes.Length)
                {
                    Debug.LogError($"[Node] {TitleName}s port names and port types do not match in count");
                }
                var query = new List<PortInfo>();
                for (var i = 0; i < OutputPortNames.Length; i++)
                {
                    var portName = OutputPortNames[i];
                    var portType = i <= OutputPortTypes.Length - 1
                        ? OutputPortTypes[i]
                        : typeof(Nothing);
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
            public string PortName { get; private set; }
            public Type PortType { get; private set; }

            public PortInfo(string portName, Type portType)
            {
                PortName = portName;
                PortType = portType;
            }
        }
    }
#endif
}
