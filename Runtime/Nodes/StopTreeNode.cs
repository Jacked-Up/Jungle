using System.Collections.Generic;
using Jungle;

namespace Jungle.Nodes
{
    public class StopTreeNode : BaseNode
    {
        #region Variables

        
        
        #endregion

        public override void Initialize() {}
        
        public override Verdict Execute()
        {
            tree.Stop();
            return new Verdict(true);
        }
        
#if UNITY_EDITOR
        public override string ViewName() => "Stop Tree";

        public override string Category() => "Special";

        public override List<string> PortNames => new();
#endif
    }
}