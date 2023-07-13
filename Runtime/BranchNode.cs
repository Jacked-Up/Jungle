using System;

namespace Jungle
{
    /// <summary>
    /// A Jungle Node type that accepts a value and can return anything, called a port call.
    /// </summary>
    [Serializable]
    public abstract class BranchNode : JungleNode
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        protected virtual void Call(PortCall[] call)
        {
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        protected virtual void CallAndStop(PortCall[] call)
        {
            
        }
    }
}
