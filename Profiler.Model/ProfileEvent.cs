using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Ijw.Profiler.Model
{
	enum Opcode : byte
	{
		ThreadTransition = 1,
		EnterFunction = 2,
		LeaveFunction = 3,
		SetClockFrequency = 4,
		LeaveViaTailCall = 5,
	}

	class ProfileEvent
	{
		public readonly Opcode opcode;
		public readonly uint id;
		public readonly ulong timestamp;

		ProfileEvent(BinaryReader reader)
		{
			opcode = (Opcode)reader.ReadByte();
			id = reader.ReadUInt32();
			timestamp = reader.ReadUInt64();
		}

		static ProfileEvent Next(BinaryReader reader)
		{
			try { return new ProfileEvent(reader); }
			catch { return null; }
		}

		public static IEnumerable<ProfileEvent> GetEvents(BinaryReader reader)
		{
			for (; ; )
			{
				ProfileEvent e = ProfileEvent.Next(reader);
				if (e == null)
					yield break;
				else
					yield return e;
			}
		}
	}
}
