using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle
{
    [CreateAssetMenu(fileName = "Node Tree", menuName = "Jungle Node Tree")]
    public class NodeTree : ScriptableObject
    {
        #region Variables
        
        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public Node rootNode;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public Node[] nodes;

        /// <summary>
        /// 
        /// </summary>
        public List<Node> ExecutingNodes { get; private set; } = new List<Node>();

        /// <summary>
        /// 
        /// </summary>
        public TreeState State { get; private set; } = TreeState.Ready;
        /// <summary>
        /// Status flag of the node tree
        /// </summary>
        public enum TreeState
        {
            /// <summary>
            /// Describes a node that has never been run and is not currently running
            /// </summary>
            Ready,
            /// <summary>
            /// Describes a node that is currently running
            /// </summary>
            Running,
            /// <summary>
            /// Describes a node that is not currently running and has run at some point
            /// </summary>
            Finished
        }
        
        #endregion

        /// <summary>
        /// Initializes the node tree for execution
        /// </summary>
        public void Start()
        {
            if (State == TreeState.Running) return;
            ExecutingNodes = new List<Node> {rootNode};
            ExecutingNodes[0].Initialize(new Nothing());
            State = TreeState.Running;
        }

        /// <summary>
        /// Stops the execution of the node tree
        /// </summary>
        public void Stop()
        {
            State = TreeState.Finished;
        }
        
        /// <summary>
        /// Executes all listening nodes in the node tree and removes nodes which have finished execution
        /// </summary>
        public void Update()
        {
            if (State == TreeState.Finished) return;
            var query = new List<Node>(ExecutingNodes);
            foreach (var node in ExecutingNodes)
            {
                var verdict = node.Execute(out var portCalls);
                foreach (var call in portCalls)
                {
                    if (call.Index < 0 || call.Index > node.OutputPorts.Length - 1)
                    {
#if UNITY_EDITOR
                        Debug.LogError($"[{name}] {node.name} attempted to call an output port that is out of the index range");        
#endif
                        continue;
                    }
                    foreach (var connection in node.OutputPorts[call.Index].Connections)
                    {
                        connection.Initialize(call.Value);
                    }
                }
                if (verdict)
                {
                    query.Remove(node);
                }
            }
            // Populate executing nodes with new query ONLY if it has changed
            // I believe this prevents the list from redundantly reallocating new memory
            if (!ExecutingNodes.Equals(query)) ExecutingNodes = query;
            if (ExecutingNodes.Count == 0) State = TreeState.Finished;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public Node CreateNode(Type nodeType, Vector2 position)
        {
            var node = CreateInstance(nodeType) as Node;
            if (node == null) return null;
            
            // Create unique file name
            var i = 0;
            var path = $"{AssetDatabase.GetAssetPath(this)}/{name}_{nodeType.Name}_{i.ToString()}.asset";
            while (AssetDatabase.LoadAssetAtPath(path, typeof(Node)) != null) i++;
            node.name = $"{name}_{nodeType.Name}_{i.ToString()}";
            
            // Build node and populate graph view properties
            node.Tree = this;
            node.NodeProperties = new NodeProperties
            {
                guid = GUID.Generate().ToString(),
                viewName = string.Empty,
                position = position
            };
            
            // Add new node instance to list
            Undo.RecordObject(this, $"Added {node.name} to tree");
            var nodesList = new List<Node>();
            nodes ??= Array.Empty<Node>();
            nodesList.AddRange(nodes.ToList());
            nodesList.Add(node);
            nodes = nodesList.ToArray();
            
            // Add new node instance to this node tree asset
            AssetDatabase.AddObjectToAsset(node, this);
            Undo.RegisterCreatedObjectUndo(node, $"Added {node.name} to tree");
            EditorUtility.SetDirty(this);
            
            return node;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public Node DuplicateNode(Node original)
        {
            var node = Instantiate(original);
            if (node == null) return null;
            
            // Create unique file name
            var i = 0;
            var path = $"{AssetDatabase.GetAssetPath(this)}/{name}_{original.GetType().Name}_{i.ToString()}.asset";
            while (AssetDatabase.LoadAssetAtPath(path, typeof(Node)) != null) i++;
            node.name = $"{name}_{original.GetType().Name}_{i.ToString()}";
            
            // Build node and populate graph view properties
            node.Tree = this;
            node.NodeProperties = new NodeProperties
            {
                guid = GUID.Generate().ToString(),
                viewName = node.NodeProperties.viewName,
                position = original.NodeProperties.position + new Vector2(35, 35)
            };
            
            // Add new node instance to list
            Undo.RecordObject(this, $"Added {node.name} to tree");
            var query = nodes.ToList();
            query.Add(node);
            nodes = query.ToArray();
            
            // Add new node instance to this node tree asset
            AssetDatabase.AddObjectToAsset(node, this);
            Undo.RegisterCreatedObjectUndo(node, $"Added {node.name} to tree");
            EditorUtility.SetDirty(this);

            return node;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void DeleteNode(Node node)
        {
            var query = nodes.ToList();
            if (!query.Contains(node)) return;
            Undo.RecordObject(this, $"Deleted {node.name} from tree");
            query.Remove(node);
            Undo.DestroyObjectImmediate(node);
            nodes = query.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="index"></param>
        public void CreateConnection(Node parent, Node child, int index)
        {
            Undo.RecordObject(parent, $"Added edge to {parent.name}");
            if (parent.AddOutputConnection(child, index))
            {
                child.AddInputConnection(parent);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public void RemoveConnection(Node parent, Node child)
        {
            Undo.RecordObject(parent, $"Removed edge from {parent.name}");
            if (parent.RemoveOutputConnection(child))
            {
                child.RemoveInputConnection(parent);
            }
        }
#endif
    }
}
