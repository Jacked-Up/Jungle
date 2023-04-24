using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jungle
{
    [Serializable]
    [Node(ViewName = "Node", Category = "", PortNames = new []{"Next"})]
    public abstract class BaseNode : ScriptableObject, INode
    {
        #region Variables
        
        [HideInInspector]
        public NodeTree tree;
        
        [HideInInspector]
        public List<NodePort> ports;
        
        [NonSerialized]
        public bool Initialized;
        
        public string ViewName { get; set; }
        public string Category { get; set; }
        public string[] PortNames { get; set; }

        public NodeAttribute NodeInfo 
            => (NodeAttribute)GetType().GetCustomAttributes(typeof(NodeAttribute), true)[0];

#if UNITY_EDITOR
        [HideInInspector]
        public NodeProperties nodeProperties;
#endif
        
        #endregion

        private void Awake()
        {
            var nodeInfo = NodeInfo;
            ViewName = nodeInfo.ViewName;
            Category = nodeInfo.Category;
            PortNames = nodeInfo.PortNames;
        }
        
        public abstract void Initialize();

        public abstract Verdict Execute();

#if UNITY_EDITOR
        public void AddChild(BaseNode node, int i)
        {
            if (ports == null || ports.Count != PortNames.Length)
            {
                ports = new List<NodePort>();
                PortNames.ToList().ForEach(_ =>
                {
                    ports.Add(new NodePort(new List<BaseNode>()));
                });
            }
            if (ports[i].children.Contains(node)) return;
            ports[i].children.Add(node);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void RemoveChild(BaseNode node)
        {
            if (ports == null) return;
            for (var i = 0; i < ports.Count; i++)
            {
                if (ports[i].children == null)
                {
                    ports[i] = new NodePort(new List<BaseNode>());
                }
                if (!ports[i].children.Contains(node)) continue;
                ports[i].children.Remove(node);
                UnityEditor.EditorUtility.SetDirty(this);
                break;
            }
        }
#endif
    }

    [Serializable]
    public struct NodePort
    {
        public List<BaseNode> children;

        public NodePort(List<BaseNode> children)
        {
            this.children = children;
        }
    }
    
    public struct Verdict
    {
        public readonly bool Finished;
        public List<int> AlivePorts;

        public Verdict(bool finished, List<int> alivePorts = null)
        {
            Finished = finished;
            AlivePorts = alivePorts;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        public string ViewName { get; set; }
        public string Category { get; set; }
        public string[] PortNames { get; set; }
    }
    
#if UNITY_EDITOR
    [Serializable]
    public struct NodeProperties
    {
        public string guid;
        public Vector2 position;

        public NodeProperties(string guid, Vector2 position)
        {
            this.guid = guid;
            this.position = position;
        }
    }
#endif
}
