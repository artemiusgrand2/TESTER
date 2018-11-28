
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using sdm.diagnostic_section_model.client_impulses.requests;


namespace sdm.diagnostic_section_model.client_impulses
{
	/// <summary>
	/// Служит для получения значения импульсов с сервера.
	/// </summary>
    public class ImpulsesClient
    {	
		public delegate void GetNewDataEventHandler();

        public static event GetNewDataEventHandler NewData;

	    public delegate void GetNewStateEventHandler();

	    public event GetNewStateEventHandler NewState;
        /// <summary>
        /// событие на подключение к серверу или при потери связи
        /// </summary>
        public static event GetNewStateEventHandler ConnectDisconnectionServer;
        /// <summary>
        /// подключен ли сервер
        /// </summary>
        public static bool Connect
        {
            get
            {
                return _connect;
            }
            set
            {
                _connect = value;
            }
        }
        /// <summary>
        /// является ли подключение первым
        /// </summary>
        private bool _firstconnect = true;

        static bool _connect = false;
     
        private static string _stateDescription = null;

		private DateTime m_date;
		
		private int interval = 1000;
		
		/// <summary>
		/// Буфер запроса на получение таблиц импульсов
		/// </summary>
		private byte[] m_impulsesRequest = null;
		
		/// <summary>
		/// Хранилище текущих данных !!!
		/// </summary>
		private DataContainer m_data;
		public DataContainer data
		{
			get
			{
				return m_data;
			}
		}


        public string StateDescription()
        {
            string stTemp = _stateDescription;
            return stTemp;
        }

		/// <summary>
		/// Соединение через порт или сокет
		/// </summary>
		private UniConnection m_client;
		
		/// <summary>
		/// Таймер вызова функции получения данных.
		/// </summary>
		private Timer m_workTimer;
		
		/// <summary>
		/// Признак работы таймера.
		/// </summary>
		private bool m_isTimerInWork;
		
		/// <summary>
		/// Признак закрытия программы.
		/// </summary>
		private static bool m_closed;

		/// <summary>
		/// Адрес сервера импульсов.
		/// </summary>
		private string m_connectionString;

	    public static void Set_new_state_from_user()
	    {
			//if(m_closed)
	            if (NewData != null)
	                NewData();
	    }


	    public static bool Closed
	    {
            get { return m_closed; }
	    }

	    /// <summary>
		/// Обрабатывает файл конфигурации.
		/// Создает список станций. Читает таблицы импульсов.
		/// </summary>
		/// <param name="configFileName">
		/// A <see cref="System.String"/>
		/// </param>
        public ImpulsesClient(sdm.diagnostic_section_model.StationRecord[] inp_station_records, string server_address, string tables_path)
        {
			m_data = new DataContainer();
            
			 m_connectionString = server_address;
			
			m_client = new UniConnection();
			
			m_data.LoadStationsData(inp_station_records, tables_path);
            

			System.Console.WriteLine("Загружено {0} станций.", m_data.Stations.Count);
//			DiagnosticManager.Instance.new_message_impulses_server(string.Format("Загружено {0} станций.", m_data.Stations.Count));
			m_workTimer = new Timer(GetImpulsesTimerFunc);
		}

		/// <summary>
		/// Кнопка пуск ! Красненькая !
		/// </summary>
		public void Start()
		{
		    m_closed = false;
			go();
		}

        public void Set_changing_state()
        {
            if (NewState != null)
                NewState();
            //ImpulseStateChangingHolder.Instance.ImpulsesStateCheck(_stateDescription);
        }
		
		public void Wait()
		{
			while(m_workTimer != null)
			{
				Thread.Sleep(50);
			}
		}
		
		/// <summary>
		/// Кнопка стоп ! Почему-то тоже красненькая !
		/// </summary>
		public void Stop()
        {
			m_closed = true;
		}
		
