using System;
using UnityEngine;

namespace Jungle.Nodes.Object.Camera
{
    [Node(
        Title = "Get Camera",
        Group = "Object/Camera",
        Color = JungleNodeColors.Teal,
        InputPortName = "Find",
        InputPortType = typeof(UnityEngine.GameObject),
        OutputPortNames = new []{ "Found" },
        OutputPortTypes = new []{ typeof(UnityEngine.Camera) }
    )]
    public class GetCameraNode : JungleNode
    {
        #region Variables
        
        [SerializeField] 
        private bool cacheCamera;
        
        [NonSerialized] 
        private UnityEngine.Camera _camera;
        
        #endregion
        
        public override void Initialize(in object inputValue)
        {
            var gameObject = inputValue as UnityEngine.GameObject;
            if (gameObject == null)
            {
                return;
            }
            if (cacheCamera && _camera != null)
            {
                return;
            }
            _camera = gameObject.GetComponent<UnityEngine.Camera>();

#if UNITY_EDITOR
            if (_camera == null)
            {
                Debug.LogError($"[{name}] Failed to find camera component on game object by name \"{gameObject.name}\"");
            }
#endif
        }

        public override bool Execute(out PortCall[] call)
        {
            call = new[]
            {
                new PortCall(0, _camera)
            };
            return true;
        }
    }
}