using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace CustomRPC.WPF
{
    static class Program
    {
        public static Mutex AppMutex { get; private set; }
        public static string IPCPath { get; private set; }
        public static bool IsSecondInstance { get; private set; }
        public static int WM_SHOWFIRSTINSTANCE { get; private set; }
        public static int WM_IMPORTPRESET { get; private set; }

        [STAThread]
        static void Main(string[] args)
        {
            string presetFile = null;
#if DEBUG
            string mutexName = "CustomRP dev";
#else
            string mutexName = "CustomRP";
#endif
            bool isSilent = false;

            if (args.Length > 0 && (args[0] == "--help" || args[0] == "-?"))
            {
                var helpText = new StringBuilder();
                helpText.AppendLine("Usage: CustomRP.exe [options] [preset file path]");
                helpText.AppendLine();
                helpText.AppendLine("List of options:");
                helpText.AppendLine("-2, --second-instance: open as second instance");
                helpText.AppendLine("-s, --silent-import: silent preset import");
                helpText.AppendLine("-?, --help: shows this help text");
                helpText.AppendLine();
                helpText.AppendLine("Option(s) and file path(s) can be included in any order. Including more than one file path will result in the last valid one being used.");
                MessageBox.Show(helpText.ToString(), "Custom Discord Rich Presence Manager");
                return;
            }

            if (args.Length > 0 && args[0] == "uninstall")
            {
                if (MessageBox.Show("Do you want to delete all settings?", "Custom Discord Rich Presence Manager", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    string settingsPath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                    settingsPath = Path.GetFullPath(Path.Combine(settingsPath, @"..\.."));
                    Directory.Delete(settingsPath, true);
                }

                return;
            }

            foreach (string arg in args.Distinct())
            {
                if (arg == "--second-instance" || arg == "-2")
                {
                    if (IsSecondInstance)
                        continue;

                    IsSecondInstance = true;
                    mutexName += " 2";
                }
                else if (arg == "--silent-import" || arg == "-s")
                    isSilent = true;
                else if (File.Exists(arg))
                    presetFile = arg;
            }

            WM_SHOWFIRSTINSTANCE = WinApi.RegisterWindowMessage("WM_SHOWFIRSTINSTANCE|" + mutexName);
            WM_IMPORTPRESET = WinApi.RegisterWindowMessage("WM_IMPORTPRESET|" + mutexName);

            AppMutex = new Mutex(true, mutexName, out bool createdNew);
            IPCPath = Path.GetTempPath() + mutexName + ".pipe";

            if (!createdNew)
            {
                if (!isSilent)
                    WinApi.PostMessage(new IntPtr(0xffff), WM_SHOWFIRSTINSTANCE, IntPtr.Zero, IntPtr.Zero);

                if (presetFile != null)
                {
                    try
                    {
                        File.WriteAllText(IPCPath, presetFile);
                        WinApi.PostMessage(new IntPtr(0xffff), WM_IMPORTPRESET, IntPtr.Zero, IntPtr.Zero);
                    }
                    catch
                    {
                        MessageBox.Show("Invalid preset file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                return;
            }

            try
            {
                Crashes.ShouldProcessErrorReport = (ErrorReport report) =>
                {
                    if (report.StackTrace.StartsWith("Microsoft.AppCenter.Crashes.TestCrashException") ||
                    report.StackTrace.StartsWith("System.OutOfMemoryException"))
                        return false;

                    return true;
                };

                Crashes.GetErrorAttachments = (ErrorReport report) =>
                {
                    StringBuilder settingsTxt = new StringBuilder();
                    settingsTxt.AppendLine("WPF Version - Settings not yet implemented");

                    StringBuilder rpcLog = new StringBuilder();
                    try
                    {
                        string[] temp = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\" + report.AppErrorTime.ToString("yyyy-MM-dd") + ".log");
                        
                        foreach (string line in temp)
                        {
                            if (line.Contains("applicationID"))
                                continue;

                            rpcLog.AppendLine(line);
                        }
                    }
                    catch
                    {
                        rpcLog.AppendLine("No log was found!");
                    }

                    return new ErrorAttachmentLog[]
                    {
                        ErrorAttachmentLog.AttachmentWithText(settingsTxt.ToString(), "settings.txt"),
                        ErrorAttachmentLog.AttachmentWithText(rpcLog.ToString(), "rpc.log")
                    };
                };

                AppCenter.SetCountryCode(RegionInfo.CurrentRegion.TwoLetterISORegionName);
                if (AppCenterSecret.Value != "{app secret}")
                {
                    AppCenter.Start(AppCenterSecret.Value, typeof(Analytics), typeof(Crashes));
                }

                var app = new App();
                app.InitializeComponent();
                
                var mainWindow = new MainWindow();
                app.MainWindow = mainWindow;
                mainWindow.Show();
                
                app.Run();
            }
            catch (Exception ex)
            {
                var errMsg = new StringBuilder();
                errMsg.AppendLine(DateTime.Now.ToLocalTime().ToString());
                errMsg.AppendLine(ex.ToString());
                errMsg.AppendLine();

                bool error = true;
                while (error)
                {
                    try
                    {
                        File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\crash.log", errMsg.ToString());
                        error = false;
                    }
                    catch
                    {
                        System.Threading.Tasks.Task.Delay(100);
                        error = true;
                    }
                }

                Exception ex_inner = ex;

                while (ex_inner.InnerException != null)
                    ex_inner = ex_inner.InnerException;

                if (ex_inner is FileNotFoundException && ex_inner.Message.Contains("Version=") || ex_inner is FileLoadException || ex_inner is BadImageFormatException)
                {
                    var result = MessageBox.Show($"{ex_inner.Message}\r\n\r\nError loading assembly from {AppDomain.CurrentDomain.BaseDirectory}", 
                        "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);

                    if (result == MessageBoxResult.OK)
                    {
                        AppMutex.Close();
                        System.Windows.Application.Current.Shutdown();
                    }
                }
                else
                {
                    MessageBox.Show(ex.ToString(), "CustomRP - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }
            }
            finally
            {
                Utils.SaveSettings();
            }

            GC.KeepAlive(AppMutex);
        }
    }
}