        /// <summary>
        /// Запуск работы.
        /// </summary>
        /// <param name="obj">некий объект</param>
        private void go()
        {
			System.Console.WriteLine("Старт получения данных с сервера.");

            _stateDescription = "Старт получения данных с сервера.";
            Set_changing_state();


            // А нам это надо ?
            if (m_data.Stations.Count < 1)
			{
				System.Console.Error.WriteLine("Найдено 0 станций. Выход из потока получения данных с сервера.");
//				DiagnosticManager.Instance.new_message_impulses_server("Найдено 0 станций. Выход из потока получения данных с сервера.");
                return;
			}
            
			//Формирую запрос на таблицы
            m_impulsesRequest = new byte[TablesListRequestHeader.Size + m_data.Stations.Count * 4];
            int count_st = 0;
			unsafe
            {
                TablesListRequestHeader* impulsesRequestHeader;
                fixed (byte* pRequest = m_impulsesRequest)
                {
                    impulsesRequestHeader = (TablesListRequestHeader*)pRequest;
                }
                impulsesRequestHeader->Header.PacketSize = (short)m_impulsesRequest.Length;
                impulsesRequestHeader->Header.PacketType = (int)Request.ListOfTables;
                impulsesRequestHeader->StationsCount = (short)m_data.Stations.Count;
                int* station = (int*)&impulsesRequestHeader[1];
                foreach (KeyValuePair<int, Station> st in m_data.Stations)
                {
                    *station = st.Key;
                    station++;
					count_st++;
                }
			}

			Console.WriteLine("Request for {0} stations size={1}", count_st, m_impulsesRequest.Length);

            m_impulsesRequest = FrameParser.MakeFrame(m_impulsesRequest, false);
            
			
			m_workTimer.Change(0, interval);
        }

        protected void OnGetNewData()
        {
//			foreach (Station _station in data.Stations.Values)
//			{
//				m_show_station(_station);
//			}
			if (NewData != null)
            	NewData();
            //ImpulseStateChangingHolder.Instance.Changing();
            _stateDescription = "Обновлены таблицы";
            Set_changing_state();
        }
		private bool p_closed = false;
		
		private void GetImpulsesTimerFunc(object obj)
		{
			try
			{
                //_stateDescription = "Получение импульсов";
                //Set_changing_state();
				if(m_isTimerInWork)
				{
					System.Console.Error.WriteLine("Попытка запуска второго экземпляра функции таймера." +
					               					" Предыдущий запуск неуспел отработать.");
				    _stateDescription = "Переподключение клиента ";
                    Set_changing_state();
//					DiagnosticManager.Instance.new_message_impulses_server("Попытка переподключить клиент сервера импульсов");
					return;
				}
				
				m_isTimerInWork = true;
				
				if(m_closed || p_closed)
				{
					m_workTimer.Change(Timeout.Infinite, interval);
					m_client.Close();
					//m_workTimer = null;
					System.Console.WriteLine("Конец получения данных с сервера.");
				    _stateDescription = "Клиент завершил работу";
                    Set_changing_state();
//					DiagnosticManager.Instance.new_message_impulses_server("Конец получения данных с сервера.");
					m_isTimerInWork = false;
					return;
				}
				
				//если потерял связь с сервером, то надо заново соединятся
				if(!m_client.IsOpen)
				{
                    try
                    {
                        m_client.Open(m_connectionString);
                        //
                        if (!Connect)
                        {
                            Connect = true;
                            if (ConnectDisconnectionServer != null)
                                ConnectDisconnectionServer();
                        }
                        //
                        System.Console.WriteLine("Соединение с сервером импульсов {0} установлено", m_connectionString);
                        _stateDescription = string.Format("Соединение с {0} установлено", m_connectionString);
                        Set_changing_state();
                    }
                    catch(Exception error)
                    {
                        if (_firstconnect)
                        {
                            Connect = false;
                            _firstconnect = false;
                            if (ConnectDisconnectionServer != null)
                                ConnectDisconnectionServer();
                        }
                        else
                        {
                            if (Connect)
                            {
                                Connect = false;
                                if (ConnectDisconnectionServer != null)
                                    ConnectDisconnectionServer();
                            }
                        }
                    }
				}
	
				//читаю данные
				if(m_client.IsOpen)
				{
					GetImpulses();
				}
				
				m_isTimerInWork = false;
				
				//m_logger.DebugFormat("Время работы цикла: {0}", DateTime.Now - start_time);
			}
			catch(Exception ex)
			{
//				DiagnosticManager.Instance.new_message_impulses_server(string.Format("Ошибка работы таймера {0}", ex.ToString()));
				System.Console.WriteLine(ex);
			}
			finally
			{
				m_isTimerInWork = false;
			}
		}
		
