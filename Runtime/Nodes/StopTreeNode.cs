namespace Jungle.Nodes
{
    [Node(ViewName = "Stop Tree", Category = "Special", PortNames = new string[0])]
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
