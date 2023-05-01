namespace Jungle
{
    [Node(TitleName = "Start", 
        Category = "HIDDEN", 
        Color = NodeAttribute.NodeColor.Green,
        OutputPortNames = new []{"Begin"}
    )]
    public class RootNode : Node
    {
        public override void Start(in object inputValue)
        {
            
        }

        public override bool Update(out PortCall[] call)
        {
            call = new[] {new PortCall(0, new Nothing())};
            return true;
        }
    }
}
