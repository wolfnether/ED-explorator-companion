using LiteDB;

namespace ED_Explorator_Companion
{
    internal class Body
    {
        public string BodyName { get; set; }

        public string BodyType { get; set; } = null;
        public string TerraformState { get; set; } = null;
        public double Distance { get; set; }
        public bool Scanned { get; set; } = false;
    }
}