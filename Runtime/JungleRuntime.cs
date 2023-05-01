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
       
        /// <summary>
        /// 
        /// </summary>
        public static JungleRuntime Singleton
        {
            get;
            private set;
        }
        
        private List<NodeTree> _persistentExecutingTrees = new List<NodeTree>();
        private List<NodeTree> _nonPersistentExecutingTrees = new List<NodeTree>();

        #endregion

        private void Awake()
        {
            if (Singleton != null)
            {
#if UNITY_EDITOR
                Debug.LogError("[Jungle Runtime] A Jungle runtime was instantiated while another instance already existed");
#endif
                enabled = false;
                return;
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
            var query = new List<NodeTree>();
            foreach (var nodeTree in ExecutingTrees)
            {
                if (nodeTree.State == NodeTree.TreeState.Finished)
                {
                    query.Add(nodeTree);
                    continue;
                }
                nodeTree.Update();
            }
            query.ForEach(nodeTree =>
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
        public bool StartTree(NodeTree nodeTree, bool persistent)
        {
            _persistentExecutingTrees ??= new List<NodeTree>();
            _nonPersistentExecutingTrees ??= new List<NodeTree>();
            if (ExecutingTrees.Contains(nodeTree))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[Jungle Runtime] Attempt to start {nodeTree.name} but it was already running");
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
            nodeTree.Start();
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
            if (_persistentExecutingTrees.Contains(nodeTree))
            {
                _persistentExecutingTrees.Remove(nodeTree);
            }
            else if (_nonPersistentExecutingTrees.Contains(nodeTree))
            {
                _nonPersistentExecutingTrees.Remove(nodeTree);
            }
            nodeTree.Stop();
            return true;
        }

        public bool StopNode(Node node)
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
