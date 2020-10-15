using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.IO;
using System.Windows;
using SCADA.Common.ImpulsClient;
using TESTER.Enums;
using TESTER.Constants;
using TESTER.Connections;

namespace TESTER
{
    public class LoadProject
    {

        #region Переменные и свойства

        /// <summary>
        /// соответствие имени станции и еср кода 
        /// </summary>
        public Dictionary<string, int> StationsName { get; private set; }
        FileScript _test = new FileScript();
        /// <summary>
        /// сценарий тестирования участка
        /// </summary>
        public  FileScript TestFile { get { return _test; } }
        /// <summary>
        /// коллекция станций 
        /// </summary>
        public Dictionary<int, Station> Stations
        {
            get
            {
                return _dataContainer.Stations;
            }
        }
        //
        DataContainer _dataContainer;
        #endregion

        public LoadProject(ServerConnections server, DataContainer dataContainer)
        {
            _test.Server = server;
            _dataContainer = dataContainer;
            StationsName = new Dictionary<string, int>();
            foreach(var station in _dataContainer.Stations)
            {
                if (!StationsName.ContainsKey(station.Value.Name))
                    StationsName.Add(station.Value.Name, station.Key);
            }
        }

        /// <summary>
        /// загружаем сценарии тестирования
        /// </summary>
        public void LoadTest(string file)
        {
            try
            {
                if (!string.IsNullOrEmpty(file))
                {
                    if (new FileInfo(file).Exists)
                    {
                        _test.LoadScript(File.ReadAllLines(file, Encoding.GetEncoding(1251)));
                    }
                }
            }
            catch (Exception error) { MessageBox.Show(error.Message); }
        }

    }
}
