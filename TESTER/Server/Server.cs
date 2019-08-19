using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Timers;
using System.Net;
using System.Net.Sockets;
using sdm.diagnostic_section_model;
using sdm.diagnostic_section_model.client_impulses;
using System.Configuration;

namespace TESTER
{
    enum Request
    {
        StationTables = 0x0101,
        Command = 0x0102,
        ListOfTables = 0x0103
    }

    class Server
    {
        #region Переменные и свойства
        /// <summary>
        /// таймер сервера импульсов
        /// </summary>
        Timer _timerpotok = new System.Timers.Timer();
        //сервер 
        TcpListener server = null;
        /// <summary>
        /// порт по умолчанию
        /// </summary>
        int port = 2002;
        /// <summary>
        /// интервал обновления данных
        /// </summary>
        int interval = 1000;
        ImpulsesClient _client;
        /// <summary>
        /// класс для работы с сервером импульсов
        /// </summary>
        public ImpulsesClient Client
        {
            get
            {
                return _client;
            }
            set
            {
                _client = value;
            }
        }
        LoadProject _load;
        /// <summary>
        /// проект загрузки
        /// </summary>
        public LoadProject Load { get { return _load; } set { _load = value; } }
        #endregion

        public Server(bool IsAvto, string connectionString)
        {
            //подключение к импульс серверу

            ServerConfiguration config = ServerConfiguration.FromFile(App.Config.AppSettings.Settings["cfgpath"].Value);
            try
            {
                interval = int.Parse(App.Config.AppSettings.Settings["updateInterval"].Value);
            }
            catch { }
            _client = new ImpulsesClient(config.Stations, connectionString, App.Config.AppSettings.Settings["file_impuls"].Value, interval);
            _load = new LoadProject();
            _load.LoadImpuls(IsAvto, this, config.Stations);
        }

        public void Start()
        {
            //Старт связи с cервером импульсов
            _client.Start();
            try
            {
                port = int.Parse(App.Config.AppSettings.Settings["port"].Value);
            }
            catch { }
            //подключаем свой сервер транслятор
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
            }
            catch { }
            //
            _timerpotok.Interval = 100;
            _timerpotok.Elapsed += _timerserver_Elapsed;
            _timerpotok.Start();
        }

        public void Stop()
        {
            _client.Stop();
            _timerpotok.Stop();
        }

        private void _timerserver_Elapsed(object sender, ElapsedEventArgs e)
        {
            ServerStart();
        }

        void ServerStart()
        {
            try
            {
                // Buffer  для чтения данных
                Byte[] bytes = new Byte[64000];
                //
                if (server.Pending())
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    stream.ReadTimeout = 1000;
                    //размер скачаной информации
                    int lenread;
                    // читаю данные отклиента
                    while ((lenread = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        if (bytes.Length >=13 && ((bytes[0] == 126) && (bytes[lenread - 1] == 126)))
                        {
                            //колекция номеров станций 
                            List<int> station_number = new List<int>();
                            List<byte> Nst = new List<byte>();
                            int packetType = BitConverter.ToInt16(new byte[] { bytes[2], bytes[3] }, 0);
                            if (packetType == (int)Request.ListOfTables || packetType == (int)Request.StationTables)
                            {
                                for (int i = 8; i < lenread - 1; i++)
                                {

                                    if (bytes[i] == 125 && (i < lenread - 2))
                                    {
                                        if (bytes[i + 1] == 93)
                                        {
                                            Nst.Add(bytes[i]);
                                            i++;
                                            continue;
                                        }
                                        //
                                        if (bytes[i + 1] == 94)
                                        {
                                            Nst.Add(126);
                                            i++;
                                            continue;
                                        }
                                    }
                                    Nst.Add(bytes[i]);
                                    //
                                    if (Nst.Count == 4)
                                    {
                                        int answer = BitConverter.ToInt32(Nst.ToArray(), 0);
                                        //if (answer.ToString().Length <= 6)
                                        station_number.Add(answer);
                                        Nst.Clear();
                                    }
                                }
                            }
                            else if (packetType == (int)Request.Command)
                            {
                                if (lenread == 109)
                                {
                                    try
                                    {
                                        if (_load != null)
                                        {
                                            int station = BitConverter.ToInt32(new byte[] { bytes[70], bytes[71], bytes[72], bytes[73]}, 0);
                                            string name_commnad = Encoding.Unicode.GetString(bytes, 74, 32).TrimEnd(new char[]{'\0'});
                                            _load.TestFile.ListTU.Add(new CommandTU() { NameTU = name_commnad, StationNumber = station });
                                            if (!_load.TestFile.IsStart)
                                            {
                                                _load.TestFile.StartTest();
                                            }
                                            //
                                            var answer = FrameParser.MakeFrame(ImpulsesClient.PackCommandAnswer(station, name_commnad), false);
                                            stream.Write(answer, 0, answer.Length);
                                        }
                                    }
                                    catch{ }
                                }
                            }
                            //отправляем данные тс
                            if (station_number.Count > 0)
                            {
                                ServerImpuls serverimpuls = new ServerImpuls(_load.CollectionStations);
                                byte[] answer_ty_server = serverimpuls.Vihod_massiv_dann(station_number);
                                // Отправляем данные на sdm сервер
                                stream.Write(answer_ty_server, 0, answer_ty_server.Length);
                            }
                        }
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                switch (e.SocketErrorCode)
                {
                    case SocketError.Shutdown:
                        Console.WriteLine("Соединение закрыто");
                        break;
                    case SocketError.NotConnected:
                        Console.WriteLine("Соединение закрыто");
                        break;
                    case SocketError.NetworkReset:
                        Console.WriteLine("Соединение закрыто");
                        break;
                }
            }
            catch { }
        }

    }
}
