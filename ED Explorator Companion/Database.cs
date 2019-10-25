using ED_Explorator_Companion.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ED_Explorator_Companion
{
    internal class Database
    {
        private static object bodyLock = new object();
        public static ConcurrentQueue<BaseEvent> Queue = new ConcurrentQueue<BaseEvent>();
        internal static object lockObj = new object();
        public static StarSystem CurrentStarSystem;

        public static bool StopUpdatingGui { get; private set; } = false;

        public static ConfigPair GetConfig(Context context, string key)
        {
            return context.ConfigPair.FirstOrDefault(c => c.Key == key);
        }

        internal static void Init()
        {
            using (var context = new Context())
            {
                context.Database.EnsureCreated();
                context.Database.AutoTransactionsEnabled = false;
                CurrentStarSystem = GetCurrentStarSystem(context);
            }
        }

        public static IIncludableQueryable<StarSystem, List<Body>> GetStarSystemQuery(Context context)
        {
            return context.StarSystems.Include(s => s.Bodies);
        }

        public static StarSystem GetStarSystem(Context context, string systemName)
        {
            return GetStarSystemQuery(context).FirstOrDefault(s => s.SystemName == systemName);
        }

        private static Body GetBody(Context context, string bodyName)
        {
            return context.Bodies.Include(b => b.StarSystem).ThenInclude(s => s.Bodies).FirstOrDefault(b => b.BodyName == bodyName);
        }

        public static StarSystem GetCurrentStarSystem(Context context)
        {
            var currentSysConfig = GetConfig(context, "CurrentSystem");
            if (currentSysConfig == null)
            {
                var sys = GetStarSystem(context, "Sol");
                if (sys == null) sys = AddStarSystem(context, EDSMAPI.GetSystem("Sol"));
                context.SaveChanges();
                return sys;
            }

            return GetStarSystem(context, currentSysConfig.Value);
        }

        public static void Run()
        {
            bool DBUpdate = false;
            while (true)
            {
                using var context = new Context();

                Program.mainForm.UpdateStatus(Queue.Count + " Event To Process");
                if (Queue.IsEmpty)
                {
                    Thread.Sleep(500);
                    continue;
                }
                Queue.TryDequeue(out var e);
                Program.mainForm.UpdateStatus(Queue.Count + " Event To Process ( " + e.Event + " )");
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

                        lock (Database.lockObj)
                            if (((UpdateFrontEvent)e).Full)
                            {
                                var currentSys = CurrentStarSystem;
                                var nearSystems = GetStarSystemQuery(context)
                                    .Where(s => (s.X - currentSys.X) * (s.X - currentSys.X) + (s.Y - currentSys.Y) * (s.Y - currentSys.Y) + (s.Z - currentSys.Z) * (s.Z - currentSys.Z) <= 2500)
                                    .OrderBy(s => (s.X - currentSys.X) * (s.X - currentSys.X) + (s.Y - currentSys.Y) * (s.Y - currentSys.Y) + (s.Z - currentSys.Z) * (s.Z - currentSys.Z))
                                    .ToListAsync().GetAwaiter().GetResult();
                                Program.mainForm.updateGrid(currentSys, nearSystems);
                            }
                            else
                            {
                                Program.mainForm.UpdateFirstLine(CurrentStarSystem);
                            }
                        break;

                    case "FSDTarget":
                        var sysName = ((FSDTargetEvent)e).Name;
                        var targetSys = GetStarSystem(context, sysName);
                        if (targetSys == null)
                        {
                            Program.mainForm.UpdateStatus(sysName + " probably not scanned");
                            Thread.Sleep(1000);
                        }
                        break;

                    case "SetConfig":
                        lock (Database.lockObj)
                        {
                            var cp = GetConfig(context, ((SetConfig)e).key);
                            if (cp == null)
                            {
                                cp = new ConfigPair();
                                cp.Key = ((SetConfig)e).key;

                                context.ConfigPair.Add(cp);
                            }
                            cp.Value = ((SetConfig)e).value.ToString();

                            context.SaveChanges();
                        }
                        break;

                    case "FSDJump":
                        lock (Database.lockObj)
                        {
                            var sys = GetStarSystem(context, ((FSDJumpEvent)e).StarSystem);
                            if (sys == null)
                            {
                                sys = new StarSystem();
                                sys.SystemName = ((FSDJumpEvent)e).StarSystem;
                                sys.X = ((FSDJumpEvent)e).StarPos[0];
                                sys.Y = ((FSDJumpEvent)e).StarPos[1];
                                sys.Z = ((FSDJumpEvent)e).StarPos[2];

                                context.StarSystems.Add(sys);
                                context.SaveChanges();
                                DBUpdate = true;
                            }
                            if (e.timestamp != sys.LastVisite)
                            {
                                sys.LastVisite = e.timestamp;
                                sys.Visites++;
                                DBUpdate = true;
                                if (!sys.NearStarImported)
                                {
                                    var nearSystem = EDSMAPI.GetNearSystemAsync(sys);
                                    if (nearSystem != null)
                                    {
                                        var i = 0;
                                        foreach (EDSMSystem _edsmsystem in nearSystem)
                                        {
                                            Program.mainForm.UpdateStatus("import system (" + ((++i * 100) / nearSystem.Count()) + "%)(" + i + "/" + nearSystem.Count + ") " + _edsmsystem.name);
                                            if (GetStarSystem(context, _edsmsystem.name) == null)
                                                AddStarSystem(context, _edsmsystem);
                                        }

                                        sys.NearStarImported = true;
                                    }
                                }
                            }
                            CurrentStarSystem = sys;
                            context.SaveChanges();
                        }

                        break;

                    case "FSSDiscoveryScan":
                        lock (Database.lockObj)
                        {
                            var sys = GetStarSystem(context, ((FSSDiscoveryScanEvent)e).SystemName);
                            if (sys == null) break;
                            sys.BodiesCount = ((FSSDiscoveryScanEvent)e).BodyCount;
                            if (((FSSDiscoveryScanEvent)e).Progress == 1.0d)
                            {
                                foreach (var body in sys.Bodies)
                                    body.Scanned = true;
                                sys.AllBodiesFound = true;
                            }

                            context.SaveChanges();
                            DBUpdate = true;
                        }
                        break;

                    case "FSSAllBodiesFound":
                        lock (Database.lockObj)
                        {
                            var sys = GetStarSystem(context, ((FSSAllBodiesFoundEvent)e).SystemName);
                            if (sys == null) break;
                            sys.AllBodiesFound = true;

                            context.SaveChanges();
                            DBUpdate = true;
                        }
                        break;

                    case "Scan":
                        lock (Database.lockObj)
                        {
                            lock (bodyLock)
                            {
                                if (((ScanEvent)e).BodyName.Contains("Belt Cluster")) break;
                                var body = GetBody(context, ((ScanEvent)e).BodyName);
                                var sys = GetStarSystem(context, ((ScanEvent)e).StarSystem);
                                if (sys == null) break;
                                if (body == null)
                                {
                                    body = new Body();
                                    body.StarSystem = sys;
                                    body.BodyName = ((ScanEvent)e).BodyName;
                                    if (((ScanEvent)e).StarType != null)
                                        body.BodyType = ((ScanEvent)e).StarType;
                                    else
                                        body.BodyType = ((ScanEvent)e).PlanetClass;
                                    body.TerraformState = ((ScanEvent)e).TerraformState;
                                    body.Distance = ((ScanEvent)e).DistanceFromArrivalLS;

                                    context.Bodies.Add(body);
                                }

                                body.Scanned = true;
                                DBUpdate = true;

                                context.SaveChanges();
                            }
                        }
                        break;

                    default: break;
                }
            }
        }

        private static StarSystem AddStarSystem(Context context, EDSMSystem edsmsystem)
        {
            StarSystem sys = new StarSystem();
            sys.SystemName = edsmsystem.name;
            sys.X = edsmsystem.coords.x;
            sys.Y = edsmsystem.coords.y;
            sys.Z = edsmsystem.coords.z;
            context.StarSystems.Add(sys);
            return sys;
        }

        public static void AddBodies(Context context, StarSystem sys, List<EDSMBody> bodies)
        {
            lock (bodyLock)
            {
                foreach (var body in bodies)
                {
                    if (GetBody(context, body.name) != null) continue;
                    var dbbody = new Body();
                    dbbody.StarSystem = sys;
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
                    context.Bodies.Add(dbbody);
                }
                context.SaveChanges();
            }
        }
    }
}