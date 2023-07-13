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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputValue"></param>
        public abstract void OnStart(in object inputValue);
        
        /// <summary>
        /// 
        /// </summary>
        public abstract void OnUpdate();
        
        internal override void OnStartInternal(in object inputValue)
            => OnStart(inputValue);
        
        internal override void OnUpdateInternal()
            => OnUpdate();
        
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
        
        /// <summary>
        /// Creates data container with information about all this event nodes output ports.
        /// </summary>
        public PortInfo[] GetOutputPortInfo()
        {
            var query = new List<PortInfo>();
            
            if (OutputPortNames.Length != OutputPortTypes.Length)
            {
                // More names declared than types
                if (OutputPortNames.Length > OutputPortTypes.Length)
                {
#if UNITY_EDITOR
                    Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null,
                        "[Jungle] An event node has more output port names declared than output port types.");
#endif
                    for (var i = 0; i < OutputPortNames.Length; i++)
                    {
                        var name = OutputPortNames[i];
                        var type = OutputPortTypes.Length - 1 > i
                            ? OutputPortTypes[i]
                            : typeof(Unknown);
                        query.Add(new PortInfo(name, type));
                    }
                }
                // More types declared than names
                else
                {
#if UNITY_EDITOR
                    Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null,
                        "[Jungle] An event node has more output port types declared than output port names.");
#endif
                    for (var i = 0; i < OutputPortTypes.Length; i++)
                    {
                        var name = OutputPortNames.Length - 1 > i
                            ? OutputPortNames[i]
                            : "Unnamed Port";
                        var type = OutputPortTypes[i];
                        query.Add(new PortInfo(name, type));
                    }
                }
                return query.ToArray();
            }
            
            for (var i = 0; i < OutputPortNames.Length; i++)
            {
                var name = OutputPortNames[i];
                var type = OutputPortTypes[i];
                query.Add(new PortInfo(name, type));
            }
            return query.ToArray();
        }
    }
}
