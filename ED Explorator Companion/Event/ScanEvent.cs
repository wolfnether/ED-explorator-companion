namespace ED_Explorator_Companion.Event
{
    internal class ScanEvent : SystemEvent
    {
        public string BodyName;
        public double DistanceFromArrivalLS;
        public string StarType;
        public string TerraformState;
        public string PlanetClass;
    }
}