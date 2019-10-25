namespace ED_Explorator_Companion.Event
{
    internal class SystemEvent : BaseEvent
    {
        public string SystemName { get; set; }
        public string StarSystem { get; set; }
        public long SystemAddress { get; set; }

        public SystemEvent()
        {
            Event = "SystemEvent";
        }
    }
}