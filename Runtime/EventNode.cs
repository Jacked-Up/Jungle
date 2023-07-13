using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jungle
{
    /// <summary>
    /// A Jungle Node type that listens for specific events during runtime and spits out values when said event occurs.
    /// </summary>
    [Serializable] [EventNode]
    public abstract class EventNode : JungleNode
    {
        #region Variables

        private EventNodeAttribute EventNodeInfo
            => (EventNodeAttribute)GetType().GetCustomAttributes(typeof(EventNodeAttribute), true)[0];

        public override PortInfo GetInput()
        {
            return new PortInfo();
        }
        
        public override PortInfo[] GetOutputs()
        {
            var portNames = EventNodeInfo.OutputPortNames ??= new []{"Next"};
            var portTypes = EventNodeInfo.OutputPortTypes ??= new []{typeof(None)};
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
        public abstract void OnStart();
        
        /// <summary>
        /// 
        /// </summary>
        public abstract void OnUpdate();

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
        
        internal override void OnStartInternal(in object inputValue)
            => OnStart();
        
        internal override void OnUpdateInternal()
            => OnUpdate();
    }

    /// <summary>
    /// Event node attribute. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EventNodeAttribute : Attribute
    {
        /// <summary>
        /// Names to correspond with each output port.
        /// </summary>
        public string[] OutputPortNames
        {
            get;
            set;
        } = {"Call"};
        
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
