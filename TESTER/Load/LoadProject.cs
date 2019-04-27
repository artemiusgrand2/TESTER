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

namespace TESTER
{
    class LoadProject
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
        #endregion

        public void LoadImpuls(bool IsAvto, Server server, sdm.diagnostic_section_model.StationRecord[] inp_station_records)
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
        /// 
        /// </summary>
        /// <param name="file"></param>
        private void ParserTableFile(string file)
        {
            if (File.Exists(file))
            {

            }
            else
                MessageBox.Show(string.Format("Файл разбики по ячейкам по адресу - {0} не найден", file));
        }

        private bool IsStationCollection(int station)
        {
            foreach (KeyValuePair<int, Stations> value in _collectionstations)
            {
                if (value.Key == station)
                    return true;
            }
            //
            return false;
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
                if (ConfigurationManager.AppSettings["file_impuls"] != null)
                {
                    DirectoryInfo info = new DirectoryInfo(ConfigurationManager.AppSettings["file_impuls"]);
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
                                                Collection.Add(stationInfo.Code, new Stations() { CollectionImpulses = impulses, IsAllActive = avto, NameStation = stationInfo.Name });
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
                    else System.Windows.MessageBox.Show(string.Format("Папки {0} не существует !!!", ConfigurationManager.AppSettings["file_impuls"]));
                }
            }
            catch { }
        }

        //private string GetNameStation(List<string> file)
        //{
        //    try
        //    {
        //        foreach (string str in file)
        //        {
        //            string[] massiv = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //            if (massiv.Length == 2 && massiv[0].ToUpper() == "@BEGIN")
        //            {
        //                return massiv[1].Trim(new char[] { '\'' });
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return string.Empty;
        //    }
        //    //
        //    return string.Empty;
        //}

        private static IList<Impuls> GetImpulses(List<string> file)
        {
            var impulses = new List<Impuls>();
            try
            {
                foreach (string str in file)
                {
                    string[] stroka = str.Split(new string[] { " '" }, StringSplitOptions.RemoveEmptyEntries);
                    if ((stroka.Length > 1))
                    {
                        var nameStartPos = str.IndexOf('\'', 0) + 1;
                        if(str.IndexOf('\'', nameStartPos) != -1)
                        {
                            if (stroka[0] == "@N")
                                impulses.Add(new Impuls(str.Substring(nameStartPos, str.IndexOf('\'', nameStartPos) - nameStartPos), TypeImpuls.ts));
                            else if (stroka[0] == "@U")
                                impulses.Add(new Impuls(str.Substring(nameStartPos, str.IndexOf('\'', nameStartPos) - nameStartPos), TypeImpuls.tu));
                        }
                    }
                }
            }
            catch { }
            return impulses;
        }
    }
}
