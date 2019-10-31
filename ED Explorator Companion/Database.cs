using ED_Explorator_Companion.Event;
using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ED_Explorator_Companion
{
    internal class Database
    {
        public static ConcurrentQueue<BaseEvent> Queue = new ConcurrentQueue<BaseEvent>();
        //public static object lockingObject = new object();

        public static StarSystem CurrentStarSystem;
        internal static Mutex mutex = new Mutex();

        public static LiteDatabase db { get; } = new LiteDatabase(@"data.db");

        public static bool StopUpdatingGui { get; private set; } = false;

        public static ConfigPair GetConfig(string key) => db.GetCollection<ConfigPair>().FindOne(v => v.Key == key);

        internal static void Init()
        {
            db.GetCollection<StarSystem>().EnsureIndex(x => x.SystemId, true);
            db.GetCollection<StarSystem>().EnsureIndex(x => x.SystemName, false);
            db.GetCollection<StarSystem>().EnsureIndex("body_name", "$.Bodies[*].BodyName");
        }

        public static StarSystem GetStarSystem(long systemid)
        {
            mutex.WaitOne();
            StarSystem starSystem = db.GetCollection<StarSystem>().FindOne(s => s.SystemId == systemid);
            mutex.ReleaseMutex();
            return starSystem;
        }

        public static StarSystem GetStarSystem(string systemName)
        {
            mutex.WaitOne();
            StarSystem starSystem = db.GetCollection<StarSystem>().FindOne(s => s.SystemName == systemName);
            mutex.ReleaseMutex();
            return starSystem;
        }

        private static Body GetBody(string bodyName)
        {
            mutex.WaitOne();
            var sys = db.GetCollection<StarSystem>().IncludeAll().FindOne(s => s.Bodies[0].BodyName == bodyName);
            mutex.ReleaseMutex();
            if (sys == null) return null;
            return sys.Bodies.Find(b => b.BodyName == bodyName);
        }

        public static StarSystem GetCurrentStarSystem()
        {
            mutex.WaitOne();
            var currentSysConfig = GetConfig("CurrentSystem");
            StarSystem sys;
            if (currentSysConfig == null)
            {
                sys = GetStarSystem(10477373803);
                if (sys == null) sys = AddStarSystem(EDSMAPI.GetSystem("Sol"));
                mutex.ReleaseMutex();
                return sys;
            }
            sys = GetStarSystem(currentSysConfig.Value);
            mutex.ReleaseMutex();
            if (sys == null) return AddStarSystem(EDSMAPI.GetSystem(currentSysConfig.Value));
            return sys;
        }

        public static void Run()
        {
            bool DBUpdate = false;
            while (true)
            {
                Program.mainForm.UpdateStatus(Queue.Count + " Event To Process");
                if (Queue.IsEmpty)
                {
                    Thread.Sleep(500);
                    continue;
                }
                Queue.TryDequeue(out var e);
                Program.mainForm.UpdateStatus(Queue.Count + " Event To Process ( " + e.Event + " )");
                StarSystem sys;
                switch (e.Event)
                {
                    case "StopUpdatingGuiOn":
                        StopUpdatingGui = true;
                        break;

                    case "StopUpdatingGuiOff":
                        StopUpdatingGui = false;
                        break;

                    case "UpdateFrontEvent":
                        if (!DBUpdate && !((UpdateFrontEvent)e).Force) break;

                        var currentSys = CurrentStarSystem = GetCurrentStarSystem();
                        if (((UpdateFrontEvent)e).Full)
                        {
                            mutex.WaitOne();
                            var nearSystems = db.GetCollection<StarSystem>()
                            .Find(s => (s.X - currentSys.X) * (s.X - currentSys.X) + (s.Y - currentSys.Y) * (s.Y - currentSys.Y) + (s.Z - currentSys.Z) * (s.Z - currentSys.Z) <= 2500)
                            .OrderBy(s => (s.X - currentSys.X) * (s.X - currentSys.X) + (s.Y - currentSys.Y) * (s.Y - currentSys.Y) + (s.Z - currentSys.Z) * (s.Z - currentSys.Z))
                            .ToList();
                            mutex.ReleaseMutex();
                            Program.mainForm.updateGrid(currentSys, nearSystems);
                        }
                        else
                        {
                            Program.mainForm.UpdateFirstLine();
                        }
                        break;

                    case "FSDTarget":
                        var sysName = ((FSDTargetEvent)e).Name;
                        var targetSys = GetStarSystem(((FSDTargetEvent)e).SystemAddress);
                        if (targetSys == null)
                        {
                            Program.mainForm.UpdateStatus(((FSDTargetEvent)e).Name + " probably not scanned");
                            Thread.Sleep(1000);
                        }
                        break;

                    case "SetConfig":
                        var cp = GetConfig(((SetConfig)e).key);
                        if (cp == null)
                            db.GetCollection<ConfigPair>().Insert(new ConfigPair { Key = ((SetConfig)e).key, Value = ((SetConfig)e).value.ToString() });
                        else
                        {
                            cp.Value = ((SetConfig)e).value.ToString();
                            db.GetCollection<ConfigPair>().Update(cp);
                        }

                        break;

                    case "FSDJump":
                        sys = GetStarSystem(((FSDJumpEvent)e).SystemAddress);
                        if (sys == null)
                        {
                            sys = new StarSystem();
                            sys.SystemId = ((FSDJumpEvent)e).SystemAddress;
                            sys.SystemName = ((FSDJumpEvent)e).StarSystem;
                            sys.X = ((FSDJumpEvent)e).StarPos[0];
                            sys.Y = ((FSDJumpEvent)e).StarPos[1];
                            sys.Z = ((FSDJumpEvent)e).StarPos[2];
                            sys._id = db.GetCollection<StarSystem>().Insert(sys);
                            DBUpdate = true;
                        }
                        if (e.timestamp != sys.LastVisite)
                        {
                            sys.LastVisite = e.timestamp;
                            sys.Visites++;
                            DBUpdate = true;
                            if (!sys.NearStarImported)
                            {
                                var nearSystem = EDSMAPI.GetNearSystem(sys);
                                if (nearSystem != null)
                                {
                                    var i = 0;
                                    List<StarSystem> nearSys = new List<StarSystem>();
                                    foreach (EDSMSystem _edsmsystem in nearSystem)
                                    {
                                        Program.mainForm.UpdateStatus("import system (" + ((++i * 100) / nearSystem.Count()) + "%)(" + i + "/" + nearSystem.Count + ") " + _edsmsystem.name);
                                        if (GetStarSystem(_edsmsystem.id64) == null)
                                            nearSys.Add(edsmSysToStandarSys(_edsmsystem));
                                    }
                                    db.GetCollection<StarSystem>().InsertBulk(nearSys);
                                    sys.NearStarImported = true;
                                }
                            }
                        }

                        break;

                    case "FSSDiscoveryScan":
                        sys = CurrentStarSystem;
                        sys.BodiesCount = ((FSSDiscoveryScanEvent)e).BodyCount;
                        if (((FSSDiscoveryScanEvent)e).Progress == 1.0d)
                        {
                            sys.Bodies.ForEach(b => b.Scanned = true);
                            sys.AllBodiesFound = true;
                        }
                        db.GetCollection<StarSystem>().Update(sys);
                        DBUpdate = true;
                        break;

                    case "FSSAllBodiesFound":
                        sys = CurrentStarSystem;
                        sys.AllBodiesFound = true;
                        db.GetCollection<StarSystem>().Update(sys);
                        DBUpdate = true;
                        break;

                    case "Scan":
                        if (((ScanEvent)e).BodyName.Contains("Belt Cluster")) break;
                        sys = GetStarSystem(((ScanEvent)e).SystemAddress);
                        if (sys == null) sys = AddStarSystem(EDSMAPI.GetSystem(((ScanEvent)e).StarSystem));
                        var body = sys.Bodies.Where(b => b.BodyName == ((ScanEvent)e).BodyName).FirstOrDefault();
                        if (body == null)
                        {
                            body = new Body();
                            body.BodyName = ((ScanEvent)e).BodyName;
                            if (((ScanEvent)e).StarType != null)
                                body.BodyType = ((ScanEvent)e).StarType;
                            else
                                body.BodyType = ((ScanEvent)e).PlanetClass;
                            body.TerraformState = ((ScanEvent)e).TerraformState;
                            body.Distance = ((ScanEvent)e).DistanceFromArrivalLS;
                            body.Scanned = true;

                            sys.Bodies.Add(body);
                            db.GetCollection<StarSystem>().Update(sys);
                        }
                        body.Scanned = true;
                        db.GetCollection<Body>().Update(body);
                        DBUpdate = true;

                        break;

                    default: break;
                }
            }
        }

        private static StarSystem edsmSysToStandarSys(EDSMSystem edsmsystem)
        {
            StarSystem sys = new StarSystem();
            sys.SystemName = edsmsystem.name;
            sys.SystemId = edsmsystem.id64;
            sys.X = edsmsystem.coords.x;
            sys.Y = edsmsystem.coords.y;
            sys.Z = edsmsystem.coords.z;
            return sys;
        }

        private static StarSystem AddStarSystem(EDSMSystem edsmsystem)
        {
            StarSystem starSystem = GetStarSystem(edsmsystem.id64);
            if (starSystem != null) return starSystem;
            StarSystem sys = new StarSystem();
            sys.SystemName = edsmsystem.name;
            sys.SystemId = edsmsystem.id64;
            sys.X = edsmsystem.coords.x;
            sys.Y = edsmsystem.coords.y;
            sys.Z = edsmsystem.coords.z;
            sys._id = db.GetCollection<StarSystem>().Insert(sys);
            return sys;
        }

        public static void AddBodies(StarSystem sys, List<EDSMBody> bodies)
        {
            foreach (var body in bodies)
            {
                if (GetBody(body.name) != null) continue;
                var dbbody = new Body();
                dbbody.BodyName = body.name;
                dbbody.Distance = body.distanceToArrival;
                dbbody.BodyType = body.getEDsubType();
                if (sys.AllBodiesFound)
                    dbbody.Scanned = true;
                if (body.terraformingState != null)
                {
                    if (body.terraformingState.Length == 0)
                        dbbody.TerraformState = "";
                    else if (body.terraformingState == "Candidate for terraforming")
                        dbbody.TerraformState = "Terraformable";
                    else if (body.terraformingState == "Terraformed")
                        dbbody.TerraformState = "Terraformed";
                }
                sys.Bodies.Add(dbbody);
            }
            db.GetCollection<StarSystem>().Update(sys);
        }
    }
}