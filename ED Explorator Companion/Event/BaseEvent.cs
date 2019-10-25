using System;

namespace ED_Explorator_Companion.Event
{
    internal class BaseEvent
    {
        public string Event { get; set; }
        public DateTime timestamp { get; set; }

        public BaseEvent()
        {
            Event = "BaseEvent";
        }
    }
}