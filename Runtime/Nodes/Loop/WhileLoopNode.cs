﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jungle.Nodes.Loop
{
    [Node(TitleName = "While Loop", 
        Category = "Loop",
        Color = Color.Purple,
        OutputPortNames = new []{"Invoke", "Done"}
    )]
    public class WhileLoopNode : Node
    {
        #region Variables

        [SerializeField]
        private List<Node> nodes = new List<Node>();

        #endregion

        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
        {
            if (tree.ExecutingNodes.Any(executingNode => nodes.Any(node => node == executingNode)))
            {
                call = new[] {new PortCall(0, true)};
                return false;
            }
            call = new[] {new PortCall(1, true)};
            return true;
        }
    }
}