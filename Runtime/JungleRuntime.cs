using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jungle
{
    public class JungleRuntime : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// List of executing node trees. (Includes both persistent and non-persistent trees)
        /// </summary>
        public List<Tree> ExecutingTrees
        {
            get
            {
                var combinedList = new List<Tree>();
                combinedList.AddRange(_persistentExecutingTrees);
                combinedList.AddRange(_nonPersistentExecutingTrees);
                return combinedList;
            }
        }
       
        /// <summary>
        /// Jungle runtime mono behaviour reference
        /// </summary>
        public static JungleRuntime Singleton
        {
            get;
            private set;
        }
        
        private List<Tree> _persistentExecutingTrees = new();
        private List<Tree> _nonPersistentExecutingTrees = new();

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
            var query = new List<Tree>();
            foreach (var nodeTree in ExecutingTrees)
            {
                if (nodeTree.State == Tree.TreeState.Finished)
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
        /// <param name="tree">Node tree to add to execution queue.</param>
        /// <param name="persistent">If the node tree should persist between scene changes.</param>
        /// <returns>True if the node tree was initialized and queued for execution.</returns>
        public bool StartTree(Tree tree, bool persistent)
        {
            _persistentExecutingTrees ??= new List<Tree>();
            _nonPersistentExecutingTrees ??= new List<Tree>();
            if (ExecutingTrees.Contains(tree))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[Jungle Runtime] Attempt to start {tree.name} but it was already running");
#endif
                return false;
            }
            if (persistent)
            {
                _persistentExecutingTrees.Add(tree);
            }
            else
            {
                _nonPersistentExecutingTrees.Add(tree);
            }
            tree.Start();
            return true;
        }
        
        /// <summary>
        /// Stops the execution of a node tree.
        /// </summary>
        /// <param name="tree">Node tree to remove from execution.</param>
        /// <returns>True if the node tree was remove from the execution queue.</returns>
        public bool StopTree(Tree tree)
        {
            _persistentExecutingTrees ??= new List<Tree>();
            _nonPersistentExecutingTrees ??= new List<Tree>();
            if (!ExecutingTrees.Contains(tree))
            {
                return false;
            }
            if (_persistentExecutingTrees.Contains(tree))
            {
                _persistentExecutingTrees.Remove(tree);
            }
            else if (_nonPersistentExecutingTrees.Contains(tree))
            {
                _nonPersistentExecutingTrees.Remove(tree);
            }
            tree.Stop();
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
            _nonPersistentExecutingTrees ??= new List<Tree>(); 
            foreach (var nodeTree in _nonPersistentExecutingTrees)
            {
                StopTree(nodeTree);
            }
        }
    }
}
