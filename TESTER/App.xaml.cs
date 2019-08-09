using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace TESTER
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool Close { get; private set; } = false;

        public static IList<int> StationsNumber { get; private set; } = new List<int>();

        public static bool IsDifferences { get; private set; } = false;

        public static string Filter { get; private set; } = string.Empty;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                int buffer;
                foreach(var arg in e.Args)
                {
                    if (arg == "-d")
                    {
                        IsDifferences = true;
                        continue;
                    }
                    if (arg.IndexOf("-r=") != -1)
                    {
                        Filter = arg.Replace("-r=", string.Empty);
                        continue;
                    }
                    //
                    if (int.TryParse(arg, out buffer))
                    {
                        StationsNumber.Add(buffer);
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Номер станции {0} имеет неверный формат", arg), "", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close = true;
                        Shutdown();
                    }
                }
                //
                if(!(StationsNumber.Count ==2 || StationsNumber.Count == 4))
                {
                    MessageBox.Show("Количество станций должно быть 2 или 4", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close = true;
                    Shutdown();
                }
            }
        }
    }
}
