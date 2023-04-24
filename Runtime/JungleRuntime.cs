using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jungle
{
    public class JungleRuntime : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// Returns a list of executing node trees. (Includes both persistent and non-persistent trees)
        /// </summary>
        public List<NodeTree> ExecutingTrees
        {
            get
            {
                var combinedList = new List<NodeTree>();
                combinedList.AddRange(_persistentExecutingTrees);
                combinedList.AddRange(_nonPersistentExecutingTrees);
                return combinedList;
            }
        }
       
        public static JungleRuntime Singleton
        {
            get;
            private set;
        }
        
        private List<NodeTree> _persistentExecutingTrees = new();
        private List<NodeTree> _nonPersistentExecutingTrees = new();

        #endregion

        private void Awake()
        {
            if (Singleton != null)
            {
#if UNITY_EDITOR
                Debug.LogError("[Jungle Runtime] A Jungle runtime was instantiated while another instance already existed");
#endif
                Destroy(Singleton);
            }
            Singleton = this;
        }

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += SceneUnloadedCallback;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= SceneUnloadedCallback;
        }

        private void Update()
        {
            var finishedTrees = new List<NodeTree>();
            foreach (var nodeTree in ExecutingTrees)
            {
                if (nodeTree.State == NodeTreeState.Executed)
                {
                    finishedTrees.Add(nodeTree);
                    continue;
                }
                nodeTree.PerformExecution();
            }
            finishedTrees.ForEach(nodeTree =>
            {
                StopTree(nodeTree);
            });
        }

        /// <summary>
        /// Initializes and executes a node tree.
        /// </summary>
        /// <param name="nodeTree">Node tree to add to execution queue.</param>
        /// <param name="persistent">If the node tree should persist between scene changes.</param>
        /// <returns>True if the node tree was initialized and queued for execution.</returns>
        public bool RunTree(NodeTree nodeTree, bool persistent)
        {
            _persistentExecutingTrees ??= new List<NodeTree>();
            _nonPersistentExecutingTrees ??= new List<NodeTree>();
            if (!nodeTree.Start() && !ExecutingTrees.Contains(nodeTree))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[Jungle Runtime] Attempt to execute {nodeTree.name} but it was already running OR is already queued for execution");
#endif
                return false;
            }
            if (persistent)
            {
                _persistentExecutingTrees.Add(nodeTree);
            }
            else
            {
                _nonPersistentExecutingTrees.Add(nodeTree);
            }
            return true;
        }
        
        /// <summary>
        /// Stops the execution of a node tree.
        /// </summary>
        /// <param name="nodeTree">Node tree to remove from execution.</param>
        /// <returns>True if the node tree was remove from the execution queue.</returns>
        public bool StopTree(NodeTree nodeTree)
        {
            _persistentExecutingTrees ??= new List<NodeTree>();
            _nonPersistentExecutingTrees ??= new List<NodeTree>();
            if (!ExecutingTrees.Contains(nodeTree))
            {
                return false;
            }
            nodeTree.Stop();
            if (_persistentExecutingTrees.Contains(nodeTree))
            {
                _persistentExecutingTrees.Remove(nodeTree);
            }
            else if (_nonPersistentExecutingTrees.Contains(nodeTree))
            {
                _nonPersistentExecutingTrees.Remove(nodeTree);
            }
            return true;
        }

        public bool StopNode(BaseNode node)
        {
            foreach (var executingTree in ExecutingTrees)
            {
                if (!executingTree.ExecutingNodes.Contains(node))
                {
                    continue;
                }
                executingTree.ExecutingNodes.Remove(node);
                return true;
            }
            return false;
        }
        
        private void SceneUnloadedCallback(Scene _)
        {
            _nonPersistentExecutingTrees ??= new List<NodeTree>();
            foreach (var nodeTree in _nonPersistentExecutingTrees)
            {
                StopTree(nodeTree);
            }
        }
    }
}
