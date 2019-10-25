namespace ED_Explorator_Companion
{
    internal class EDSMSystem
    {
        public string name { get; set; }
        public long id64 { get; set; }
        public EDSMSystemCoords coords { get; set; }
    }

    internal class EDSMSystemCoords
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }
}