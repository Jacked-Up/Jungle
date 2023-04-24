using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jungle
{
    [Serializable]
    public abstract class BaseNode : ScriptableObject, INode
    {
        #region Variables

        [HideInInspector]
        public NodeTree tree;
        
        [HideInInspector]
        public List<NodePort> ports;
        
        [NonSerialized]
        public bool Initialized;

        #endregion

        public abstract void Initialize();

        public abstract Verdict Execute();

#if UNITY_EDITOR
        [HideInInspector]
        public NodeProperties nodeProperties;

        public virtual string ViewName() => "Node";

        public virtual string Category() => string.Empty;
        
        public abstract List<string> PortNames { get; }
        
        public void AddChild(BaseNode node, int i)
        {
            if (ports == null || ports.Count != PortNames.Count)
            {
                ports = new List<NodePort>();
                PortNames.ForEach(_ =>
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

#if UNITY_EDITOR
    [Serializable]
    public struct NodeProperties
    {
        public string guid;
        public Vector2 graphPosition;

        public NodeProperties(string guid, Vector2 graphPosition)
        {
            this.guid = guid;
            this.graphPosition = graphPosition;
        }
    }

#endif
    
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
}
