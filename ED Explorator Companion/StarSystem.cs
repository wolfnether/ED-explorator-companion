using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ED_Explorator_Companion
{
    internal class StarSystem
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public long SystemId { get; set; }

        public string SystemName { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public int Visites { get; set; } = 0;
        public DateTime LastVisite { get; set; } = DateTime.MinValue;

        [BsonIgnore]
        public int BodiesScanned { get => Bodies.Where(b => b.Scanned).Count(); }

        public bool AllBodiesFound { get; set; } = false;
        public bool Imported { get; set; } = false;
        public int TriedToImport { get; set; } = 0;

        public List<Body> Bodies { get; set; } = new List<Body>();

        public bool NearStarImported { get; set; } = false;

        public int BodiesCount { get; set; } = 0;

        [BsonIgnore]
        public double Distance { get => DistanceFromCurrent(); }

        [BsonIgnore]
        public string DistanceStr { get => String.Format("{0:F2}", Distance) + "ly"; }

        [BsonIgnore]
        public string InterrestingBodies { get => GetInterrestingBodies(); }

        public double DistanceFrom(StarSystem s) => Math.Sqrt(Math.Pow(s.X - X, 2) + Math.Pow(s.Y - Y, 2) + Math.Pow(s.Z - Z, 2));

        public double DistanceFromCurrent()
        {
            return DistanceFrom(Database.CurrentStarSystem);
        }

        internal string GetInterrestingBodies()
        {
            string main = "";
            string secondaries = "";
            string planets = "";

            if (Bodies.Count != 0)
                main = "*";

            foreach (var body in Bodies)
            {
                switch (body.BodyType)
                {
                    //is high value ?
                    case "N": //Neutron Star
                        if (body.Distance == 0.0d)
                            main = "N";
                        else if (body.Distance <= 1000.0d)
                            secondaries += "n̄";
                        else
                            secondaries += "n";
                        break;

                    case "D": //White dwarf
                    case "DA":
                    case "DAB":
                    case "DAO":
                    case "DAZ":
                    case "DAV":
                    case "DB":
                    case "DBZ":
                    case "DBV":
                    case "DQ":
                    case "DC":
                    case "DCV":
                    case "DX":
                        if (body.Distance == 0.0d)
                            main = "D";
                        else if (body.Distance <= 1000.0d)
                            secondaries += "d̄";
                        else
                            secondaries += "d";
                        break;

                    case "H": // Black hole
                    case "SupermassiveBlackHole":
                        if (body.Distance == 0.0d)
                            main = "H";
                        else
                            secondaries += "h";
                        break;
                    //Is scoopable ?
                    case "O":
                    case "A":
                    case "A_BlueWhiteSuperGiant":
                    case "B":
                    case "B_BlueWhiteSuperGiant":
                    case "F":
                    case "F_WhiteSuperGiant":
                    case "G":
                    case "G_WhiteSuperGiant":
                    case "K":
                    case "K_OrangeGiant":
                    case "M":
                    case "M_RedGiant":
                    case "M_RedSuperGiant":
                        if (body.Distance == 0.0d)
                            main = "S";
                        else if (body.Distance <= 1000.0d && !secondaries.Contains("s"))
                            secondaries += "s";
                        break;

                    case "High metal content body":
                    case "Metal rich body":
                        if (body.TerraformState == "Terraformable")
                            planets += "M";
                        //else
                        //    planets += "m";
                        break;

                    case "Rocky body":
                        if (body.TerraformState == "Terraformable")
                            planets += "R";
                        break;

                    case "Earthlike body":
                        if (body.TerraformState == "Terraformable")
                            planets += "E";
                        else
                            planets += "e";
                        break;

                    case "Water world":
                        if (body.TerraformState == "Terraformable")
                            planets += "W";
                        else
                            planets += "w";
                        break;

                    case "Ammonia world":
                        if (body.TerraformState == "Terraformable")
                            planets += "A";
                        else
                            planets += "a";
                        break;

                    case "Sudarsky class II gas giant":
                        planets += "2";
                        break;

                    default:
                        if (body.Distance == 0.0d)
                            main = "*";
                        break;
                }
            }

            return (main + secondaries + "-" + planets).Trim();
        }
    }
}