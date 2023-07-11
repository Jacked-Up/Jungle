namespace Jungle
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BranchNode : JungleNode, IBranchNode
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract void OnStart(in object inputValue);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract bool OnUpdate(out PortCall[] call);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public interface IBranchNode
    {
        public void OnStart(in object inputValue);
        public bool OnUpdate(out PortCall[] call);
    }
}
