using System.Collections.Generic;
using UnityEngine;

namespace Jungle
{
    public class JungleRuntime : MonoBehaviour
    {
        #region Variables

        public List<NodeTree> ExecutingTrees
        {
            get;
            private set;
        } = new();

        public static JungleRuntime Singleton
        {
            get;
            private set;
        }
        
        #endregion

        private void Awake()
        {
            Singleton = this;
        }

        private void Update()
        {
            var finishedTrees = new List<NodeTree>();
            foreach (var tree in ExecutingTrees)
            {
                if (tree.State == NodeTreeState.Executed)
                {
                    finishedTrees.Add(tree);
                    continue;
                }
                tree.PerformExecution();
            }
            finishedTrees.ForEach(nodeTree =>
            {
                ExecutingTrees.Remove(nodeTree);
            });
        }

        public void RunTree(NodeTree tree)
        {
            if (!tree.Begin())
            {
                return;
            }
            ExecutingTrees.Add(tree);
        }
    }
}
