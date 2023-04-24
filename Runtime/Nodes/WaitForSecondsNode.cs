using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes
{
    [Node(ViewName = "Wait For Seconds", Category = "Time", PortNames = new []{"Elapsed"})]
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
    }
}
