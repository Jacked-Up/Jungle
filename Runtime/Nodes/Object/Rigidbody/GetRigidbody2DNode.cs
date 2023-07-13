using System;
using UnityEngine;

namespace Jungle.Nodes.Object.Rigidbody
{
    [NodeProperties(
        Title = "Get Rigidbody 2D",
        Category = "Object/Physics",
        Color = Teal,
        InputPortName = "Find",
        InputPortType = typeof(UnityEngine.GameObject),
        OutputPortNames = new []{ "Found" },
        OutputPortTypes = new []{ typeof(Rigidbody2D) }
    )]
    public class GetRigidbody2DNode : JungleNode
    {
        #region Variables
        
        [SerializeField] 
        private bool cacheRigidbody2D;
        
        [NonSerialized] 
        private Rigidbody2D _rigidbody2D;
        
        #endregion
        
        public override void Initialize(in object inputValue)
        {
            var gameObject = inputValue as UnityEngine.GameObject;
            if (gameObject == null)
            {
                return;
            }
            if (cacheRigidbody2D && _rigidbody2D != null)
            {
                return;
            }
            _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();

#if UNITY_EDITOR
            if (_rigidbody2D == null)
            {
                Debug.LogError($"[{name}] Failed to find rigidbody 2D component on " +
                               $"game object by name \"{gameObject.name}\"");
            }
#endif
        }

        public override bool Execute(out PortCall[] call)
        {
            call = Array.Empty<PortCall>();
            if (_rigidbody2D != null)
            {
                call = new[]
                {
                    new PortCall(0, _rigidbody2D)
                };
            }
            return true;
        }
    }
}
