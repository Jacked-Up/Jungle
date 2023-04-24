using System.Collections.Generic;
using Jungle;

namespace Jungle.Nodes
{
    public class RootNode : BaseNode
    {
        #region Variables

        
        
        #endregion

        public override void Initialize() {}
        
        public override Verdict Execute()
        {
            return new Verdict(true, new List<int> { 0 });
        }
        
#if UNITY_EDITOR

        public override string ViewName() => "Start";

        public override List<string> PortNames => new() { "Begin" };
        
#endif
    }
}
