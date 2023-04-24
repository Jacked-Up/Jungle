using System.Collections.Generic;
using Jungle;
using UnityEngine;

namespace Jungle.Nodes
{
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
        
#if UNITY_EDITOR

        public override string ViewName() => "Random Of Four";

        public override string Category() => "Operation";
        
        public override List<string> PortNames => new() {"One", "Two", "Three", "Four"};
        
#endif
    }
}