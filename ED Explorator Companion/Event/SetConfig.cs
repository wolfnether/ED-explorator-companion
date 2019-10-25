using ED_Explorator_Companion.Event;

namespace ED_Explorator_Companion
{
    internal class SetConfig : BaseEvent
    {
        public string key;
        public string value;

        public SetConfig(string key, string value)
        {
            this.Event = "SetConfig";
            this.key = key;
            this.value = value;
        }
    }
}