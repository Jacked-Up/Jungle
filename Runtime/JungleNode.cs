using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jungle
{
    /// <summary>
    /// Base node class inherited by all Jungle nodes.
    /// </summary>
    [Serializable] [NodeProperties]
    public abstract class JungleNode : ScriptableObject, IJungleNode
    {
        #region Variables
        
        /// <summary>
        /// #DC1313FF
        /// </summary>
        public const string Red    = "#DC1313FF";
        
        /// <summary>
        /// #FF8500FF
        /// </summary>
        public const string Orange = "#FF8500FF";
        
        /// <summary>
        /// #D9BE12FF
        /// </summary>
        public const string Yellow = "#D9BE12FF";
        
        /// <summary>
        /// #00CC4AFF
        /// </summary>
        public const string Green  = "#00CC4AFF";
        
        /// <summary>
        /// #15DEABFF
        /// </summary>
        public const string Teal   = "#15DEABFF";
        
        /// <summary>
        /// #00EAFFFF
        /// </summary>
        public const string Cyan   = "#00EAFFFF";
        
        /// <summary>
        /// #0069FFFF
        /// </summary>
        public const string Blue   = "#0069FFFF";
        
        /// <summary>
        /// #B300FFFF
        /// </summary>
        public const string Purple = "#B300FFFF";
        
        /// <summary>
        /// #FF00EAFF
        /// </summary>
        public const string Pink   = "#FF00EAFF";
        
        /// <summary>
        /// #85034CFF
        /// </summary>
        public const string Violet = "#85034CFF";
        
        /// <summary>
        /// #FFFFFFFF
        /// </summary>
        public const string White  = "#FFFFFFFF";
        
        /// <summary>
        /// #101010FF
        /// </summary>
        public const string Black  = "#101010FF";

        /// <summary>
        /// Reference to this nodes Jungle Tree.
        /// </summary>
        public JungleTree Tree => tree;
        [SerializeField] [HideInInspector] 
        internal JungleTree tree;
        
        /// <summary>
        /// Array of the output ports on this node.
        /// </summary>
        public JunglePort[] OutputPorts => outputPorts;
        [SerializeField] [HideInInspector] 
        private JunglePort[] outputPorts = Array.Empty<JunglePort>();
        
        /// <summary>
        /// True if this Jungle Node is actively being executed by the Jungle Tree.
        /// </summary>
        public bool IsRunning => Tree.ExecutionList.Contains(this);

        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        public abstract void OnStart(in object inputValue);

        /// <summary>
        /// 
        /// </summary>
        public abstract void OnUpdate();

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
        public NodeEditorProperties NodeEditorProperties
        {
            get => nodeEditorProperties;
            set
            {
                nodeEditorProperties = value;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        [SerializeField] [HideInInspector] 
        private NodeEditorProperties nodeEditorProperties;
        
        private NodePropertiesAttribute NodePropertiesAttributeInfo
            => (NodePropertiesAttribute) GetType().GetCustomAttributes(typeof(NodePropertiesAttribute), true)[0];
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="portIndex"></param>
        public void MakeConnection(JungleNode node, byte portIndex)
        {
            /*
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
                    return new JunglePort(Array.Empty<JungleNode>(), info.Type);
                }).ToArray();
                outputPorts = repairedPortsList;
            }
            // Output ports type mismatch error
            var newPortsList = new List<JunglePort>();
            for (var i = 0; i < GetOutputs().Length; i++)
            {
                var portType = GetOutputs()[i].Type;
                if (OutputPorts[i].PortType != portType)
                {
                    Debug.LogError($"[{name}] Port by name \"{GetOutputs()[i].Name}\" was of type" +
                                   $" {OutputPorts[i].PortType} but is now of type {portType}." +
                                   " All port connections have been lost to prevent type mismatch errors");
                }
                var connections = i == portIndex 
                    ? new List<JungleNode>(OutputPorts[i].connections) {node}.ToArray()
                    : OutputPorts[i].connections;
                newPortsList.Add(new JunglePort(connections, portType));
            }
            outputPorts = newPortsList.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
            */
        }
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="portIndex"></param>
        public void RemoveConnection(JungleNode node, byte portIndex)
        {
            /*
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
            UnityEditor.EditorUtility.SetDirty(this);
            */
        }
#endif

        /// <summary>
        /// Returns the Jungle Nodes title name.
        /// </summary>
        public string GetTitle()
        {
            return NodePropertiesAttributeInfo.Title;
        }

        /// <summary>
        /// Returns a brief description of the Jungle Nodes function. 
        /// </summary>
        public string GetTooltip()
        {
            return NodePropertiesAttributeInfo.Tooltip;
        }

        /// <summary>
        /// Returns the Jungle Nodes category.
        /// </summary>
        public string GetCategory()
        {
            return NodePropertiesAttributeInfo.Category;
        }

        /// <summary>
        /// Returns the Jungle Nodes accent color.
        /// </summary>
        public Color GetColor()
        {
            return ColorUtility.TryParseHtmlString(NodePropertiesAttributeInfo.Color, out var color) 
                ? color 
                : Color.clear;
        }
        
        /// <summary>
        /// Returns the nodes cached icon.
        /// </summary>
        public Texture2D GetIcon()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorGUIUtility.ObjectContent(this, GetType()).image as Texture2D;
#else
            return Texture2D.whiteTexture;
#endif
        }
    }
    
    /// <summary>
    /// Base Jungle Node interface.
    /// </summary>
    public interface IJungleNode
    {
        public void OnStart(in object inputValue);
        public void OnUpdate();
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
                    portType = typeof(Unknown).AssemblyQualifiedName;
                    return typeof(Unknown);
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
    /// Container that stores the port ID and the value to send to the port.
    /// </summary>
    public struct PortCall
    {
        /// <summary>
        /// Index of the port to send the value to.
        /// </summary>
        public byte PortIndex { get; private set; }

        /// <summary>
        /// Value to send out of the requested port.
        /// </summary>
        public object Value { get; private set; }

        public PortCall(byte portIndex, object value)
        {
            PortIndex = portIndex;
            Value = value;
        }
    }
    
    /// <summary>
    /// Jungle Nodes editor properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodePropertiesAttribute : Attribute
    {
        /// <summary>
        /// The name of the Jungle Node.
        /// </summary>
        public string Title
        {
            get; 
            set;
        } = "Untitled Node";

        /// <summary>
        /// A brief description of the Jungle Nodes function.
        /// </summary>
        public string Tooltip
        {
            get; 
            set;
        } = string.Empty;

        /// <summary>
        /// Category path where this Jungle Node can be found in the Jungle Editor search window.
        /// </summary>
        public string Category
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// The Jungle Nodes accent color inside the Jungle Editor.
        /// *Hex Code*
        /// </summary>
        public string Color
        {
            get;
            set;
        } = JungleNode.Blue;
    }
    
    /// <summary>
    /// Jungle Node port type that denotes a port where no data is accepted or released.
    /// </summary>
    public struct None { }

    /// <summary>
    /// Jungle Node port type that denotes a port with an unknown type.
    /// </summary>
    public struct Unknown { }
    
    /// <summary>
    /// Jungle Node port information container.
    /// </summary>
    public struct PortInfo
    {
        /// <summary>
        /// Name of the port.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// 
        /// </summary>
        public readonly Type Type;
        
        public PortInfo(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Details about the node in the Jungle editor like GUID, position, and view name
    /// </summary>
    [Serializable]
    public struct NodeEditorProperties
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
}
