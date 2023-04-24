using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes.Operation
{
    [Node(ViewName = "Random of Two", Category = "Operation", Color = NodeColor.Orange, OutputPortNames = new []{"One", "Two"})]
    public class RandomOfTwoNode : BaseNode
    {
        #region Variables
        
        

        #endregion

        public override void Initialize() {}

        public override Verdict Execute()
        {
            var choice = Random.Range(0, 1);
            return new Verdict(true, new List<int> {choice});
        }
    }
}
