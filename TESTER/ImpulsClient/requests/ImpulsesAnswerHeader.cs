using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace sdm.diagnostic_section_model.client_impulses.requests
{
	[StructLayout(LayoutKind.Explicit, Size = 12)]
	unsafe struct ImpulsesAnswerHeader
	{
		public const int Size = 12;

		[FieldOffset(0)]
		public RequestHeader Header;

		[FieldOffset(4)]
		public int StationID;

		[FieldOffset(8)]
		public short TSCount;

		[FieldOffset(10)]
		public short TUCount;
	}
}
