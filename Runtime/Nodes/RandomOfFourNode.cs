using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes
{
    [Node(ViewName = "Random of Four", Category = "Operation", PortNames = new []{"One", "Two", "Three", "Four"})]
    public class RandomOfFourNode : BaseNode
    {
        #region Variables
        
        

        #endregion

        public override void Initialize() {}

        public override Verdict Execute()
        {
            var choice = Random.Range(0, 3);
            return new Verdict(true, new List<int> {choice});
        }
    }
}
