﻿namespace Jungle.Nodes
{
    [Node(TitleName = "Start", 
        Tooltip = "The first executed node.",
        Category = "HIDDEN", 
        Color = Color.Green,
        OutputPortNames = new []{ "" },
        OutputPortTypes = new []{ typeof(None) }
    )]
    public class StartNode : JungleNode
    {
        public override void Initialize(in object inputValue)
        {
            
        }
        
        public override bool Execute(out PortCall[] call)
        {
            call = new[]
            {
                new PortCall(0, new None())
            };
            return true;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(StartNode))]
    public class RootNodeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI() { }
    }
#endif
}
