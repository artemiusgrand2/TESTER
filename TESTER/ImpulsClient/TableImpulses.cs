using System;
using System.Collections.Generic;

namespace sdm.diagnostic_section_model.client_impulses
{
	/// <summary>
	/// Таблица импульсов.
	/// </summary>
	public class TableImpulses
	{

		/// <summary>
		/// Время последнего изменения.
		/// </summary>
		private DateTime m_timeChanged;

		
		/// <summary>
		/// Массив состояний импульсов.
		/// </summary>
		private ImpulseState[] m_impulseStates;
		
		/// <summary>
		/// Имя импульса - индекс в массиве импульсов.
		/// </summary>
		private Dictionary<string, short> m_impulsesNames;

        /// <summary>
        /// реальное количество загруженных импульсов
        /// </summary>
        public int RealCountImpuls { get; set; }
		/// <summary>
		/// Заполняет таблицу импульсов.
		/// </summary>
		/// <param name="impulses">
		/// Массив записей с конфигурационном файле об импульсах. <see cref="ImpulseRecord[]"/>
		/// </param>
		public TableImpulses(ImpulseRecord[] impulses, int st_code, bool inp_is_tu)
		{
			m_timeChanged = DateTime.Now;
			
			m_impulseStates = new ImpulseState[impulses.Length];
			
			m_impulsesNames = new Dictionary<string, short>();
			for(short i = 0; i < impulses.Length; i++)
			{
				if (inp_is_tu)
					m_impulseStates[i] = ImpulseState.Taken;
				else
					m_impulseStates[i] = ImpulseState.UncontrolledState;
				try
				{
					m_impulsesNames.Add(impulses[i].Name,i);
				}
				catch(Exception)
				{
					System.Console.Error.WriteLine("Дублированное имя импульса '{0}'({1}[{2}])",
					                                                        impulses[i].Name, st_code, i);
					m_impulsesNames.Add(String.Format("{0}_{1}", impulses[i].Name, i),i);
				}
			}
		}


        public int GetCountProjectImpuls()
        {
            return m_impulseStates.Length;
        }

		public List<string> Names
		{
			get
			{
				return new List<string>(m_impulsesNames.Keys);
			}
		}

        /// <summary>
        /// Задать состояния импульсов.
        /// </summary>
        /// <param name="states">
        /// A <see cref="ImpulseState[]"/>
        /// </param>
        public void SetStates(ImpulseState[] states, DateTime time_changed)
        {
            int min_index = 0;
            min_index = m_impulseStates.Length > states.Length ? states.Length : m_impulseStates.Length;

            // ERROR Тут есть проблема - объем импульсов не совпадает с приходящим.
            m_timeChanged = time_changed;

            //if(m_impulseStates.Length != states.Length)
            //	m_logger.DebugFormat("{0}!={1}", m_impulseStates.Length, states.Length);

            for (int i = 0; i < min_index; i++)
            {
                m_impulseStates[i] = states[i];
            }
        }


        /// <summary>
        /// Задать состояния импульсов.
        /// </summary>
        /// <param name="state">
        /// A <see cref="ImpulseState"/>
        /// </param>
        public void SetAllStatesInTable(ImpulseState state, DateTime time_changed)
        {
			//if (ImpulsesClient.Closed)
			//{
                m_timeChanged = time_changed;

                for (int i = 0; i < m_impulseStates.Length; i++)
                {
                    m_impulseStates[i] = state;
                }
               // ImpulsesClient.Set_new_state_from_user();
			//}
        }

		/// <summary>
		/// Установить состояние импульса с определенным именем
		/// </summary>
		/// <param name="name_imp">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="state">
		/// A <see cref="ImpulseState"/>
		/// </param>
		/// <param name="time_change">
		/// A <see cref="DateTime"/>
		/// </param>
		public void set_state(string name_imp, ImpulseState state, DateTime time_change)
		{
			//if (ImpulsesClient.Closed)
			//{
                if (m_impulsesNames.ContainsKey(name_imp))
                {
                    Console.WriteLine("impulse {0} changed state to {1}", name_imp, state.ToString());
                    m_impulseStates[m_impulsesNames[name_imp]] = state;
                    m_timeChanged = time_change;
                    ImpulsesClient.Set_new_state_from_user();
                }
                else
                {
                    Console.WriteLine("Can't find impulse {0} on the station", name_imp);
                }
			//}
		}
		
		
		/// <summary>
		/// Задать состояния импульсов.
		/// </summary>
		/// <param name="states">
		/// A <see cref="ImpulseState[]"/>
		/// </param>
		/*public void SetStates(ImpulseState new_state, DateTime time_changed)
		{
			m_timeChanged = time_changed;
			
			for(int i = 0; i < m_impulseStates.Length; i++)
			{
				m_impulseStates[i] = new_state;
			}
		}*/
				
		/// <summary>
		/// Получить состояние импульса.
		/// </summary>
		/// <param name="index">
		/// Номер импульса <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// Текущее состояние импульса <see cref="ImpulseState"/>
		/// </returns>
		public ImpulseState GetState(int index)
		{
			return m_impulseStates[index];
		}
		
		
		/// <summary>
		/// Получить состояние импульса.
		/// </summary>
		/// <param name="name">
		/// Имя импульса <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// Текущее состояние импульса <see cref="ImpulseState"/>
		/// </returns>
		public ImpulseState GetState(string name)
		{
			return m_impulseStates[m_impulsesNames[name]];
		}
		
		/// <summary>
		/// Получить номер импульса.
		/// </summary>
		/// <param name="name">
		/// Имя импульса <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// Номер импульса <see cref="System.Int32"/>
		/// </returns>
		public short GetImpulseIndex(string name)
		{
			return m_impulsesNames[name];
		}
		
		/// <summary>
		/// Гет время получения последних данных
		/// </summary>
		/// <returns>
		/// Время получения последних данных <see cref="DateTime"/>
		/// </returns>
		public DateTime get_time()
		{
			return m_timeChanged;
		}
	}
}