		/// <summary>
		/// 
		/// Прочитать таблицы импульсов
		/// </summary>
		private void GetImpulses()
		{
			if(m_impulsesRequest == null)
				return;

			//отправляю запрос
			try
			{
				if(m_client.Write(m_impulsesRequest, 0, m_impulsesRequest.Length) == 0)
				{
					// закрывать соединение не надо, т.к. 0 это не ошибка,
					// ошибки генерируют исключение
					return;
				}
			}
			catch(UniConnectionException e)
			{
//				DiagnosticManager.Instance.new_message_impulses_server(string.Format("Ошибка отправки запроса {0}", e.ToString()));
                if (e.Error != UniConnectionError.TimedOut)
                {
                    m_client.Close();
                }
				return;
			}
			//читаю ответ
			byte[] buffer = new byte[32 * 1024];
			FrameParser parser = new FrameParser();
			int receivedStations = 0;
			ManualTimer readTimeout = new ManualTimer(5000);
			do
			{
				if(!m_client.IsOpen)
					break;

				int readed = 0;
				try
				{
					readed = m_client.Read(buffer, 0, buffer.Length);
					m_date = DateTime.Now;
				}
				catch(UniConnectionException e)
				{
//					DiagnosticManager.Instance.new_message_impulses_server(string.Format("Ошибка чтения импульсов {0}", e.ToString()));
					if(e.Error != UniConnectionError.TimedOut)
						m_client.Close();
					return;
				}
				
				// если долго не было данных, то прервать ожидание
				// это нужно, т.к. Read может вернуть 0 (для посл. порта)
                //if(readTimeout.Timeout)
                //    break;
				
				// обрабатываю полученные данные
				for(int i = 0; i < readed; i++)
				{
					parser.Parse(buffer[i]);
					if(parser.IsFrameReady)
					{
						ParseTablesAnswer(parser.GetFrame());
						receivedStations++;
						// сбрасываю только при получении станции
						readTimeout.Reset();
					}
				}
			//m_logger.DebugFormat("Кол-во полученных станций при запросе: {0}", receivedStations);
			} while(receivedStations < m_data.Stations.Count);
			OnGetNewData();
			//сбросить таймаут
			readTimeout.Reset();
		}
		
        /// <summary>
        /// Собрать запрос на отправление импульса
        /// </summary>
        /// <param name="stationCode">Код станции на которую отправлялся импульс</param>
        /// <param name="impulse">Название импульса</param>
        /// <param name="error">Результат отправления</param>
        /// <returns>Буфер с результатом отправления (не упакованный)</returns>
        public static unsafe byte[] PackCommandAnswer(int stationCode, string impulse)
        {
            byte[] answer = new byte[CommandRequest.Size];
            fixed (byte* pAnswer = answer)
            {
                CommandRequest* result = (CommandRequest*)pAnswer;
                result->Header.PacketType = (int)Request.Command;
                result->Header.PacketSize = CommandRequest.Size;
                //в качестве идентификатора использую имя компьютера
                byte[] tmp = Encoding.Unicode.GetBytes(System.Net.Dns.GetHostName());
                int count = (tmp.Length < CommandRequest.SenderIDLength) ? (tmp.Length) : (CommandRequest.SenderIDLength);
                for (int i = 0; i < count; i++)
                    result->SenderID[i] = tmp[i];

                result->StationID = stationCode;
                result->CommandValue = (short)0x0000;
                //заполняю команду
                tmp = Encoding.Unicode.GetBytes(impulse);
                count = (tmp.Length < CommandRequest.CommandIDLength) ? (tmp.Length) : (CommandRequest.CommandIDLength);
                for (int i = 0; i < count; i++)
                    result->CommandID[i] = tmp[i];
            }
            return answer;
        }

        /// <summary>
        /// Преобразовать указатель в строку
        /// </summary>
        /// <param name="pointer">Указатель на массив байт</param>
        /// <param name="length">Длинна массива</param>
        /// <param name="encode">Кодировка символов</param>
        /// <returns>Преобразованная строка</returns>
        private unsafe string PointerToString(byte* pointer, int length, Encoding encode)
        {
            byte[] tmp = new byte[length];
            fixed (byte* ptmp = tmp)
            {
                CopyBytes(pointer, ptmp, length);
            }
            return encode.GetString(tmp).Trim('\0');

        }

