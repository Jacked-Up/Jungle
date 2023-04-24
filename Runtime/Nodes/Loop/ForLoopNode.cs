using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes.Loop
{
    [Node(ViewName = "For Loop", Category = "Loop", NodeColor = NodeColor.Purple, OutputPortNames = new []{"Invoke", "Done"})]
    public class ForLoopNode : BaseNode
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

        public override void Initialize()
        {
            _increment = 0;
            _nextInvokeTime = 0f;
        }
        
        public override Verdict Execute()
        {
            if (Time.unscaledTime >= _nextInvokeTime && _increment < incrementCount)
            {
                _increment++;
                _nextInvokeTime = Time.unscaledTime + timeBetweenIncrements;
                return new Verdict(false, new List<int>{0});
            }
            if (Time.unscaledTime < _nextInvokeTime && _increment < incrementCount)
            {
                return new Verdict(false);
            }
            return new Verdict(true, new List<int>{1});
        }
    }
}
