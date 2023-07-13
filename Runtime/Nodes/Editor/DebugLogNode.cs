using UnityEngine;

namespace Jungle.Nodes.Editor
{
    [NodeProperties(
        Title = "Debug Log",
        Category = "Editor", 
        Color = White
    )]
    [IdentityNode(
        InputPortName = "Write"
    )]
    public class DebugLogNode : IdentityNode
    {
        #region Variables

        [SerializeField] [TextArea]
        private string message = "I am a debug message";

        [SerializeField] 
        private Type type = Type.Log;
        
        private enum Type
        {
            Log,
            Warning,
            Error
        }

        #endregion

        public override void OnStart()
        {
#if UNITY_EDITOR
            var completeMessage = $"[{Tree.name}] {message}";
            switch (type)
            {
                case Type.Log:
                    Debug.Log(completeMessage);
                    break;
                case Type.Warning:
                    Debug.LogWarning(completeMessage);
                    break;
                case Type.Error:
                    Debug.LogError(completeMessage);
                    break;
            }
#endif
            CallAndStop();
        }

        public override void OnUpdate()
        {
            
        }
    }
}
