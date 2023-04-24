﻿using System.Collections.Generic;
using Jungle;
using UnityEngine;

namespace Jungle.Nodes
{
    public class PlayAnimationNode : BaseNode
    {
        #region Variables

        [SerializeField]
        private string gameObjectName;

        [SerializeField]
        private string animationName;

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
            animator.enabled = true;
            animator.Play(animationName);
        }

        public override Verdict Execute()
        {
            return new Verdict(true, new List<int> {0});
        }

#if UNITY_EDITOR
        public override string ViewName() => "Play Animation";

        public override string Category() => "Animation";
        
        public override List<string> PortNames => new() {"Playing"};
#endif
    }
}