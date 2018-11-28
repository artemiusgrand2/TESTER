using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace sdm.diagnostic_section_model.client_impulses.requests
{
	/// <summary>
	/// Заголовок запроса на список таблиц
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size=6)]
	struct TablesListRequestHeader
	{
		public const int Size = 6;

		[FieldOffset(0)]
		public RequestHeader Header;

		[FieldOffset(4)]
		public short StationsCount;
	}
}
