namespace ED_Explorator_Companion.Event
{
    internal class AddEdsmSystemEvent : BaseEvent
    {
        public EDSMSystem sys;

        public AddEdsmSystemEvent()
        {
            Event = "AddEdsmSystemEvent";
        }
    }
}