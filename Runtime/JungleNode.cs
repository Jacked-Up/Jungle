using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle
{
    /// <summary>
    /// Base node class inherited by all Jungle nodes.
    /// </summary>
    [Serializable] [Node]
    public abstract class JungleNode : ScriptableObject, INode
    {
        #region Variables

        /// <summary>
        /// Reference to this nodes Jungle Tree.
        /// </summary>
        public JungleTree Tree
        {
            get => tree;
            set
            {
                if (tree != null)
                {
                    if (tree == value) return;
#if UNITY_EDITOR
                    Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, tree,
                        $"[{name}] You cannot set the Jungle Tree reference after it has already been set.");
#endif
                    return;
                }
                tree = value;
            }
        }
        [SerializeField] [HideInInspector] 
        private JungleTree tree;
        
        /// <summary>
        /// Array of the output ports on this node.
        /// </summary>
        public JunglePort[] OutputPorts => outputPorts;
        [SerializeField] [HideInInspector] 
        private JunglePort[] outputPorts = Array.Empty<JunglePort>();

        /// <summary>
        /// True if the node is actively being executed by the Jungle Tree.
        /// </summary>
        public bool IsRunning => Tree.ExecutionList.Contains(this);

        /// <summary>
        /// List of Jungle Node view colors.
        /// </summary>
        public enum Color
        {
            Red,
            Orange,
            Yellow,
            Green,
            Teal,
            Cyan,
            Blue,
            Purple,
            Pink,
            Violet,
            White,
            Black
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
        /// Editor property cache for the Jungle editor.
        /// </summary>
        public NodeProperties NodeProperties
        {
            get => nodeProperties;
            set
            {
                nodeProperties = value;
                EditorUtility.SetDirty(this);
            }
        }
        [SerializeField] [HideInInspector] 
        private NodeProperties nodeProperties;

        /// <summary>
        /// Returns the nodes title name.
        /// </summary>
        public string GetTitle()
        {
            return NodeInfo.Title;
        }
        
        /// <summary>
        /// Returns a description of the nodes function. 
        /// </summary>
        public string GetTooltip()
        {
            return NodeInfo.Tooltip;
        }
        
        /// <summary>
        /// Returns the nodes category.
        /// </summary>
        public string GetCategory()
        {
            return NodeInfo.Category;
        }
        
        /// <summary>
        /// Returns the nodes accent color.
        /// </summary>
        public UnityEngine.Color GetColor()
        {
            // Remove all stray hashtags from the hex code
            var colorString = NodeInfo.Color.Replace("#", string.Empty);
            if (ColorUtility.TryParseHtmlString($"#{colorString}", out var color))
            { 
                return color;
            }
            return UnityEngine.Color.clear;
        }
        
        /// <summary>
        /// Returns the nodes cached icon.
        /// </summary>
        public Texture2D GetIcon()
        {
            return EditorGUIUtility.ObjectContent(this, GetType()).image as Texture2D;
        }
        
        /// <summary>
        /// Returns the input port name and value type.
        /// </summary>
        public NodeAttribute.PortInfo GetInput()
        {
            return NodeInfo.InputInfo;
        }

        /// <summary>
        /// Returns all a list array of output ports names and value types.
        /// </summary>
        public NodeAttribute.PortInfo[] GetOutputs()
        {
            return NodeInfo.OutputInfo;
        }
        
        private NodeAttribute NodeInfo 
            => (NodeAttribute)GetType().GetCustomAttributes(typeof(NodeAttribute), true)[0];
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="portIndex"></param>
        public void MakeConnection(JungleNode node, byte portIndex)
        {
            if (node.Tree != Tree)
            {
                // You cannot connect nodes from different trees
                return;
            }
            
            // Fix any mismatches with the port attributes
            // Output ports count error
            if (OutputPorts.Length != GetOutputs().Length)
            {
                if (OutputPorts.Length != 0)
                {
                    Debug.LogError($"[{name}] Output port count was changed was changed. All connections have " +
                                   "been lost to prevent type mismatch errors");
                }
                var repairedPortsList = GetOutputs().Select(info =>
                {
                    return new JunglePort(Array.Empty<JungleNode>(), info.PortType);
                }).ToArray();
                outputPorts = repairedPortsList;
            }
            // Output ports type mismatch error
            var newPortsList = new List<JunglePort>();
            for (var i = 0; i < GetOutputs().Length; i++)
            {
                var portType = GetOutputs()[i].PortType;
                if (OutputPorts[i].PortType != portType)
                {
                    Debug.LogError($"[{name}] Port by name \"{GetOutputs()[i].PortName}\" was of type" +
                                   $" {OutputPorts[i].PortType} but is now of type {portType}." +
                                   " All port connections have been lost to prevent type mismatch errors");
                }
                var connections = i == portIndex 
                    ? new List<JungleNode>(OutputPorts[i].connections) {node}.ToArray()
                    : OutputPorts[i].connections;
                newPortsList.Add(new JunglePort(connections, portType));
            }
            outputPorts = newPortsList.ToArray();
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="portIndex"></param>
        public void RemoveConnection(JungleNode node, byte portIndex)
        {
            if (OutputPorts.Length != GetOutputs().Length)
            {
                return;
            }
            var outputPortsQuery = new List<JunglePort>(OutputPorts);
            var outputPort = OutputPorts[portIndex];
            var connections = new List<JungleNode>(outputPort.connections);
            if (connections.Contains(node))
            {
                connections.Remove(node);
            }
            outputPortsQuery[portIndex] = new JunglePort(connections.ToArray(), outputPort.PortType);
            outputPorts = outputPortsQuery.ToArray();
            EditorUtility.SetDirty(this);
        }
#endif
    }

    /// <summary>
    /// Base port class. Used for both input and output ports
    /// </summary>
    [Serializable]
    public struct JunglePort
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

        public JunglePort(JungleNode[] connections, Type portType)
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
        /// The name the node goes by.
        /// </summary>
        public string Title
        {
            get; 
            set;
        } = "Untitled Node";

        /// <summary>
        /// Tooltip displayed while hovering over the node.
        /// </summary>
        public string Tooltip
        {
            get; 
            set;
        } = string.Empty;

        /// <summary>
        /// ...
        /// </summary>
        public string Category
        {
            get; 
            set;
        } = string.Empty;

        /// <summary>
        /// The hex code color of the nodes accent.
        /// </summary>
        public string Color
        {
            get;
            set;
        } = JungleNodeColors.Blue;
        
        /// <summary>
        /// 
        /// </summary>
        public string InputPortName
        {
            get; 
            set;
        } = "Execute";

        /// <summary>
        /// 
        /// </summary>
        public Type InputPortType
        {
            get; 
            set;
        } = typeof(None);

        /// <summary>
        /// 
        /// </summary>
        public string[] OutputPortNames
        {
            get;
            set;
        } = {"Next"};

        /// <summary>
        /// 
        /// </summary>
        public Type[] OutputPortTypes
        {
            get; 
            set;
        } = {typeof(None)};

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
                var outputPortCount = OutputPortNames.Length == OutputPortTypes.Length
                    ? OutputPortNames.Length
                    : OutputPortNames.Length > OutputPortTypes.Length 
                        ? OutputPortNames.Length 
                        : OutputPortTypes.Length;
                
                var infoList = new List<PortInfo>();
                for (var i = 0; i < outputPortCount; i++)
                {
                    var portName = !(i > OutputPortNames.Length - 1)
                        ? OutputPortNames[i]
                        : "ERROR";
                    var portType = !(i > OutputPortTypes.Length - 1)
                        ? OutputPortTypes[i]
                        : typeof(Error);
                    infoList.Add(new PortInfo(portName, portType));
                }
                return infoList.ToArray();
            }
        }

        /// <summary>
        /// Contains info about the ports name and value type.
        /// </summary>
        public struct PortInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public string PortName { get; }
            
            /// <summary>
            /// 
            /// </summary>
            public Type PortType { get; }

            public PortInfo(string portName, Type portType)
            {
                PortName = portName;
                PortType = portType;
            }
        }
    }

    /// <summary>
    /// Default Jungle node type.
    /// </summary>
    public struct None { }
    
    /// <summary>
    /// Error state for Jungle node ports.
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
        /// Position of the node inside the graph view
        /// </summary>
        public Vector2 position;
    }
#endif
    
    /// <summary>
    /// List of hex codes for Jungle Node accent color.
    /// </summary>
    public static class JungleNodeColors
    {
        public const string Red =     "DC1313FF";
        public const string Orange = "FF8500FF";
        public const string Yellow = "D9BE12FF";
        public const string Green =  "00CC4AFF";
        public const string Teal =   "15DEABFF";
        public const string Cyan =   "00EAFFFF";
        public const string Blue =   "0069FFFF";
        public const string Purple = "B300FFFF";
        public const string Pink =   "FF00EAFF";
        public const string Violet = "85034CFF";
        public const string White =  "FFFFFFFF";
        public const string Black =  "101010FF";
    }
}