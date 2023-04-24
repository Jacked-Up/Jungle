using System.Collections.Generic;
using Jungle;
using UnityEngine;

namespace Jungle.Nodes
{
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
        
#if UNITY_EDITOR
        public override string ViewName() => "Stop Node";

        public override string Category() => "Special";
        
        public override List<string> PortNames => new();
#endif
    }
}