using System;
using UnityEngine;

namespace Jungle.Nodes.Loop
{
    [NodeProperties(
        Title = "For Loop",
        Tooltip = "Invokes for the set iterations.",
        Category = "Loop",
        Color = Purple
    )]
    [BranchNode(
        InputPortName = "Begin",
        InputPortType = typeof(None),
        OutputPortNames = new[] {"Invoke", "Done"},
        OutputPortTypes = new[] {typeof(None), typeof(None)}
    )]
    public class ForLoopNode : BranchNode
    {
        #region Variables

        [SerializeField] 
        private int incrementCount = 5;
        
        [SerializeField]
        private float timeBetweenIncrements = 1f;

        [NonSerialized]
        private int _increment;

        [NonSerialized] 
        private float _nextInvokeTime;
        
        #endregion

        public override void OnStart(in object inputValue)
        {
            _increment = 0;
            _nextInvokeTime = 0f;
        }

        public override void OnUpdate()
        {
            if (UnityEngine.Time.unscaledTime >= _nextInvokeTime && _increment < incrementCount)
            {
                _increment++;
                _nextInvokeTime = UnityEngine.Time.unscaledTime + timeBetweenIncrements;
                Call(new [] {new PortCall(0, true)});
                return;
            }
            if (UnityEngine.Time.unscaledTime < _nextInvokeTime && _increment < incrementCount)
            {
                return;
            }
            CallAndStop(new [] {new PortCall(1, true)});
        }
    }
}
