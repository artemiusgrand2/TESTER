using System;
using System.Collections.Generic;
using System.Text;

namespace sdm.diagnostic_section_model.client_impulses
{
	public class DataContainer
	{
		private Dictionary<int, Station> m_stations;
		
		public DataContainer()
		{
			m_stations = new Dictionary<int, Station>();
		}

		public Dictionary<int, Station> Stations
		{
			get
			{
				return m_stations;
			}
		}

		public bool LoadStationsData(sdm.diagnostic_section_model.StationRecord[] inp_station_records, string tables_path)
		{
            foreach (sdm.diagnostic_section_model.StationRecord st_config in inp_station_records)
 			{
                Station st = new Station(st_config.Name, st_config.Code);
 				ImpulseRecord[] ts_impulses = null;
				ImpulseRecord[] tu_impulses = null;
				try
				{
                    TableLoader.GetStdImpulses(tables_path, st.Code, false, out ts_impulses);
					TableLoader.GetStdImpulses(tables_path, st.Code, true, out tu_impulses);
                }
				catch(SystemException ex)
				{
                    System.Windows.MessageBox.Show(ex.Message);
				//	System.Console.Error.WriteLine("TI for {0} not found. {1}", st.Code, ex.Message);
					continue;
				}
				if(ts_impulses == null)
				{
                    System.Windows.MessageBox.Show(string.Format("TI for {0} not found.", st.Code));
					continue;
				}
				
				try
				{
					st.LoadData(ts_impulses, tu_impulses);
				}
				catch(Exception ex)
				{
					System.Console.Error.WriteLine("Can't load TI for {0}. {1}", st.Code, ex.Message);
					continue;
				}
 				try
 				{
					m_stations.Add(st.Code, st);
				}
				catch(ArgumentException ex)
				{
					System.Console.Error.WriteLine(ex.Message);
					continue;
				}
			}
			if(m_stations.Count == 0)
				return false;
			else
				return true;
		}

 /*       public void LoadProtokol(ReportRecord[] records)
		{
			int currentMinute = 0;
			int currentSecond = 0;

            for(int i = 0; i < records.Length; i++)
			{
                if (currentMinute != records[i].Time.Minute || currentSecond != records[i].Time.Second)
				{
                    
                    DateTime dt = new DateTime(DateTime.Now.Year, records[i].Time.Month, records[i].Time.Day,
                        records[i].Time.Hour, currentMinute, currentSecond);
					foreach(Station st in m_stations.Values)
                        st.CalculateVoltages(dt);
                    currentMinute = records[i].Time.Minute;
                    currentSecond = records[i].Time.Second;
				}
				foreach(Station st in m_stations.Values)
				{
					if(st.Name == records[i].StationName)
					{
						st.SetImpulseState(records[i].ImpulseName, (ImpulseState)records[i].ImpulseState);
                        break;
					}
				}
			}
		}
		 */
	}
}
