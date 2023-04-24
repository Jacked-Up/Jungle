using System.Collections.Generic;
using System.Linq;
using Jungle;
using UnityEngine;

namespace Jungle.Nodes
{
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
        
#if UNITY_EDITOR
        public override string ViewName() => "While Loop";

        public override string Category() => "Special";

        public override List<string> PortNames => new() {"Invoke", "Finished"};
#endif
    }
}