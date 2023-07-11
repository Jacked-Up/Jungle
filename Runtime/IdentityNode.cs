using System;

namespace Jungle
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class IdentityNode : JungleNode, IIdentityNode
    {
        #region Variables
        
        /// <summary>
        /// 
        /// </summary>
        [NonSerialized]
        public object Identity;

        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        public abstract void OnStart();
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract bool OnUpdate();
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IIdentityNode
    {
        public void OnStart();
        public bool OnUpdate();
    }
}
