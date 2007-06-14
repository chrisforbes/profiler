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

			Thread currentThread = null;

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

								if (currentThread.current == null)
									currentThread.root = currentThread.current = new Function(null);
								else
								{
									Function f = currentThread.current.children[funcId];
									++f.calls;
								}
							}
							break;

						case Opcode.LeaveFunction:
							{
								uint funcId = reader.ReadUInt32();

								if (currentThread.current != null)
									currentThread.current = currentThread.current.caller;
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
		public Function(Function caller) { this.caller = caller; }

		public int calls = 1;
		public Dictionary<uint, Function> children = new Dictionary<uint, Function>();
		public Function caller;
	}

	class Thread
	{
		public Function root, current;
	}
}
