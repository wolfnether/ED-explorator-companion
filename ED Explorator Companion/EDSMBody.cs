namespace ED_Explorator_Companion
{
    internal class EDSMBody
    {
        public string name;
        public string subType;

        public double distanceToArrival;
        public string terraformingState;

        public string getEDsubType()
        {
            if (subType.Equals("O (Blue-White) Star"))
                return "O";
            else if (subType.Equals("B (Blue-White) Star"))
                return "B";
            else if (subType.Equals("B (Blue-White super giant) Star"))
                return "B_BlueWhiteSuperGiant";
            else if (subType.Equals("A (Blue-White) Star"))
                return "A";
            else if (subType.Equals("A (Blue-White super giant) Star"))
                return "A_BlueWhiteSuperGiant";
            else if (subType.Equals("F (White) Star"))
                return "F";
            else if (subType.Equals("F (White super giant) Star"))
                return "F_WhiteSuperGiant";
            else if (subType.Equals("G (White-Yellow) Star"))
                return "G";
            else if (subType.Equals("G (White-Yellow super giant) Star"))
                return "G_WhiteSuperGiant";
            else if (subType.Equals("K (Yellow-Orange) Star"))
                return "K";
            else if (subType.Equals("K (Yellow-Orange giant) Star"))
                return "K_OrangeGiant";
            else if (subType.Equals("M (Red dwarf) Star"))
                return "M";
            else if (subType.Equals("M (Red giant) Star"))
                return "M_RedGiant";
            else if (subType.Equals("M (Red super giant) Star"))
                return "M_RedSuperGiant";
            else if (subType.Equals("L (Brown dwarf) Star"))
                return "L";
            else if (subType.Equals("T (Brown dwarf) Star"))
                return "T";
            else if (subType.Equals("Y (Brown dwarf) Star"))
                return "Y";
            else if (subType.Equals("T Tauri Star"))
                return "TTS";
            else if (subType.Equals("Herbig Ae/Be Star"))
                return "AeBe";
            else if (subType.Equals("Wolf-Rayet Star"))
                return "W";
            else if (subType.Equals("Wolf-Rayet N Star"))
                return "WN";
            else if (subType.Equals("Wolf-Rayet NC Star"))
                return "WNC";
            else if (subType.Equals("Wolf-Rayet C Star"))
                return "WC";
            else if (subType.Equals("Wolf-Rayet O Star"))
                return "WO";
            else if (subType.Equals("CS Star"))
                return "CS";
            else if (subType.Equals("C Star"))
                return "C";
            else if (subType.Equals("CN Star"))
                return "CN";
            else if (subType.Equals("CJ Star"))
                return "CJ";
            else if (subType.Equals("CH Star"))
                return "CH";
            else if (subType.Equals("CHd Star"))
                return "CHd";
            else if (subType.Equals("MS-type Star"))
                return "MS";
            else if (subType.Equals("S-type Star"))
                return "S";
            else if (subType.Equals("White Dwarf (D) Star"))
                return "D";
            else if (subType.Equals("White Dwarf (DA) Star"))
                return "DA";
            else if (subType.Equals("White Dwarf (DAB) Star"))
                return "DAB";
            else if (subType.Equals("White Dwarf (DAO) Star"))
                return "DAO";
            else if (subType.Equals("White Dwarf (DAZ) Star"))
                return "DAZ";
            else if (subType.Equals("White Dwarf (DAV) Star"))
                return "DAV";
            else if (subType.Equals("White Dwarf (DB) Star"))
                return "DB";
            else if (subType.Equals("White Dwarf (DBZ) Star"))
                return "DBZ";
            else if (subType.Equals("White Dwarf (DBV) Star"))
                return "DBV";
            else if (subType.Equals("White Dwarf (DO) Star"))
                return "DO";
            else if (subType.Equals("White Dwarf (DOV) Star"))
                return "DOV";
            else if (subType.Equals("White Dwarf (DQ) Star"))
                return "DQ";
            else if (subType.Equals("White Dwarf (DC) Star"))
                return "DC";
            else if (subType.Equals("White Dwarf (DCV) Star"))
                return "DCV";
            else if (subType.Equals("White Dwarf (DX) Star"))
                return "DX";
            else if (subType.Equals("Neutron Star"))
                return "N";
            else if (subType.Equals("Black Hole"))
                return "H";
            else if (subType.Equals("Supermassive Black Hole"))
                return "SupermassiveBlackHole";
            else if (subType.Equals("High metal content world"))
                return "High metal content body";
            else if (subType.Equals("Metal-rich body"))
                return "Metal rich body";
            else if (subType.Equals("Earth-like world"))
                return "Earthlike body";
            else if (subType.Equals("Class II gas giant"))
                return "Sudarsky class II gas giant";
            return subType;
        }
    }
}