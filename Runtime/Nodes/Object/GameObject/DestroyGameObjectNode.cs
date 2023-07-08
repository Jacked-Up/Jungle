using System;

namespace Jungle.Nodes.Object.GameObject
{
    [Node(
        Title = "Destroy Game Object",
        Group = "Object",
        Color = JungleNodeColors.Red,
        InputPortName = "Destroy",
        InputPortType = typeof(UnityEngine.GameObject),
        OutputPortNames = new string[0],
        OutputPortTypes = new Type[0]
    )]
    public class DestroyGameObjectNode : JungleNode
    {
        #region Variables

        private UnityEngine.GameObject gameObject;

        #endregion
        
        public override void Initialize(in object inputValue)
        {
            gameObject = inputValue as UnityEngine.GameObject;
        }

        public override bool Execute(out PortCall[] call)
        {
            Destroy(gameObject);
            call = Array.Empty<PortCall>();
            return true;
        }
    }
}