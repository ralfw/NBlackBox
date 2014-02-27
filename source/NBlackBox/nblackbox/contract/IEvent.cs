namespace nblackbox.contract
{
    public interface IEvent
    {
        string Name { get; }
        string Context { get; }
        string Data { get; }
    }


    public class Event : IEvent
    {
        public Event(string name, string context, string data)
        {
            Data = data;
            Name = name;
            Context = context;
        }

        public string Name { get; private set; }
        public string Context { get; private set; }
        public string Data { get; private set; }
    }
}