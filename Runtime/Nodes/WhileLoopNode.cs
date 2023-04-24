using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jungle.Nodes
{
    [Node(ViewName = "While Loop", Category = "Special", PortNames = new []{"Invoke", "Finished"})]
    public class WhileLoopNode : BaseNode
    {
        #region Variables

        [SerializeField]
        private List<BaseNode> nodes = new();

        #endregion

        public override void Initialize() {}
        
        public override Verdict Execute()
        {
            if (tree.ExecutingNodes.Any(executingNode => nodes.Any(node => node == executingNode)))
            {
                return new Verdict(false, new List<int>{0});
            }
            return new Verdict(true, new List<int>{1});
        }
    }
}
