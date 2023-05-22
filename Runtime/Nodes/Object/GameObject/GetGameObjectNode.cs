using System;
using UnityEngine;

namespace Jungle.Nodes.Object.GameObject
{
    [Node(
        TitleName = "Get Game Object",
        Category = "Object",
        Color = Color.Teal,
        InputPortName = "Find",
        OutputPortNames = new []{ "Found" },
        OutputPortTypes = new []{ typeof(UnityEngine.GameObject) }
    )]
    public class GetGameObjectNode : JungleNode
    {
        #region Variables

        [SerializeField] 
        private string gameObjectName;

        [SerializeField] 
        private bool cacheGameObject;
        
        [NonSerialized] 
        private UnityEngine.GameObject _gameObject;
        
        #endregion
        
        public override void Initialize(in object inputValue)
        {
            if (cacheGameObject && _gameObject != null)
            {
                return;
            }
            _gameObject = UnityEngine.GameObject.Find(gameObjectName);
            
#if UNITY_EDITOR
            if (_gameObject == null)
            {
                Debug.LogError($"[{name}] Failed to find game object by name \"{gameObjectName}\"");
            }
#endif
        }

        public override bool Execute(out PortCall[] call)
        {
            call = new[]
            {
                new PortCall(0, _gameObject)
            };
            return true;
        }
    }
}
