namespace TD.Services.Cache
{
    public enum EventType
    {
        Button,
        MessageEdit,
        WaitForConfrim
    }
    public class Event
    {
        public ulong MessageId { get; set; }
        public string EventName { get; set; } = "";
        public EventType EventType { get; set; }
        public DateTime CreatedAt { get; set; }
        public event Action OnRaise = delegate { };
        public bool DisposeMessage { get; set; } = true;
        public void ClearListeners()
        {
            OnRaise = () => { };
        }
        public void Raise()
        {
            OnRaise();
        }
    }
    public class Publisher
    {
        public List<Event> Events = new List<Event>();

        public void ClearEvent(Event e)
        {
            e.ClearListeners();
            Events.Remove(e);
        }
    }
}
