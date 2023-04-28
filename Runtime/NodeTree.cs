using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle
{
    [Serializable]
    [CreateAssetMenu(fileName = "Node Tree", menuName = "Jungle Node Tree")]
    public class NodeTree : ScriptableObject
    {
        #region Variables

        [HideInInspector]
        public BaseNode rootNode;

        [HideInInspector]
        public List<BaseNode> nodes = new();

        [NonSerialized]
        public NodeTreeState State = NodeTreeState.Unexecuted;

        public List<BaseNode> ExecutingNodes { get; private set; } = new();

        public List<Delegate> RevertDelegates { get; private set; } = new();
        
        #endregion

        public bool Start()
        {
            if (State == NodeTreeState.Executing)
            {
                return false;
            }
            ExecutingNodes = new List<BaseNode> {rootNode};
            State = NodeTreeState.Unexecuted;
            return true;
        }

        public void Stop()
        {
            ExecutingNodes = new List<BaseNode>();
            State = NodeTreeState.Executed;
        }
        
        public void PerformExecution()
        {
            if (ExecutingNodes.Count == 0 && State != NodeTreeState.Executed)
            {
                State = NodeTreeState.Executed;
                return;
            }
            var refreshedExecutingNodes = new List<BaseNode>(ExecutingNodes);
            foreach (var node in ExecutingNodes)
            {
                if (!node.Initialized)
                {
                    node.Initialize();
                    node.Initialized = true;
                }
                var verdict = node.Execute();
                verdict.AlivePorts ??= new List<int>();
                foreach (var alivePortIndex in verdict.AlivePorts)
                {
                    if (alivePortIndex > node.ports.Count - 1 || alivePortIndex < 0) continue;
                    var subChildren = node.ports[alivePortIndex].children;
                    subChildren.ForEach(subChild =>
                    {
                        if (!refreshedExecutingNodes.Contains(subChild))
                        {
                            refreshedExecutingNodes.Add(subChild);
                        }
                    });
                }
                if (verdict.Finished)
                {
                    refreshedExecutingNodes.Remove(node);
                }
            }
            ExecutingNodes = refreshedExecutingNodes;
        }

#if UNITY_EDITOR
        public BaseNode CreateNode(System.Type node, Vector2 graphPosition)
        {
            var nodeInstance = CreateInstance(node) as BaseNode;
            if (nodeInstance == null) return null;
            
            nodeInstance.name = $"{node.Name}";
            nodeInstance.tree = this;
            nodeInstance.NodeProperties = new NodeProperties
            {
                guid = GUID.Generate().ToString(),
                name = node.Name,
                comments = "Notes\n \n",
                position = graphPosition
            };
            
            Undo.RecordObject(this, $"Added {nodeInstance.name} to tree");
            nodes.Add(nodeInstance);
            
            AssetDatabase.AddObjectToAsset(nodeInstance, this);
            Undo.RegisterCreatedObjectUndo(nodeInstance, $"Added {nodeInstance.name} to tree");
            AssetDatabase.SaveAssets();
            
            return nodeInstance;
        }

        public BaseNode DuplicateNode(BaseNode node)
        {
            var nodeCopy = Instantiate(node);
            if (nodeCopy == null) return null;
            
            nodeCopy.name = node.name;
            nodeCopy.tree = this;
            nodeCopy.NodeProperties = new NodeProperties
            {
                guid = GUID.Generate().ToString(),
                name = nodeCopy.NodeProperties.name,
                comments = nodeCopy.NodeProperties.comments,
                position = node.NodeProperties.position + new Vector2(25, 25)
            };
            nodeCopy.ports = new List<NodePort>();
            
            Undo.RecordObject(this, $"Added {nodeCopy.name} to tree");
            nodes.Add(nodeCopy);
            
            AssetDatabase.AddObjectToAsset(nodeCopy, this);
            Undo.RegisterCreatedObjectUndo(nodeCopy, $"Added {nodeCopy.name} to tree");
            AssetDatabase.SaveAssets();
            
            return nodeCopy;
        }
        
        public void DeleteNode(BaseNode node)
        {
            Undo.RecordObject(this, $"Deleted {node.name} from tree");
            nodes.Remove(node);
            
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }

        public void AddChild(BaseNode parent, BaseNode child, int index)
        {
            Undo.RecordObject(parent, $"Added edge to {parent.name}");
            parent.AddChild(child, index);
        }
        
        public void RemoveChild(BaseNode parent, BaseNode child)
        {
            Undo.RecordObject(parent, $"Removed edge from {parent.name}");
            parent.RemoveChild(child);
        }
#endif
    }
    
    public enum NodeTreeState
    {
        Unexecuted,
        Executing,
        Executed
    }
}
