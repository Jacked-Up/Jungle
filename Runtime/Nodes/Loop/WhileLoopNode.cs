using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jungle.Nodes.Loop
{
    [NodeProperties(
        Title = "While Loop", 
        Tooltip = "Invokes until all nodes are finished.",
        Category = "Loop",
        Color = Purple
    )]
    [BranchNode(
        InputPortName = "Begin",
        InputPortType = typeof(None),
        OutputPortNames = new[] {"Invoke", "Done"},
        OutputPortTypes = new[] {typeof(None), typeof(None)}
    )]
    public class WhileLoopNode : BranchNode
    {
        #region Variables

        [SerializeField]
        private List<JungleNode> nodes = new();

        #endregion

        public override void OnStart(in object inputValue)
        {
            
        }

        public override void OnUpdate()
        {
            if (nodes.Any(node => node.IsRunning))
            {
                Call(new[] {new PortCall(0, true)});
                return;
            }
            CallAndStop(new[] {new PortCall(1, true)});
        }
    }
}
