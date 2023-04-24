using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes
{
    [Node(ViewName = "Stop Animation", Category = "Animation", PortNames = new []{"Stopped"})]
    public class StopAnimationNode : BaseNode
    {
        #region Variables

        [SerializeField]
        private string gameObjectName;

        #endregion

        public override void Initialize()
        {
            var animatorGameObject = GameObject.Find(gameObjectName);
            if (animatorGameObject == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{name}] Could not find game object with name \"{gameObjectName}\"");
#endif
                return;
            }
            var animator = animatorGameObject.GetComponent<Animator>();
            if (animator == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{name}] Could not find animator on game object with name \"{gameObjectName}\"");
#endif
                return;
            }
            animator.enabled = false;
        }

        public override Verdict Execute()
        {
            return new Verdict(true, new List<int> {0});
        }
    }
}