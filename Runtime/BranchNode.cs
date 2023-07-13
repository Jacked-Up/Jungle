using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jungle
{
    /// <summary>
    /// A Jungle Node type that accepts a value and can return anything, called a port call.
    /// </summary>
    [Serializable] [BranchNode]
    public abstract class BranchNode : JungleNode
    {
        #region Variables

        private BranchNodeAttribute BranchNodeInfo
            => (BranchNodeAttribute)GetType().GetCustomAttributes(typeof(NodePropertiesAttribute), true)[0];

        public override PortInfo GetInput()
        {
            var portName = BranchNodeInfo.InputPortName ??= "Execute";
            var portType = BranchNodeInfo.InputPortType ??= typeof(None);
            return new PortInfo(portName, portType);
        }
        
        public override PortInfo[] GetOutputs()
        {
            var portNames = BranchNodeInfo.OutputPortNames ??= new []{"Next"};
            var portTypes = BranchNodeInfo.OutputPortTypes ??= new []{typeof(None)};
            var query = new List<PortInfo>();
            
            if (portNames.Length != portTypes.Length)
            {
                // More names declared than types
                if (portNames.Length > portTypes.Length)
                {
#if UNITY_EDITOR
                    Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, this,
                        $"[Jungle] {GetTitle()} has more output port names declared than output port types.");
#endif
                    for (var i = 0; i < portNames.Length; i++)
                    {
                        var portName = portNames[i];
                        var portType = portTypes.Length - 1 > i
                            ? portTypes[i]
                            : typeof(Unknown);
                        query.Add(new PortInfo(portName, portType));
                    }
                }
                // More types declared than names
                else
                {
#if UNITY_EDITOR
                    Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, this,
                        $"[Jungle] {GetTitle()} has more output port types declared than output port names.");
#endif
                    for (var i = 0; i < portTypes.Length; i++)
                    {
                        var portName = portNames.Length - 1 > i
                            ? portNames[i]
                            : "Unnamed Port";
                        var portType = portTypes[i];
                        query.Add(new PortInfo(portName, portType));
                    }
                }
                return query.ToArray();
            }
            
            for (var i = 0; i < portNames.Length; i++)
            {
                var portName = portNames[i];
                var portType = portTypes[i];
                query.Add(new PortInfo(portName, portType));
            }
            return query.ToArray();
        }
        
        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        public abstract void OnStart(in object inputValue);
        
        /// <summary>
        /// 
        /// </summary>
        public abstract void OnUpdate();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portCalls"></param>
        protected virtual void Call(PortCall[] portCalls)
            => Tree.Call(this, portCalls);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portCalls"></param>
        protected virtual void CallAndStop(PortCall[] portCalls)
            => Tree.CallAndStop(this, portCalls);
        
        internal override void OnStartInternal(in object inputValue)
            => OnStart(inputValue);
        
        internal override void OnUpdateInternal()
            => OnUpdate();
    }
    
    /// <summary>
    /// Branch node attribute. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BranchNodeAttribute : Attribute
    {
        /// <summary>
        /// Name of the input port.
        /// </summary>
        public string InputPortName
        {
            get;
            set;
        } = "Execute";
        
        /// <summary>
        /// Input port value type.
        /// </summary>
        public Type InputPortType
        {
            get;
            set;
        } = typeof(None);
        
        /// <summary>
        /// Names to correspond with each output port.
        /// </summary>
        public string[] OutputPortNames
        {
            get;
            set;
        } = {"Next"};
        
        /// <summary>
        /// Output value types to correspond with each output port.
        /// </summary>
        public Type[] OutputPortTypes
        {
            get;
            set;
        } = {typeof(None)};
    }
}
