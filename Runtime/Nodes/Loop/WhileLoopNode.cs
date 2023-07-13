using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jungle.Nodes.Loop
{
    [NodeProperties(Title = "While Loop", 
        Category = "Loop",
        Color = Purple,
        OutputPortNames = new []{"Invoke", "Done"}
    )]
    public class WhileLoopNode : JungleNode
    {
        #region Variables

        [SerializeField]
        private List<JungleNode> nodes = new List<JungleNode>();

        #endregion

        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
        {
            if (Tree.ExecutionList.Any(executingNode => nodes.Any(node => node == executingNode)))
            {
                call = new[] {new PortCall(0, true)};
                return false;
            }
            call = new[] {new PortCall(1, true)};
            return true;
        }
    }
}
