using System;
using UnityEngine;

namespace Jungle.Nodes.Loop
{
    [Node(Title = "For Loop",
        Category = "Loop",
        Color = Color.Purple,
        OutputPortNames = new []{"Invoke", "Done"}
    )]
    public class ForLoopNode : JungleNode
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

        public override void Initialize(in object inputValue)
        {
            _increment = 0;
            _nextInvokeTime = 0f;
        }

        public override bool Execute(out PortCall[] call)
        {
            if (UnityEngine.Time.unscaledTime >= _nextInvokeTime && _increment < incrementCount)
            {
                _increment++;
                _nextInvokeTime = UnityEngine.Time.unscaledTime + timeBetweenIncrements;
                call = new [] {new PortCall(0, true)};
                return false;
            }
            if (UnityEngine.Time.unscaledTime < _nextInvokeTime && _increment < incrementCount)
            {
                call = Array.Empty<PortCall>();
                return false;
            }
            call = new [] {new PortCall(1, true)};
            return true;
        }
    }
}
