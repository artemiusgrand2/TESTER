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
        public static bool Close = false;

        public static IList<int> StationsNumber = new List<int>();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                if(e.Args[0] == "-d")
                {
                    if((e.Args.Length -1) == 2 || (e.Args.Length - 1) == 4)
                    {
                        int buffer;
                        for (var i = 1; i < e.Args.Length; i++)
                        {
                            if(int.TryParse(e.Args[i], out buffer))
                            {
                                StationsNumber.Add(buffer);
                            }
                            else
                            {
                                MessageBox.Show(string.Format("Номер станции {0} имеет неверный формат", e.Args[i]), "", MessageBoxButton.OK, MessageBoxImage.Information);
                                Close = true;
                                Shutdown();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Количество станций должно быть 2 или 4", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close = true;
                        Shutdown();
                    }

                }
            }
        }
    }
}
