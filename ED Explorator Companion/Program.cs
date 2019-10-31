using System;
using System.Threading;
using System.Windows.Forms;

namespace ED_Explorator_Companion
{
    internal static class Program
    {
        public static readonly SystemDataGrid mainForm = new SystemDataGrid();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var dbThread = new Thread(Database.Run);
            var watcherThread = new Thread(EDWatcher.Run);
            var importThread = new Thread(EDSMAPI.Run);

            Database.Init();
            EDWatcher.Init();

            dbThread.Start();

            Database.Queue.Enqueue(new UpdateFrontEvent(true, true));
            Database.Queue.Enqueue(new CustomEvent("StopUpdatingGuiOn"));
            EDWatcher.CheckEDFiles();
            Database.Queue.Enqueue(new CustomEvent("StopUpdatingGuiOff"));

            importThread.Start();
            watcherThread.Start();

            dbThread.Name = "Database Thread";
            watcherThread.Name = "Journal Watcher Thread";
            importThread.Name = "EDSM API Thread";

            Application.ApplicationExit += delegate (object o, EventArgs e)
            {
                dbThread.Abort();
                watcherThread.Abort();
                importThread.Abort();
            };
            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(mainForm);
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) => ShowExceptionDetails(e.Exception);

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => ShowExceptionDetails(e.ExceptionObject as Exception);

        private static void ShowExceptionDetails(Exception Ex) => MessageBox.Show(Ex.StackTrace + "\n" + Ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}