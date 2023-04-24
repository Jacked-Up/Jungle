using System.Collections.Generic;

namespace Jungle.Nodes
{
    [Node(ViewName = "Start", Category = "", PortNames = new []{"Begin"})]
    public class RootNode : BaseNode
    {
        #region Variables

        
        
        #endregion

        public override void Initialize() {}
        
        public override Verdict Execute()
        {
            return new Verdict(true, new List<int> {0});
        }
    }
}
