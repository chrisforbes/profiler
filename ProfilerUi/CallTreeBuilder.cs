using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ProfilerUi
{
	enum Opcode : byte
	{
		ThreadTransition = 1,
		EnterFunction = 2,
		LeaveFunction = 3,
	}

	class CallTree
	{
		public Dictionary<uint, Thread> threads = new Dictionary<uint, Thread>();

		public CallTree(string filename)
		{
			Stream s = new FileStream(filename, FileMode.Open, FileAccess.Read);
			BinaryReader reader = new BinaryReader(s);

			Thread currentThread;

			try
			{
				while (true)
				{
					Opcode o = (Opcode)reader.ReadByte();

					switch (o)
					{
						case Opcode.ThreadTransition:
							{
								uint threadId = reader.ReadUInt32();

								if (!threads.TryGetValue(threadId, out currentThread))
									threads.Add(threadId, currentThread = new Thread());
							}
							break;

						case Opcode.EnterFunction:
							{
								uint funcId = reader.ReadUInt32();
							}
							break;

						case Opcode.LeaveFunction:
							{
								uint funcId = reader.ReadUInt32();
							}
							break;
					}
				}
			}
			catch (EndOfStreamException) { }
		}
	}

	class Function
	{
		public int calls = 0;
		public Dictionary<uint, Function> children = new Dictionary<uint, Function>();
	}

	class Thread
	{
	}
}
