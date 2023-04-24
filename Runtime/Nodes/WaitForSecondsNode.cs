using System.Collections.Generic;
using Jungle;
using UnityEngine;

namespace Jungle.Nodes
{
    public class WaitForSecondsNode : BaseNode
    {
        #region Variables

        [SerializeField] 
        private float duration = 1f;

        [SerializeField]
        private bool scaledTime = true;

        private float _startTime;

        #endregion

        public override void Initialize()
        {
            _startTime = scaledTime
                ? Time.time
                : Time.unscaledTime;
        }

        public override Verdict Execute()
        {
            var currentTime = scaledTime
                ? Time.time
                : Time.unscaledTime;

            if (currentTime - _startTime >= duration)
            {
                return new Verdict(true, new List<int> { 0 });
            }
            return new Verdict(false);
        }
        
#if UNITY_EDITOR

        public override string ViewName() => "Wait For Seconds";

        public override string Category() => "Time";
        
        public override List<string> PortNames => new() { "Elapsed" };
        
#endif
    }
}
