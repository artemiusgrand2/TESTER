using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.IO;
using System.Windows;
using sdm.diagnostic_section_model;
using TESTER.Enums;
using TESTER.Constants;
using TESTER.ServerListen;

namespace TESTER
{
    public class LoadProject
    {

        #region Переменные и свойства

        Dictionary<string, int> _station = new Dictionary<string,int>();
        /// <summary>
        /// станции сервера 5
        /// </summary>
        public Dictionary<string, int> Station { get { return _station; } }
        FileScript _test = new FileScript();
        /// <summary>
        /// сценарий тестирования участка
        /// </summary>
        public  FileScript TestFile { get { return _test; } }
        Dictionary<int, Stations> _collectionstations = new Dictionary<int, Stations>();
        /// <summary>
        /// коллекция станций 
        /// </summary>
        public Dictionary<int, Stations> CollectionStations
        {
            get
            {
                return _collectionstations;
            }
            set
            {
                _collectionstations = value;
            }
        }

        private readonly IDictionary<TypeImpulsParser, string> parserDictionary = new Dictionary<TypeImpulsParser, string>()
        {
             {TypeImpulsParser.tsWithHelp,  @"\s*@N\s*'(.+)'\s*;\s*(.+)\s*"}, {TypeImpulsParser.ts,  @"\s*@N\s*'(.+)'\s*"},
             {TypeImpulsParser.tuWithHelp,  @"\s*@U\s*'(.+)'\s*;\s*(.+)\s*"}, {TypeImpulsParser.tu,  @"\s*@U\s*'(.+)'\s*"},
        };
        #endregion

        public void LoadImpuls(bool IsAvto, ListenController server, sdm.diagnostic_section_model.StationRecord[] inp_station_records)
        {
            GetCollectionStation(_collectionstations, IsAvto, inp_station_records);
            _test.Server = server;
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


        /// <summary>
        /// возвращаем коллекцию вида название станции -- номер станции
        /// </summary>
        /// <returns></returns>
        private void GetCollectionStation(Dictionary<int, Stations> Collection, bool IsAvto, sdm.diagnostic_section_model.StationRecord[] inp_station_records)
        {
            string avto = string.Empty;
            if (!IsAvto)
                avto = ProjectConstants.sever;
            try
            {
                if (App.Config.AppSettings.Settings["file_impuls"].Value != null)
                {
                    DirectoryInfo info = new DirectoryInfo(App.Config.AppSettings.Settings["file_impuls"].Value);
                    if (info.Exists)
                    {
                        foreach (StationRecord stationInfo in inp_station_records)
                        {
                            string directory = info.FullName + string.Format(@"\{0:D6}", stationInfo.Code);
                            if (Directory.Exists(directory))
                            {
                                foreach (var file in Directory.GetFiles(directory, string.Format("TI{0}.ASM", string.Format("{0:D6}", stationInfo.Code)), SearchOption.TopDirectoryOnly))
                                {
                                    using (StreamReader strReader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.GetEncoding(866)))
                                    {
                                        List<string> file_ts = new List<string>();
                                        string str;
                                        while ((str = strReader.ReadLine()) != null)
                                            file_ts.Add(str);
                                        //добавляемновую станцию
                                        //  string namestation =  GetNameStation(file_ts);
                                        if (!Collection.ContainsKey(stationInfo.Code))
                                        {
                                            var impulses = GetImpulses(file_ts);
                                            if (impulses != null)
                                            {
                                                Collection.Add(stationInfo.Code, new Stations() { CollectionImpulses = impulses, NameStation = stationInfo.Name });
                                                if (!_station.ContainsKey(stationInfo.Name))
                                                    _station.Add(stationInfo.Name, stationInfo.Code);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                System.Windows.MessageBox.Show(string.Format("Папки {0} не существует !!!", directory));
                            }
                        }
                    }
                    else System.Windows.MessageBox.Show(string.Format("Папки {0} не существует !!!", App.Config.AppSettings.Settings["file_impuls"].Value));
                }
            }
            catch { }
        }

        private  IList<Impuls> GetImpulses(List<string> file)
        {
            var impulses = new List<Impuls>();
            try
            {
                foreach (string str in file)
                {
                    foreach(var keyValueParser in parserDictionary)
                    {
                        var parser = Regex.Match(str, keyValueParser.Value);
                        if (parser.Success)
                        {
                            switch (keyValueParser.Key)
                            {
                                case TypeImpulsParser.ts:
                                    {
                                        impulses.Add(new Impuls(parser.Groups[1].Value, TypeImpuls.ts));
                                    }
                                    break;
                                case TypeImpulsParser.tsWithHelp:
                                    {
                                        impulses.Add(new Impuls(parser.Groups[1].Value, TypeImpuls.ts, parser.Groups[2].Value));
                                    }
                                    break;
                                case TypeImpulsParser.tu:
                                    {
                                        impulses.Add(new Impuls(parser.Groups[1].Value, TypeImpuls.tu));
                                    }
                                    break;
                                case TypeImpulsParser.tuWithHelp:
                                    {
                                        impulses.Add(new Impuls(parser.Groups[1].Value, TypeImpuls.tu, parser.Groups[2].Value));
                                    }
                                    break;
                            }
                            break;
                        }
                    }
                
                    //string[] stroka = str.Split(new string[] { " '" }, StringSplitOptions.RemoveEmptyEntries);
                    //if ((stroka.Length > 1))
                    //{
                    //    var nameStartPos = str.IndexOf('\'', 0) + 1;
                    //    if(str.IndexOf('\'', nameStartPos) != -1)
                    //    {
                    //        if (stroka[0] == "@N")
                    //            impulses.Add(new Impuls(str.Substring(nameStartPos, str.IndexOf('\'', nameStartPos) - nameStartPos), TypeImpuls.ts));
                    //        else if (stroka[0] == "@U")
                    //            impulses.Add(new Impuls(str.Substring(nameStartPos, str.IndexOf('\'', nameStartPos) - nameStartPos), TypeImpuls.tu));
                    //    }
                    //}
                }
            }
            catch { }
            return impulses;
        }
    }
}
