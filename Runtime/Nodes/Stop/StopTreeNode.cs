namespace Jungle.Nodes.Stop
{
    [Node(ViewName = "Stop Tree", Category = "Stop", NodeColor = NodeColor.Red, OutputPortNames = new string[0])]
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
    }
}
