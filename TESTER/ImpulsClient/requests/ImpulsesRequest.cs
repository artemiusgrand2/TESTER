using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace sdm.diagnostic_section_model.client_impulses.requests
{
	[StructLayout(LayoutKind.Explicit, Size=8)]
	unsafe struct ImpulsesRequest
	{
		public const int Size = 8;

		[FieldOffset(0)]
		public RequestHeader Header;

		[FieldOffset(4)]
		public int StationID;
	}
}
