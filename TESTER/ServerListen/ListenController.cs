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

namespace TESTER.ServerListen
{

    public class ListenController
    {
        #region Переменные и свойства
        /// <summary>
        /// таймер сервера импульсов
        /// </summary>
        readonly Timer timerWork;
        //сервер 
        readonly TcpListener listener = null;
        /// <summary>
        /// порт по умолчанию
        /// </summary>
        int port = 2002;
        /// <summary>
        /// интервал обновления данных
        /// </summary>
        int interval = 1000;
        /// <summary>
        /// класс для работы с сервером импульсов
        /// </summary>
        public ImpulsesClient SourceImpulsServer { get; private set; }
        /// <summary>
        /// проект загрузки
        /// </summary>
        public LoadProject ProjectTester { get; private set; }
        /// <summary>
        /// максимальное количество подключенных клиентов
        /// </summary>
        readonly int m_maxCountClient = 50;

        readonly IList<ICommunicationController> clientControllers;

        #endregion

        public ListenController(bool IsAvto, string connectionString)
        {
            //подключение к импульс серверу

            ServerConfiguration config = ServerConfiguration.FromFile(App.Config.AppSettings.Settings["cfgpath"].Value);
            try
            {
                interval = int.Parse(App.Config.AppSettings.Settings["updateInterval"].Value);
            }
            catch { }
            SourceImpulsServer = new ImpulsesClient(config.Stations, connectionString, App.Config.AppSettings.Settings["file_impuls"].Value, interval);
            ProjectTester = new LoadProject();
            ProjectTester.LoadImpuls(IsAvto, this, config.Stations);
            //
            try
            {
                port = int.Parse(App.Config.AppSettings.Settings["port"].Value);
            }
            catch { }
            //подключаем свой сервер транслятор
            listener = new TcpListener(IPAddress.Any, port);
            timerWork = new System.Timers.Timer();
            timerWork.Interval = 100;
            timerWork.Elapsed += Work;
            clientControllers = new List<ICommunicationController>();
        }

        public void Start()
        {
            //Старт связи с источником
            SourceImpulsServer.Start();
            listener.Start();
            timerWork.Start();
        }

        public void Stop()
        {
            SourceImpulsServer.Stop();
            timerWork.Stop();
            listener.Stop();
            //останвливаем связб с клиентами
            foreach (var communicationController in clientControllers)
            {
                communicationController.Stop();
                DisposeController(communicationController);
            }
        }

        private void Work(object sender, ElapsedEventArgs e)
        {
            ServerStart();
        }

        void ServerStart()
        {
            if (listener.Pending())
            {
                if (clientControllers.Count < m_maxCountClient)
                {
                    var newISController = new ISController(listener.AcceptTcpClient(), ProjectTester);
                    newISController.OnError += ClientControllerOnOnError;
                    clientControllers.Add(newISController);
                    newISController.Start();
                }

            }
        }

        private void ClientControllerOnOnError(ICommunicationController sender, Exception value)
        {
            lock (clientControllers)
            {
                DisposeController(sender);
                clientControllers.Remove(sender);
            }
        }

        private void DisposeController(ICommunicationController controller)
        {
            try
            {
                controller.Dispose();
            }
            catch (Exception e)
            {
               // Logger.Log.LogError("Can't free client. {0}", e);
            }
        }

    }
}
