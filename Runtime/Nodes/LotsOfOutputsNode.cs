namespace Jungle.Nodes
{
    [Node(TitleName = "Lots Of Outputs", 
        OutputPortNames = new []{ "One", "Two", "Three" },
        OutputPortTypes = new []{ typeof(bool), typeof(bool), typeof(bool) }
    )]
    public class LotsOfOutputsNode : Node
    {
        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
        {
            call = new[]
            {
                new PortCall(0, true),
                new PortCall(1, true),
                new PortCall(2, true)
            };
            return true;
        }
    }
}