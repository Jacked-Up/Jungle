using System;

namespace Jungle.Nodes.Object.GameObject
{
    [NodeProperties(
        Title = "Destroy Game Object",
        Tooltip = "Destroys the game object.",
        Category = "Object",
        Color = Red
    )]
    [BranchNode(
        InputPortName = "Destroy",
        InputPortType = typeof(UnityEngine.GameObject),
        OutputPortNames = new string[0],
        OutputPortTypes = new Type[0]
    )]
    public class DestroyGameObjectNode : BranchNode
    {
        public override void OnStart(in object inputValue)
        {
            var gameObject = inputValue as UnityEngine.GameObject;
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            CallAndStop(Array.Empty<PortCall>());
        }
        
        public override void OnUpdate()
        {
            
        }
    }
}