using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace ED_Explorator_Companion
{
    internal class EDSMAPI
    {
        private static DateTime GetNearSystemAsyncThrottle = DateTime.MinValue;
        private static DateTime GetSystemInformationThrottle = DateTime.MinValue;

        public static void Run()
        {
            while (true)
            {
                using var context = new Context();
                var currentSys = Database.GetCurrentStarSystem(context);
                var sysToImportQuery = Database.GetStarSystemQuery(context).Where(s => !s.Imported);

                int count;
                count = sysToImportQuery.Count();

                if (count == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                if (currentSys == null) currentSys = Database.GetStarSystem(context, "Sol");

                StarSystem sysToImport;
                sysToImport = sysToImportQuery.OrderBy(s => s.AllBodiesFound).ThenBy(s => s.TriedToImport).ThenBy(s => (s.X - currentSys.X) * (s.X - currentSys.X) + (s.Y - currentSys.Y) * (s.Y - currentSys.Y) + (s.Z - currentSys.Z) * (s.Z - currentSys.Z)).First();
                Program.mainForm.UpdateImport(count, sysToImport);

                sysToImport.TriedToImport++;
                if (sysToImport.AllBodiesFound)
                {
                    sysToImport.Imported = true;
                }
                else
                {
                    List<EDSMBody> bodies;
                    bodies = GetSystemInformation(sysToImport.SystemName);

                    if (bodies != null)
                    {
                        Database.AddBodies(context, sysToImport, bodies);
                        sysToImport.Imported = true;
                    }
                    if (currentSys != null && currentSys.DistanceFrom(sysToImport) <= 50 && !Database.StopUpdatingGui)
                        Database.Queue.Enqueue(new UpdateFrontEvent(true, true));
                }
                context.SaveChanges();
            }
        }

        public static EDSMSystem GetSystem(string name)
        {
            EDSMSystem sys = null;

            string path = "https://www.edsm.net/api-v1/system?systemName=" + name + "&showCoordinates=1";
            HttpResponseMessage response;
            try
            {
                response = (new HttpClient()).GetAsync(new Uri(path)).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                try
                {
                    sys = JsonConvert.DeserializeObject<EDSMSystem>(json);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return sys;
        }

        internal static List<EDSMSystem> GetNearSystemAsync(StarSystem sys)
        {
            if (GetNearSystemAsyncThrottle > DateTime.Now) Thread.Sleep(GetNearSystemAsyncThrottle - DateTime.Now);

            string path = "https://www.edsm.net/api-v1/sphere-systems?radius=50&showCoordinates=1&x=" + sys.X + "&y=" + sys.Y + "&z=" + sys.Z;
            HttpResponseMessage response;
            try
            {
                response = (new HttpClient()).GetAsync(new Uri(path)).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                return null;
            }

            int resetRate = 0;
            IEnumerable<string> values;

            if (response.Headers.TryGetValues("x-rate-limit-reset", out values))
            {
                var e = values.GetEnumerator();
                e.MoveNext();
                resetRate = int.Parse(e.Current);
            }

            Console.WriteLine("NSA - " + resetRate);

            GetNearSystemAsyncThrottle = DateTime.Now.AddSeconds(resetRate);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                try
                {
                    return JsonConvert.DeserializeObject<List<EDSMSystem>>(json);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        internal static List<EDSMBody> GetSystemInformation(string name)
        {
            if (GetSystemInformationThrottle > DateTime.Now) Thread.Sleep(GetSystemInformationThrottle - DateTime.Now);

            string path = "https://www.edsm.net/api-system-v1/bodies?systemName=" + name;
            HttpResponseMessage response;
            try
            {
                response = new HttpClient().GetAsync(new Uri(path)).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                return null;
            }

            int resetRate = 0;
            IEnumerable<string> values;

            if (response.Headers.TryGetValues("x-rate-limit-reset", out values))
            {
                var e = values.GetEnumerator();
                e.MoveNext();
                resetRate = int.Parse(e.Current);
            }

            Console.WriteLine("GSI " + name + " - " + resetRate);

            GetSystemInformationThrottle = DateTime.Now.AddSeconds(resetRate);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (json == "{}") return null;
                try
                {
                    return JToken.Parse(json)["bodies"].ToObject<List<EDSMBody>>();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }
    }
}