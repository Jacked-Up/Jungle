using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jungle
{
    /// <summary>
    /// The Jungle sequencer base node class
    /// </summary>
    [Node]
    public class Node : ScriptableObject, INode
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
            get => inputPort;
            private set => inputPort = value;
        }

        [SerializeField] [HideInInspector] 
        private Port inputPort;

        /// <summary>
        /// Array of the nodes output ports
        /// </summary>
        public Port[] OutputPorts
        {
            get => outputPorts;
            private set => outputPorts = value;
        }
        [SerializeField] [HideInInspector] 
        private Port[] outputPorts = Array.Empty<Port>();

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
        public virtual void Initialize(in object inputValue)
        {
            
        }

        /// <summary>
        /// Invoked every frame just like the Unity update event method
        /// </summary>
        /// <param name="call">Ports to invoke during this update call</param>
        /// <returns>Returns true if the node has declared it is finished executing. Returns false if not</returns>
        public virtual bool Execute(out PortCall[] call)
        {
            call = new PortCall[] {new(0, new Nothing())};
            return true;
        }

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
        public void AddOutputConnection(Node node, int index)
        {
            if (OutputPorts.Length != OutputInfo.Length)
            {
                Debug.Log("A");
                
                var portQuery = new List<Port>();
                foreach (var info in OutputInfo)
                {
                    portQuery.Add(new Port(Array.Empty<Node>(), info.PortType));
                }
                OutputPorts = portQuery.ToArray();
            }
            
            var portsList = new List<Port>();
            for (var i = 0; i < OutputPorts.Length; i++)
            {
                if (i != index)
                {
                    portsList.Add(OutputPorts[i]);
                    continue;
                }
                var connectionsList = new List<Node>(OutputPorts[i].Connections);
                if (connectionsList.Contains(node))
                {
                    portsList.Add(OutputPorts[i]);
                    continue;
                }
                connectionsList.Add(node);
                portsList.Add(new Port(connectionsList.ToArray(), OutputPorts[i].PortType));
            }
            outputPorts = portsList.ToArray();
        }

        /// <summary>
        /// Removes the node as a child of this node
        /// </summary>
        /// <param name="node">Node to remove as a connection</param>
        public void RemoveOutputConnection(Node node)
        {
            var portList = new List<Port>(OutputPorts);
            for (var i = 0; i < portList.Count; i++)
            {
                var connectionsList = new List<Node>(portList[i].Connections);
                if (!connectionsList.Contains(node)) continue;
                connectionsList.Remove(node);
                portList[i] = new Port(connectionsList.ToArray(), portList[i].PortType);
                break;
            }
            outputPorts = portList.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void AddInputConnection(Node node)
        {
            var connectionsList = new List<Node>(inputPort.Connections);
            if (connectionsList.Contains(node))
            {
                return;
            }
            connectionsList.Add(node);
            inputPort.Connections = connectionsList.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void RemoveInputConnection(Node node)
        {
            var connectionsList = new List<Node>(inputPort.Connections);
            if (!connectionsList.Contains(node))
            {
                return;
            }
            connectionsList.Remove(node);
            inputPort.Connections = connectionsList.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
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
        /// List connections to this port
        /// </summary>
        public Node[] Connections
        {
            get
            {
                connections ??= Array.Empty<Node>();
                return connections;
            }
            set => connections = value;
        }
        [SerializeField] [HideInInspector] 
        private Node[] connections;

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
                    portType = typeof(Nothing).AssemblyQualifiedName;
                    return typeof(Nothing);
                }

                return Type.GetType(portType);
            }
        }
        [SerializeField] [HideInInspector] 
        private string portType;

        public Port(Node[] connections, Type portType)
        {
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
    /// Data value used to define a data-less port
    /// </summary>
    public struct Nothing
    {
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
        public string viewName;

        /// <summary>
        /// Position of the node inside the graph view
        /// </summary>
        public Vector2 position;
    }
#endif
}