using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Nodes.Editor
{
    [Node(ViewName = "Debug", Category = "Editor", NodeColor = NodeColor.Grey, InputPortName = "Execute", OutputPortNames = new []{"Next"})]
    public class DebugNode : BaseNode
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

        public override void Initialize() {}

        public override Verdict Execute()
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
            return new Verdict(true, new List<int> { 0 });
        }
    }
}
