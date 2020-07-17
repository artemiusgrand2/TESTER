using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Text;
using System.Net.Sockets;
using sdm.diagnostic_section_model;
using sdm.diagnostic_section_model.client_impulses;
using sdm.diagnostic_section_model.client_impulses.requests;

namespace TESTER.ServerListen
{
    public class ISController : ICommunicationController
    {
        private readonly TimeSpan echoMessageTimeout = TimeSpan.FromSeconds(7);

        private DateTime lastCommunicatedTime;
        private bool isStop;
        private readonly Thread thread;
        private readonly TcpClient client;
        private readonly NetworkStream stream;
        private readonly LoadProject project;
        private readonly ServerImpuls serverimpuls;

        public event ErrorHandler<ICommunicationController, Exception> OnError;


        public ISController(TcpClient client, LoadProject project)
        {
            lastCommunicatedTime = DateTime.Now;
            this.client = client;
            this.stream = client.GetStream();
            this.project = project;
            this.serverimpuls = new ServerImpuls(this.project.CollectionStations);
            thread = new Thread(Work);
        }

        public void Start()
        {
            if (!isStop)
            {
                isStop = false;
                thread.Start();
            }
        }

        public void Stop()
        {
            isStop = true;
            thread.Join();
        }

        public void Dispose()
        {
            stream.Dispose();
            client.Close();
        }

        private void Work()
        {
            try
            {
                while (!isStop)
                {
                    // Buffer  для чтения данных
                    Byte[] bytes = new Byte[64000];
                    // читаю данные отклиента
                    var readCount = 0;
                    if ((readCount = stream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        if (bytes.Length >= 13 && ((bytes[0] == 126) && (bytes[readCount - 1] == 126)))
                        {
                            //колекция номеров станций 
                            List<int> station_number = new List<int>();
                            List<byte> Nst = new List<byte>();
                            int packetType = BitConverter.ToInt16(new byte[] { bytes[2], bytes[3] }, 0);
                            if (packetType == (int)Request.ListOfTables || packetType == (int)Request.StationTables)
                            {
                                for (int i = 8; i < readCount - 1; i++)
                                {

                                    if (bytes[i] == 125 && (i < readCount - 2))
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
                                if (readCount == 109)
                                {
                                    try
                                    {
                                        if (project != null)
                                        {
                                            int station = BitConverter.ToInt32(new byte[] { bytes[70], bytes[71], bytes[72], bytes[73] }, 0);
                                            string name_commnad = Encoding.Unicode.GetString(bytes, 74, 32).TrimEnd(new char[] { '\0' });
                                            project.TestFile.ListTU.Add(new CommandTU() { NameTU = name_commnad, StationNumber = station });
                                            if (!project.TestFile.IsStart)
                                                project.TestFile.StartTest();
                                            //
                                            var answer = FrameParser.MakeFrame(ImpulsesClient.PackCommandAnswer(station, name_commnad), false);
                                            stream.Write(answer, 0, answer.Length);
                                        }
                                    }
                                    catch { }
                                }
                            }
                            //отправляем данные тс
                            if (station_number.Count > 0)
                            {
                                byte[] answer_ty_server = serverimpuls.OutPutData(station_number);
                                // Отправляем данные на sdm сервер
                                stream.Write(answer_ty_server, 0, answer_ty_server.Length);
                            }
                        }
                    }
                    else
                    {
                        if (DateTime.Now - lastCommunicatedTime > echoMessageTimeout)
                        {
                            lastCommunicatedTime = DateTime.Now;
                            isStop = true;
                            if (OnError != null)
                                OnError(this, new Exception(string.Format("Client {0} disconnected", client.Client.RemoteEndPoint.ToString())));
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (OnError != null)
                {
                    OnError(this, e);
                }
            }
        }

    }
}

