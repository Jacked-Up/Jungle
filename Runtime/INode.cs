namespace Jungle
{
    public interface INode
    {
        public void Start(in object inputValue);
        public bool Update(out PortCall[] call);
    }
}
