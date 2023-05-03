using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes.Stop
{
    [Node(TitleName = "Stop Node(s)",
        Category = "Stop",
        Color = NodeAttribute.NodeColor.Red,
        OutputPortNames = new string[0])]
    public class StopNodeNode : Node
    {
        #region Variables

        [SerializeField]
        private List<Node> nodesToStop = new List<Node>();

        #endregion

        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
        {
            foreach (var node in nodesToStop)
            {
                JungleRuntime.Singleton.StopNode(node);
            }
            call = new[] {new PortCall(0, new Nothing())};
            return true;
        }
    }
}