        /// <summary>
        /// Скопировать массив байт
        /// </summary>
        /// <param name="pSrc">Исходный массив байт</param>
        /// <param name="pDst">Результирующий массив байт</param>
        /// <param name="count">Количество копируемых байт</param>
        private unsafe void CopyBytes(byte* pSrc, byte* pDst, int count)
        {
            byte* ps = pSrc;
            byte* pd = pDst;
            // Loop over the count in blocks of 4 bytes, copying an integer (4 bytes) at a time:
            for (int i = 0; i < count / 4; i++)
            {
                *((int*)pd) = *((int*)ps);
                pd += 4;
                ps += 4;
            }

            // Complete the copy by moving any bytes that weren't moved in blocks of 4:
            for (int i = 0; i < count % 4; i++)
            {
                *pd = *ps;
                pd++;
                ps++;
            }
        }

        /// <summary>
        /// Заполнить состояния импульсов принятыми
        /// значениями из пакета.
        /// </summary>
        /// <param name="answerTable">Пакет байт с значениями импульсов.</param>
        public void ParseTablesAnswer(byte[] answerTable)
        {
			ImpulsesAnswer answer = TableParser.ParseTablesAnswer(answerTable);
			
			if (answer == null)
                return;
			
			ImpulseState[] states_ts = new ImpulseState[answer.Header.TSCount];
			ImpulseState[] states_tu = new ImpulseState[answer.Header.TUCount];
			
			if (m_data.Stations.ContainsKey(answer.Header.StationID))
            {
				for (int k = 0; k < answer.Header.TSCount; k++)
                {
					states_ts[k] = (ImpulseState)answer.TsImpulses[k];
                }
				for (int k = 0; k < answer.Header.TUCount; k++)
                {
					states_tu[k] = (ImpulseState)answer.TuImpulses[k];
                }
                m_data.Stations[answer.Header.StationID].TS.RealCountImpuls = answer.TsImpulses.Length;
                m_data.Stations[answer.Header.StationID].TU.RealCountImpuls = answer.TuImpulses.Length;
                //
                m_data.Stations[answer.Header.StationID].SetImpulsesStates(states_ts, m_date, ImpulsesTableType.TS);
				m_data.Stations[answer.Header.StationID].SetImpulsesStates(states_tu, m_date, ImpulsesTableType.TU);
			}
        }
//		public static void m_show_station(Station inp_station)
//		{
//			System.Console.WriteLine("********************TS*********************");
//			int _i = 0;
//			foreach (string state in inp_station.TS.Names)
//			{
//				if (_i%5 == 0)
//					System.Console.WriteLine();
//				System.Console.Write(state + " ");
//				_i ++;
//			}
//			System.Console.WriteLine();
//			System.Console.WriteLine("*******************************************");
//			System.Console.WriteLine("********************TU*********************");
//			_i = 0;
//			foreach (string state in inp_station.TU.Names)
//			{
//				if (_i%5 == 0)
//					System.Console.WriteLine();
//				System.Console.Write(state + " ");
//				_i ++;
//			}
//			System.Console.WriteLine();
//		}
//		private static void m_show_tu_states(ImpulseState[] states)
//		{
//			System.Console.WriteLine("*******************************************");
//			int _i = 0;
//			foreach (ImpulseState state in states)
//			{
//				if (_i%2 == 2)
//					System.Console.WriteLine();
//				System.Console.Write(state + " ");
//				_i ++;
//			}
//			System.Console.WriteLine();
//			System.Console.WriteLine("*******************************************");
//		}
		
