namespace Jungle.Nodes
{
    [NodeProperties(
        Title = "Start", 
        Tooltip = "The first executed node.",
        Category = "Internal:HIDDEN",
        Color = Green
    )]
    [EventNode(
        OutputPortNames = new []{""},
        OutputPortTypes = new []{typeof(None)}
    )]
    public class StartNode : EventNode
    {
        public override void OnStart()
        {
            CallAndStop(new []{new PortCall(0, new None())});
        }
        
        public override void OnUpdate()
        {
            
        }
    }
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(StartNode))]
    public class RootNodeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // I just don't want anything to be drawn to the inspector
        }
    }
#endif
}
