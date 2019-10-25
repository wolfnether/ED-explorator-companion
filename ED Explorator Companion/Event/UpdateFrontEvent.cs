using ED_Explorator_Companion.Event;

namespace ED_Explorator_Companion
{
    internal class UpdateFrontEvent : BaseEvent
    {
        public bool Full;

        public bool Force;

        public UpdateFrontEvent(bool full = false, bool force = false)
        {
            this.Event = "UpdateFrontEvent";
            Full = full;
            Force = force;
        }
    }
}