using System;
using System.Windows.Forms;
using GameCore;
using DataBuildSystem;

namespace BigfileViewer
{
    static class MainProgram
    {
        // Create a logger for use in this class
        public static bool printMessage = true;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Globalization.CultureInfo oldCurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            if (oldCurrentCulture.IetfLanguageTag != "en-US")
            {
                Console.WriteLine("Please change your regional settings to en-US!");
            }
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            CommandLine arg = new CommandLine(GameCore.Environment.GetCommandLineArgs());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainGUI(arg));

            System.Threading.Thread.CurrentThread.CurrentCulture = oldCurrentCulture;
        }
    }
}