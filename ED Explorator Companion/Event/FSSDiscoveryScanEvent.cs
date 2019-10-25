namespace ED_Explorator_Companion.Event
{
    internal class FSSDiscoveryScanEvent : SystemEvent
    {
        public int BodyCount { get; set; }
        public int NonBodyCount { get; set; }
        public double Progress { get; set; }
    }
}