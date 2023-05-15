using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes.Stop
{
    [Node(TitleName = "Stop Tree(s)",
        Category = "Stop",
        Color = Color.Red,
        OutputPortNames = new string[0]
    )]
    public class StopTreeNode : Node
    {
        #region Variables

        [SerializeField] 
        private List<Tree> nodeTreesToStop = new List<Tree>();

        #endregion

        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
        {
            foreach (var node in nodeTreesToStop)
            {
                JungleRuntime.Singleton.StopTree(node);
            }
            call = new[] {new PortCall(0, true)};
            return true;
        }
    }
}
