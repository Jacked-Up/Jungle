using System;
using UnityEngine;

namespace Jungle.Nodes.Object.Rigidbody
{
    [Node(
        Title = "Get Rigidbody",
        Group = "Object/Physics",
        Color = JungleNodeColors.Teal,
        InputPortName = "Find",
        InputPortType = typeof(UnityEngine.GameObject),
        OutputPortNames = new []{ "Found" },
        OutputPortTypes = new []{ typeof(UnityEngine.Rigidbody) }
    )]
    public class GetRigidbodyNode : JungleNode
    {
        #region Variables
        
        [SerializeField] 
        private bool cacheRigidbody;
        
        [NonSerialized] 
        private UnityEngine.Rigidbody _rigidbody;
        
        #endregion
        
        public override void Initialize(in object inputValue)
        {
            var gameObject = inputValue as UnityEngine.GameObject;
            if (gameObject == null)
            {
                return;
            }
            if (cacheRigidbody && _rigidbody != null)
            {
                return;
            }
            _rigidbody = gameObject.GetComponent<UnityEngine.Rigidbody>();

#if UNITY_EDITOR
            if (_rigidbody == null)
            {
                Debug.LogError($"[{name}] Failed to find rigidbody component on " +
                               $"game object by name \"{gameObject.name}\"");
            }
#endif
        }

        public override bool Execute(out PortCall[] call)
        {
            call = Array.Empty<PortCall>();
            if (_rigidbody != null)
            {
                call = new[]
                {
                    new PortCall(0, _rigidbody)
                };
            }
            return true;
        }
    }
}
