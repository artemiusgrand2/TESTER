using System;
using System.Collections.Generic;
using System.Text;

namespace sdm.diagnostic_section_model.client_impulses.requests
{
	enum Request
	{
		StationTables = 0x0101,
		Command = 0x0102,
		ListOfTables = 0x0103
	}

	enum Answer
	{
		StationTables = 1,
		Command = 2,
		Error = -1
	}

	enum RequestError
	{
		UnexpectedError = -1,
		Successful = 0,
		UnknownRequest = 1,
		AccessDenied = 2,
		UnknownStation = 3,
		UnknownCommand = 4,
		IOError = 5
	}

	enum Broadcast
	{
		ImpulsesTable = 1,
		Command = 3,
		CommandAnswer = 4
	}
}
