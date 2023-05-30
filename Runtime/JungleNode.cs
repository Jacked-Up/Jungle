using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jungle
{
    /// <summary>
    /// Base node class inherited by all Jungle sequencer nodes.
    /// </summary>
    [Serializable] [Node]
    public abstract class JungleNode : ScriptableObject, INode
    {
        #region Variables

        /// <summary>
        /// Reference to the tree which this node is apart of.
        /// Recommended to never touch this.
        /// </summary>
        [SerializeField] [HideInInspector] 
        public JungleTree tree;
        
        /// <summary>
        /// Array of the output ports on this node.
        /// </summary>
        public Port[] OutputPorts => outputPorts;
        [SerializeField] [HideInInspector] 
        private Port[] outputPorts = Array.Empty<Port>();
        
        /// <summary>
        /// Jungle editor available node colors.
        /// </summary>
        public enum Color
        {
            Red,
            Orange,
            Yellow,
            Green,
            Teal,
            Blue,
            Purple,
            Violet,
            Grey
        }
        
        #endregion
        
        /// <summary>
        /// This method is invoked everytime the node is called. This method will only run once per node call and always
        /// runs before the "Execute" method. Should be used for initialization.
        /// </summary>
        /// <param name="inputValue">The value sent from a parent node.</param>
        public abstract void Initialize(in object inputValue);
         
        /// <summary>
        /// This method is called every frame while this node is running. On every execution, return its run state using
        /// true if finished, and false if still running. Port calls are used to send data to nodes connected to this
        /// nodes output(s).
        /// </summary>
        /// <param name="call">List of calls to send to connected nodes.</param>
        /// <returns>True if the node is finished executing and false if not.</returns>
        public abstract bool Execute(out PortCall[] call);

        /// <summary>
        /// This method is called by the Jungle validator while generating a report. Override this method to call out
        /// issues in your nodes.
        /// </summary>
        /// <param name="tryFix">True if the validator requested the tree to auto-fix.</param>
        /// <param name="issues">List of issues to report in the validator window. Set null/empty if there are no issues.</param>
        public virtual void Validate(in bool tryFix, out List<string> issues)
        {
            issues = null;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Jungle editor properties of the node.
        /// *Strongly recommended to not touch this.
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
        /// Main name of the node displayed in big letters inside the Jungle editor graph view.
        /// </summary>
        public string TitleName => NodeInfo.TitleName;

        /// <summary>
        /// Category placement of the node inside the create node window.
        /// </summary>
        public string Category => NodeInfo.Category;

        /// <summary>
        /// Displayed color of the node inside the Jungle editor graph view.
        /// </summary>
        public Color NodeColor => NodeInfo.Color;

        /// <summary>
        /// Name and data type info about the input port.
        /// </summary>
        public NodeAttribute.PortInfo InputInfo => NodeInfo.InputInfo;

        /// <summary>
        /// Name and data type info about the output ports.
        /// </summary>
        public NodeAttribute.PortInfo[] OutputInfo => NodeInfo.OutputInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="portIndex"></param>
        public void MakeConnection(JungleNode node, byte portIndex)
        {
            if (node.tree != tree)
            {
                // You cannot connect nodes from different trees
                return;
            }
            
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
                    return new Port(Array.Empty<JungleNode>(), info.PortType);
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
                    ? new List<JungleNode>(OutputPorts[i].connections) {node}.ToArray()
                    : OutputPorts[i].connections;
                newPortsList.Add(new Port(connections, portType));
            }
            outputPorts = newPortsList.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="portIndex"></param>
        public void RemoveConnection(JungleNode node, byte portIndex)
        {
            if (OutputPorts.Length != OutputInfo.Length)
            {
                return;
            }
            var outputPortsQuery = new List<Port>(OutputPorts);
            var outputPort = OutputPorts[portIndex];
            var connections = new List<JungleNode>(outputPort.connections);
            if (connections.Contains(node))
            {
                connections.Remove(node);
            }
            outputPortsQuery[portIndex] = new Port(connections.ToArray(), outputPort.PortType);
            outputPorts = outputPortsQuery.ToArray();
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
        /// List of the nodes that are connected to this port
        /// </summary>
        [SerializeField] [HideInInspector]
        public JungleNode[] connections;

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

        public Port(JungleNode[] connections, Type portType)
        {
            connections ??= Array.Empty<JungleNode>();
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
        /// Index of the port to send the value to 
        /// </summary>
        public byte PortIndex { get; private set; }

        /// <summary>
        /// Value to send out of the requested port
        /// </summary>
        public object Value { get; private set; }

        public PortCall(byte portIndex, object value)
        {
            PortIndex = portIndex;
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
        public JungleNode.Color Color { get; set; } = JungleNode.Color.Blue;

        /// <summary>
        /// 
        /// </summary>
        public string InputPortName { get; set; } = "Execute";

        /// <summary>
        /// 
        /// </summary>
        public Type InputPortType { get; set; } = typeof(None);

        /// <summary>
        /// 
        /// </summary>
        public string[] OutputPortNames { get; set; } = {"Next"};

        /// <summary>
        /// 
        /// </summary>
        public Type[] OutputPortTypes { get; set; } = { typeof(None) };

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
    public struct None { }
    
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
        [FormerlySerializedAs("notes")] public string comments;

        /// <summary>
        /// Position of the node inside the graph view
        /// </summary>
        public Vector2 position;
    }
#endif
}