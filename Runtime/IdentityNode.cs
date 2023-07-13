using System;

namespace Jungle
{
    /// <summary>
    /// A Jungle Node type that accepts a value, called an identity, and returns that value when execution is complete.
    /// </summary>
    [Serializable] [IdentityNode]
    public abstract class IdentityNode : JungleNode
    {
        #region Variables
        
        /// <summary>
        /// Reference to the identity.
        /// </summary>
        [NonSerialized]
        internal object Identity;
        
        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        public abstract void OnStart();
        
        /// <summary>
        /// 
        /// </summary>
        public abstract void OnUpdate();
        
        internal override void OnStartInternal(in object inputValue)
            => OnStart();
        
        internal override void OnUpdateInternal()
            => OnUpdate();
        
        /// <summary>
        /// 
        /// </summary>
        protected virtual void Call()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void CallAndStop()
        {
            
        }
    }

    /// <summary>
    /// Identity node attribute. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IdentityNodeAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string InputPortName
        {
            get; 
            set;
        } = "In";
        
        /// <summary>
        /// 
        /// </summary>
        public string OutputPortName
        {
            get; 
            set;
        } = "Out";

        /// <summary>
        /// 
        /// </summary>
        public PortInfo InputInfo => new(InputPortName, typeof(None));
        
        /// <summary>
        /// 
        /// </summary>
        public PortInfo OutputInfo => new(OutputPortName, typeof(None));
    }
}
