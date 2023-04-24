using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes.Stop
{
    [Node(ViewName = "Stop Tree(s)", Category = "Stop", NodeColor = NodeColor.Red, OutputPortNames = new string[0])]
    public class StopTreeNode : BaseNode
    {
        #region Variables

        [SerializeField] 
        private List<NodeTree> nodeTreesToStop = new();

        #endregion

        public override void Initialize()
        {
            nodeTreesToStop ??= new List<NodeTree>();
        }
        
        public override Verdict Execute()
        {
            foreach (var nodeTree in nodeTreesToStop)
            {
                JungleRuntime.Singleton.StopTree(nodeTree);
            }
            return new Verdict(true);
        }
    }
}
