using System;
using System.Collections.Generic;
using UnityEngine;

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
        
        private IdentityNodeAttribute IdentityNodeInfo
            => (IdentityNodeAttribute)GetType().GetCustomAttributes(typeof(IdentityNodeAttribute), true)[0];
        
        public override PortInfo GetInput()
        {
            var portName = IdentityNodeInfo.InputPortName ??= "Execute";
            var portType = typeof(None);
            return new PortInfo(portName, portType);
        }
        
        public override PortInfo[] GetOutputs()
        {
            var portName = IdentityNodeInfo.OutputPortName ??= "Next";
            var portType = typeof(None);
            return new[]
            {
                new PortInfo(portName, portType)
            };
        }
        
        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        public abstract void OnStart();
        
        /// <summary>
        /// 
        /// </summary>
        public abstract void OnUpdate();

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
        
        internal override void OnStartInternal(in object inputValue)
            => OnStart();
        
        internal override void OnUpdateInternal()
            => OnUpdate();
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
        } = "Execute";
        
        /// <summary>
        /// 
        /// </summary>
        public string OutputPortName
        {
            get; 
            set;
        } = "Next";
        
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
