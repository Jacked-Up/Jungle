using UnityEngine;

namespace Jungle.Nodes.Editor
{
    [Node(
        TitleName = "Debug Log",
        Category = "Editor", 
        Color = Color.Grey
    )]
    public class DebugLogNode : Node
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

        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
        {
#if UNITY_EDITOR
            var completeMessage = $"[{tree.name}] {message}";
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
            call = new[]
            {
                new PortCall(0, true)
            };
            return true;
        }
    }
}
