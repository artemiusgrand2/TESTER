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

        public static bool Topmost { get; private set; } = false;

        public static Configuration Config;

        public static bool IsServer1 { get; private set; } = true;

        public static int CountServer { get; private set; } = 1;

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

                    if (arg.IndexOf("-s1") != -1)
                    {
                        IsServer1 = true;
                        continue;
                    }

                    if (arg.IndexOf("-s2") != -1)
                    {
                        IsServer1 = false;
                        continue;
                    }

                    if (arg.IndexOf("-tm") != -1)
                    {
                        Topmost = true;
                        continue;
                    }

                    if (arg.IndexOf("-c=") != -1)
                    {
                        var configFile = arg.Replace(@"-c=", string.Empty);
                        if (System.IO.File.Exists(configFile))
                        {
                            var configMap = new ExeConfigurationFileMap();
                            configMap.ExeConfigFilename = configFile;
                            Config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                        }
                        else
                        {
                            MessageBox.Show(string.Format("Файла конфигурации по адресу {0} не существует", configFile), "", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
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
            //
            if(Config == null)
            {
                var configMap = new ExeConfigurationFileMap();
                configMap.ExeConfigFilename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                Config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            }
            //
            Validation();
        }

        private void Validation()
        {
            if (!Config.AppSettings.Settings.AllKeys.Contains("port"))
            {
                MessageBox.Show("Не введено значение ключа - 'port', порта для прослушки", "", MessageBoxButton.OK, MessageBoxImage.Information);
                Close = true;
                Shutdown();
            }
            //
            if (!Config.AppSettings.Settings.AllKeys.Contains("cfgpath"))
            {
                MessageBox.Show("Не введено значение ключа - 'file_impuls', нахождение таблиц ТУ и ТС", "", MessageBoxButton.OK, MessageBoxImage.Information);
                Close = true;
                Shutdown();
            }
            //
            if (!Config.AppSettings.Settings.AllKeys.Contains("file_impuls"))
            {
                MessageBox.Show("Не введено значение ключа - 'cfgpath', нахождение файла конфигурации импульс сервера", "", MessageBoxButton.OK, MessageBoxImage.Information);
                Close = true;
                Shutdown();
            }
            //
            if (!Config.AppSettings.Settings.AllKeys.Contains("server1") && IsServer1)
            {
                MessageBox.Show("Не введено значение ключа - 'server1', адрес и порт импульс сервера №1", "", MessageBoxButton.OK, MessageBoxImage.Information);
                Close = true;
                Shutdown();
            }
            //
            if (!Config.AppSettings.Settings.AllKeys.Contains("server2") && !IsServer1)
            {
                MessageBox.Show("Не введено значение ключа - 'server2', адрес и порт импульс сервера №2", "", MessageBoxButton.OK, MessageBoxImage.Information);
                Close = true;
                Shutdown();
            }
            //
            if (Config.AppSettings.Settings.AllKeys.Contains("server1") && Config.AppSettings.Settings.AllKeys.Contains("server2"))
                CountServer = 2;
        }
    }
}
