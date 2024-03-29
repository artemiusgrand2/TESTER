﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Timers;
using System.Net;
using System.Net.Sockets;
using SCADA.Common.ImpulsClient;
using SCADA.Common.ImpulsClient.ServerController;

namespace TESTER.Connections
{

    public class ServerConnections
    {
        #region Переменные и свойства
        //сервер 
        readonly Lister _listener = null;
        //сервер порта для тестирования
        readonly Lister _listenerTest = null;
        /// <summary>
        /// порт по умолчанию
        /// </summary>
        int _port = 2002;
        /// <summary>
        /// порт для тестирования
        /// </summary>
        int _portTest = 5556;
        /// <summary>
        /// интервал обновления данных
        /// </summary>
        int _interval = 1000;
        /// <summary>
        /// класс для работы с сервером импульсов
        /// </summary>
        public ImpulsesClientTCP SourceImpulsServer { get; private set; }
        /// <summary>
        /// проект загрузки
        /// </summary>
        public LoadProject ProjectTester { get; private set; }

        #endregion

        public ServerConnections(string connectionString)
        {
            //подключение к импульс серверу

            ServerConfiguration config = ServerConfiguration.FromFile(App.Config.AppSettings.Settings["cfgpath"].Value);
            try
            {
                _interval = int.Parse(App.Config.AppSettings.Settings["updateInterval"].Value);
            }
            catch { }
            SourceImpulsServer = new ImpulsesClientTCP(config.Stations, connectionString, App.Config.AppSettings.Settings["file_impuls"].Value, _interval);
            var dataContainerBuffer = SourceImpulsServer.Data.Clone();
            ProjectTester = new LoadProject(this, dataContainerBuffer);
            //
            try
            {
                _port = int.Parse(App.Config.AppSettings.Settings["port"].Value);
                _portTest = int.Parse(App.Config.AppSettings.Settings["test_port"].Value);
            }
            catch { }
            //подключаем свой сервер транслятор
            _listener = new Lister(_port, dataContainerBuffer);
            _listenerTest = new Lister(_portTest, dataContainerBuffer, SourceImpulsServer);
        }

        public void Start()
        {
            //Старт связи с источником
            SourceImpulsServer.Start();
            _listener.Start();
            _listenerTest.Start();
        }

        public void Stop()
        {
            SourceImpulsServer.Stop();
            _listener.Stop();
            _listenerTest.Stop();
        }
    }
}
