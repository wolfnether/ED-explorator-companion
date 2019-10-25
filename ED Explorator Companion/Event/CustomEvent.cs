using ED_Explorator_Companion.Event;

namespace ED_Explorator_Companion
{
    internal class CustomEvent : BaseEvent
    {
        public CustomEvent(string eventName)
        {
            Event = eventName;
        }
    }
}