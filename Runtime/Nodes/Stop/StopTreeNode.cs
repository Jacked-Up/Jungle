using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes.Stop
{
    [Node(TitleName = "Stop Tree(s)",
        Category = "Stop",
        Color = NodeAttribute.NodeColor.Red,
        OutputPortNames = new string[0])]
    public class StopTreeNode : Node
    {
        #region Variables

        [SerializeField] 
        private List<NodeTree> nodeTreesToStop = new List<NodeTree>();

        #endregion

        public override void Start(in object inputValue)
        {
            
        }

        public override bool Update(out PortCall[] call)
        {
            foreach (var node in nodeTreesToStop)
            {
                JungleRuntime.Singleton.StopTree(node);
            }
            call = new[] {new PortCall(0, new Nothing())};
            return true;
        }
    }
}
