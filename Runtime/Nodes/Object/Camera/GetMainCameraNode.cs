using System;
using UnityEngine;

namespace Jungle.Nodes.Object.Camera
{
    [Node(
        Title = "Get Main Camera",
        Group = "Object/Camera",
        Color = JungleNodeColors.Teal,
        InputPortName = "Get",
        InputPortType = typeof(None),
        OutputPortNames = new []{ "Found" },
        OutputPortTypes = new []{ typeof(UnityEngine.Camera) }
    )]
    public class GetMainCameraNode : JungleNode
    {
        #region Variables
        
        [SerializeField] 
        private bool cacheCamera;
        
        [NonSerialized] 
        private UnityEngine.Camera _camera;
        
        #endregion
        
        public override void Initialize(in object inputValue)
        {
            if (cacheCamera && _camera != null)
            {
                return;
            }
            _camera = UnityEngine.Camera.main;

#if UNITY_EDITOR
            if (_camera == null)
            {
                Debug.LogError($"[{name}] Failed to find main camera.");
            }
#endif
        }

        public override bool Execute(out PortCall[] call)
        {
            call = Array.Empty<PortCall>();
            if (_camera != null)
            {
                call = new[]
                {
                    new PortCall(0, _camera)
                };
            }
            return true;
        }
    }
}
