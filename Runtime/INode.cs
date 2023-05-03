namespace Jungle
{
    public interface INode
    {
        public void Initialize(in object inputValue);
        public bool Execute(out PortCall[] call);
    }
}
