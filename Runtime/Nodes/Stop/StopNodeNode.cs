using UnityEngine;

namespace Jungle.Nodes.Stop
{
    [Node(ViewName = "Stop Node", Category = "Stop", NodeColor = NodeColor.Red, OutputPortNames = new string[0])]
    public class StopNodeNode : BaseNode
    {
        #region Variables

        [SerializeField] 
        private BaseNode nodeToStop;
        
        #endregion

        public override void Initialize() {}
        
        public override Verdict Execute()
        {
            if (tree.ExecutingNodes.Contains(nodeToStop))
            {
                tree.ExecutingNodes.Remove(nodeToStop);
            }
            return new Verdict(true);
        }
    }
}
