using System.Collections.Generic;

namespace Jungle
{
    [Node(ViewName = "Start", Category = "", OutputPortNames = new []{"Begin"}, Color = NodeColor.Green)]
    public class RootNode : BaseNode
    {
        public override void Initialize() {}
        
        public override Verdict Execute()
        {
            return new Verdict(true, new List<int> {0});
        }
    }
}
