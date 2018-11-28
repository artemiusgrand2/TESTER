using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace sdm.diagnostic_section_model.client_impulses
{
	// Используется при использовании конфигурации сервера импульсов.
	[Serializable]
    [XmlRoot(ElementName = "Configuration")]
    public struct Configuration
	{
        [XmlElement(ElementName = "Settings")]
		public ProgramSettings Settings;
        [XmlArrayItem(ElementName = "Station")]
        [XmlArray(ElementName = "Stations")]
        public StationRecord[] Stations;
    }

    [Serializable]
    public struct StationRecord
    {
        [XmlElement(ElementName = "Name")]
        public string Name;
        [XmlElement(ElementName = "Code")]
        public int Code;
    }


	[Serializable]
	public struct ProgramSettings
	{
		[XmlElement(ElementName="TablesPath")]
		public string TablesPath;
		[XmlElement(ElementName = "ServerAddress")]
		public string ServerAddress;
	}

	public struct MeasurementObjectRecord
	{
		public string Name;
		public int Type;
		public string Impulse;
		public int Channel;
		public int MinVoltage;
		public int MaxVoltage;
        public int NoNameMember;
	}

	public struct MeasurementChannelRecord
	{
		public int Id;
		public List<string> Impulses;
	}

	public struct MeasurementObjectTypeRecord
	{
		public int Id;
		public string Name;
	};
}
