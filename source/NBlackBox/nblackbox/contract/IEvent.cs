namespace nblackbox.contract
{
    public interface IEvent
    {
        string Name { get; }
        string Context { get; }
        string Data { get; }
    }
}