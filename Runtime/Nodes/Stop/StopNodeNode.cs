using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes.Stop
{
    [Node(ViewName = "Stop Node(s)", Category = "Stop", NodeColor = NodeColor.Red, OutputPortNames = new string[0])]
    public class StopNodeNode : BaseNode
    {
        #region Variables

        [SerializeField]
        private List<BaseNode> nodesToStop = new();

        #endregion

        public override void Initialize() {}
        
        public override Verdict Execute()
        {
            foreach (var node in nodesToStop)
            {
                JungleRuntime.Singleton.StopNode(node);
            }
            return new Verdict(true);
        }
    }
}
