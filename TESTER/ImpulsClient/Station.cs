using System;
using System.Collections.Generic;
using System.Text;

namespace sdm.diagnostic_section_model.client_impulses
{
	/// <summary>
	/// Станция
	/// </summary>
	public class Station
	{
		#region Данные
		/// <summary>
		/// Код станции
		/// </summary>
		private int m_code;

		/// <summary>
		/// Название станции
		/// </summary>
		private string m_name;

        /// <summary>
        /// Список импульсов ТС станции.
        /// </summary>
        private TableImpulses m_ts;
		
		/// <summary>
        /// Список импульсов ТУ станции.
        /// </summary>
        private TableImpulses m_tu;
		#endregion

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="name">Название станции</param>
		/// <param name="code">Код станции</param>
		public Station(string name, int code)
		{
			m_name = name;
			m_code = code;
            m_ts = null;
		}

		#region Свойства
		public string Name
		{
			get
			{
				return m_name;
			}
		}

		public int Code
		{
			get
			{
				return m_code;
			}
		}

        public TableImpulses TS
        {
            get
            {
                return m_ts;
            }
        }

		public TableImpulses TU
        {
            get
            {
                return m_tu;
            }
        }
        #endregion

		#region Методы
		public override string ToString()
		{
			return string.Format("{0} ({1})", m_name, m_code);
		}

		/// <summary>
		/// Загрузить объекты станции.
		/// </summary>
		public void LoadData(ImpulseRecord[] ts_impulses, ImpulseRecord[] tu_impulses)
		{
            #region загружаю таблицу импульсов для станции
            try
            {
                if (ts_impulses != null)
                    m_ts = new TableImpulses(ts_impulses, m_code, false);
                else
                    m_ts = new TableImpulses(new ImpulseRecord[0], m_code, false);
                //
                if(tu_impulses != null)
                    m_tu = new TableImpulses(tu_impulses, m_code, true);
                else
                    m_tu = new TableImpulses(new ImpulseRecord[0], m_code, true);
            }
            catch { }
			#endregion
		}

		/// <summary>
		/// Установить значение импульса.
		/// </summary>
		/// <param name="impulse">Название импульса.</param>
		/// <param name="state">Состояние импульса.</param>
		/// <returns>true - если импульс найден и состояние установлено, иначе false.</returns>
		public void SetImpulsesStates(ImpulseState[] states, DateTime time_changed, ImpulsesTableType type)
		{
			if (type == ImpulsesTableType.TS && m_ts != null)
				m_ts.SetStates(states, time_changed);
            else if (type == ImpulsesTableType.TU && m_tu != null)
				m_tu.SetStates(states, time_changed);
		}

		/// <summary>
		/// Установить одинаковые значение импульсов.
		/// </summary>
		/// <param name="states">Состояния импульса.</param>
		/*public void SetImpulsesStates(ImpulseState state, DateTime time_changed)
		{
			m_ti.SetStates(state, time_changed);
		}*/
		
		/// <summary>
		/// Гет время получения последних данных
		/// </summary>
		/// <returns>
		/// Время получения последних данных <see cref="DateTime"/>
		/// </returns>
		public DateTime get_time_changed()
		{
			return TS.get_time();
		}
		
		#endregion
	}
}