		/// <summary>
		/// Отправить импульс ТУ.
		/// </summary>
		/// <param name="request">Буфер с запросом на отправку импульса</param>
		/// <returns>Буфер с результатом отправки импульса</returns>
		public unsafe string SendImpulse(string impulse_name, int st_code, ImpulseState state)
		{
			if(!m_client.IsOpen)
			{
				return MakeCommandAnswer(impulse_name, st_code, RequestError.IOError);
			}

//			//получаю название импульса
//			string impulse = impulse_name;
			
			//проверяю есть ли такая станция
			TableImpulses table = null;
			foreach(Station st in m_data.Stations.Values)
			{
				if(st.Code == st_code)
				{
					table = st.TU;
					break;
				}
			}
			
			if(table == null)
			{
				return MakeCommandAnswer(impulse_name, st_code, RequestError.UnknownStation);
			}

			
			bool is_finded_imp = false;
			//ищу нужный импульс
			foreach(string name in table.Names)
			{
				if(name == impulse_name)
				{
					is_finded_imp = true;
					break;
				}
			}
			
			if(!is_finded_imp)
				return MakeCommandAnswer(impulse_name, st_code, RequestError.UnknownCommand);

			
			byte[] requestBuffer = new byte[CommandRequest.Size];
			CommandRequest* pCommandRequest;
			fixed(byte* pRequest = requestBuffer)
			{
				pCommandRequest = (CommandRequest*)pRequest;
			}
			pCommandRequest->Header.PacketSize = CommandRequest.Size;
			pCommandRequest->Header.PacketType = 0x0102; // Command type
			pCommandRequest->StationID = st_code;
			pCommandRequest->CommandValue = (short)state;
			
			byte[] comand_id = Encoding.Unicode.GetBytes(impulse_name);
			int count = (comand_id.Length < CommandRequest.CommandIDLength) ? (comand_id.Length) : (CommandRequest.CommandIDLength);
			for(int i = 0; i < count; i++)
				pCommandRequest->CommandID[i] = comand_id[i];
			
			// в качестве идентификатора использую имя компьютера
			byte[] comp_id = Encoding.Unicode.GetBytes(System.Net.Dns.GetHostName());
			count = (comp_id.Length < CommandRequest.SenderIDLength) ? (comp_id.Length) : (CommandRequest.SenderIDLength);
			for(int i = 0; i < count; i++)
				pCommandRequest->SenderID[i] = comp_id[i];
			
			
			// отправляю запрос
			byte[] answer = null;
			answer = SendRequest(requestBuffer, true);
			
			if(answer == null)
				return MakeCommandAnswer(impulse_name, st_code, RequestError.IOError);
			else
			{
				fixed(byte* pBuff = answer)
				{
					CommandRequest* pAnswer = (CommandRequest*)pBuff;
					return MakeCommandAnswer(impulse_name, st_code, (RequestError)pAnswer->CommandValue);
				}
			}
		}
		
		/// <summary>
		/// Послать запрос
		/// </summary>
		/// <param name="request">Запрос</param>
		/// <param name="waitAnswer">Ждать ли ответа</param>
		/// <returns>Ответ на запрос, либо null если ответ не получен или его не надо ждать</returns>
		public byte[] SendRequest(byte[] request, bool waitAnswer)
		{
			if(!m_client.IsOpen)
				return null;
			
			byte[] frame = FrameParser.MakeFrame(request, false);
			//очищаю входной буфер
			//отправляю запрос
			int written = 0;
			try
			{
				written = m_client.Write(frame, 0, frame.Length);
			}
			catch
			{
				return null;
			}
			
			if(written < frame.Length)
				return null;

			if(!waitAnswer)
				return null;

			//читаю только один пакет
			byte[] inBuffer = new byte[4 * 1024];
			ManualTimer readTimeout = new ManualTimer(5000);

			int readed = 0;
			FrameParser parser = new FrameParser();
			while(!readTimeout.Timeout)
			{
				try
				{
					readed = m_client.Read(inBuffer, 0, inBuffer.Length);
				}
				catch
				{
					return null;
				}
				if(readed != 0)
					//сбросить таймаут
					readTimeout.Reset();
				
				//обрабатываю полученные данные
				for(int i = 0; i < readed; i++)
				{
					parser.Parse(inBuffer[i]);
					if(parser.IsFrameReady)
						return parser.GetFrame();
				}
			}
			return null;
		}
		
		private unsafe string MakeCommandAnswer(string command_id, int station_id, RequestError result)
		{
			return String.Format("{0} - {1}: {2}", station_id, command_id, result);

		}
    }
	
		/// <summary>
	/// Тип таблицы импульсов
	/// </summary>
	public enum ImpulsesTableType
	{
		/// <summary>
		/// Таблица ТС
		/// </summary>
		TS = 0,

		/// <summary>
		/// Таблица ТУ
		/// </summary>
		TU = 1,

		/// <summary>
		/// Таблица состояния каналов
		/// </summary>
		Channels = 2,

		/// <summary>
		/// Таблица состояния блоков
		/// </summary>
		Blocks = 3
	}
}
