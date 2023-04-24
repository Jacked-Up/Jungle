using UnityEngine;

namespace Jungle.Nodes
{
    [Node(ViewName = "Stop Node", Category = "Special", PortNames = new string[0])]
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
