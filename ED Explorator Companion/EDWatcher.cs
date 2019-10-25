using ED_Explorator_Companion.Event;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;

namespace ED_Explorator_Companion
{
    internal class EDWatcher
    {
        private static DateTime LastEvent = DateTime.MinValue;
        private static bool starting = true;

        internal static void Init()
        {
            var ret = Database.GetConfig(new Context(), "LastEvent");
            if (ret == null)
                Database.Queue.Enqueue(new SetConfig("LastEvent", DateTime.MinValue.ToString()));
            else
                LastEvent = DateTime.Parse(ret.Value);
        }

        public static void CheckEDFiles()
        {
            string[] files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Saved Games\\Frontier Developments\\Elite Dangerous"));
            foreach (string file in files)
            {
                if (file.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessFile(file);
                }
            }
        }

        public static void Run()
        {
            while (true)
            {
                if (Database.Queue.IsEmpty)
                    CheckEDFiles();
                Thread.Sleep(500);
            }
        }

        private static void ProcessFile(String fullpath)
        {
            string line;
            int eventnbr = 0;
            var fileInfo = new FileInfo(fullpath);
            if (fileInfo.LastWriteTime <= LastEvent) return;
            using (StreamReader streamReader = new StreamReader(File.Open(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    JObject e = (JObject)JToken.Parse(line);
                    if (((DateTime)e["timestamp"]) >= LastEvent)
                    {
                        switch ((string)e["event"])
                        {
                            case "FSSDiscoveryScan":
                                Database.Queue.Enqueue(e.ToObject<FSSDiscoveryScanEvent>());
                                Database.Queue.Enqueue(new UpdateFrontEvent());
                                eventnbr++;
                                break;

                            case "FSSAllBodiesFound":
                                Database.Queue.Enqueue(e.ToObject<FSSAllBodiesFoundEvent>());
                                Database.Queue.Enqueue(new UpdateFrontEvent());
                                eventnbr++;
                                break;

                            case "FSDJump":
                                var stopUpdatingGui = Database.StopUpdatingGui;
                                if (!stopUpdatingGui)
                                    Database.Queue.Enqueue(new CustomEvent("StopUpdatingGuiOn"));
                                Database.Queue.Enqueue(new SetConfig("CurrentSystem", (string)e["StarSystem"]));
                                Database.Queue.Enqueue(new UpdateFrontEvent(true, true));
                                Database.Queue.Enqueue(e.ToObject<FSDJumpEvent>());
                                Database.Queue.Enqueue(new UpdateFrontEvent(true));
                                if (!stopUpdatingGui)
                                    Database.Queue.Enqueue(new CustomEvent("StopUpdatingGuiOff"));
                                eventnbr++;
                                break;

                            case "Scan":
                                Database.Queue.Enqueue(e.ToObject<ScanEvent>());
                                Database.Queue.Enqueue(new UpdateFrontEvent());
                                eventnbr++;
                                break;

                            case "FSDTarget":
                                Database.Queue.Enqueue(e.ToObject<FSDTargetEvent>());
                                break;

                            default: break;
                        }
                        LastEvent = (DateTime)e["timestamp"];
                        Database.Queue.Enqueue(new SetConfig("LastEvent", LastEvent.ToString()));
                    }
                }
            }
        }
    }
}