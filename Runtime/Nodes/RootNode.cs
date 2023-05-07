namespace Jungle.Nodes
{
    [Node(TitleName = "Start", 
        Category = "HIDDEN", 
        Color = NodeAttribute.NodeColor.Green,
        OutputPortNames = new []{"Begin"}
    )]
    public class RootNode : Node
    {
        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
        {
            call = new[] {new PortCall(0, new Nothing())};
            return true;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(RootNode))]
    public class RootNodeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI() { }
    }
#endif
}
