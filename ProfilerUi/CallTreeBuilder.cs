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
					uint id = reader.ReadUInt32();

					switch (o)
					{
						case Opcode.ThreadTransition:
							{
								if (!threads.TryGetValue(id, out currentThread))
									threads.Add(id, currentThread = new Thread());
							}
							break;

						case Opcode.EnterFunction:
							{
								if (currentThread.current == null)
									currentThread.root = currentThread.current = new Function(null);
								else
								{
									Function f;
									if (!currentThread.current.children.TryGetValue(id, out f))
										currentThread.current.children.Add(id, currentThread.current = f = new Function(currentThread.current));
									++f.calls;
								}
							}
							break;

						case Opcode.LeaveFunction:
							{
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
		public Function(Function caller) { this.caller = caller; ++totalFunctions; }

		public int calls = 1;
		public Dictionary<uint, Function> children = new Dictionary<uint, Function>();
		public Function caller;

		public static int totalFunctions = 0;
	}

	class Thread
	{
		public Function root, current;
	}
}
