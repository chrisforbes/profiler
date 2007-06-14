using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ProfilerUi
{
	enum Opcode : byte
	{
		ThreadTransition = 1,
		EnterFunction = 2,
		LeaveFunction = 3,
		SetClockFrequency = 4,
		LeaveViaTailCall = 5,
	}

	class CallTree
	{
		public Dictionary<uint, Thread> threads = new Dictionary<uint, Thread>();

		public CallTree(string filename, FunctionNameProvider names)
		{
			Stream s = new FileStream(filename, FileMode.Open, FileAccess.Read);
			BinaryReader reader = new BinaryReader(s);

			Thread currentThread = null;

			ulong frequency = 0;

			try
			{
				while (true)
				{
					Opcode o = (Opcode)reader.ReadByte();
					uint id = reader.ReadUInt32();
					ulong timestamp = reader.ReadUInt64();

					switch (o)
					{
						case Opcode.SetClockFrequency:
							frequency = timestamp;
							break;

						case Opcode.ThreadTransition:
							Thread previous = currentThread;
							if (previous != null)
							{
								ulong timeTaken = timestamp - previous.lastEntryTime;
								double milliseconds = 1000.0 * (double)timeTaken / (double)frequency;

								previous.time += milliseconds;
							}

							if (!threads.TryGetValue(id, out currentThread))
								threads.Add(id, currentThread = new Thread((int)id));

							currentThread.lastEntryTime = timestamp;
							break;

						case Opcode.EnterFunction:
							Function f;

							Dictionary<uint, Function> dict = (currentThread.current == null) 
								? currentThread.roots : currentThread.current.children;

							if (!dict.TryGetValue(id, out f))
								dict.Add(id, f = new Function(currentThread.current, names.GetName(id), id));

							currentThread.current = f;
							++f.calls;
							currentThread.entryTimes.Push(timestamp);

							break;

						case Opcode.LeaveFunction:
							if (currentThread.current.id != id)
							{
								MessageBox.Show("trying to leave function not in; current=" +
									names.GetName(currentThread.current.id) + "\n tried=" + names.GetName(id));
							}

							if (currentThread.current != null)
							{
								ulong entryTime = currentThread.entryTimes.Pop();
								ulong timeTaken = timestamp - entryTime;
								double milliseconds = 1000.0 * (double)timeTaken / (double)frequency;

								currentThread.current.time += milliseconds;
								currentThread.current = currentThread.current.caller;
							}
							break;
					}
				}
			}
			catch (EndOfStreamException) { }
		}
	}
}
