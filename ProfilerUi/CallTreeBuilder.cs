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

			ulong frequency = 0, timestamp = 0;

			try
			{
				while (true)
				{
					Opcode o = (Opcode)reader.ReadByte();
					uint id = reader.ReadUInt32();
					timestamp = reader.ReadUInt64();

					switch (o)
					{
						case Opcode.SetClockFrequency:
							frequency = timestamp;
							break;

						case Opcode.ThreadTransition:
							if (currentThread != null)
								currentThread.time += Activation.GetTime(currentThread.lastEntryTime, timestamp, frequency);

							if (!threads.TryGetValue(id, out currentThread))
								threads.Add(id, currentThread = new Thread((int)id));

							currentThread.lastEntryTime = timestamp;
							break;

						case Opcode.EnterFunction:
							Function f;

							Dictionary<uint, Function> dict = (currentThread.activations.Count == 0) 
								? currentThread.roots : currentThread.activations.Peek().Function.children;

							if (!dict.TryGetValue(id, out f))
								dict.Add(id, f = new Function(names.GetName(id)));

							currentThread.activations.Push(new Activation(f, timestamp));
							break;

						case Opcode.LeaveFunction:
							if (currentThread.activations.Count == 0)
							{
								MessageBox.Show("no activation to end");
								break;
							}

							Activation a = currentThread.activations.Pop();
							a.Complete(timestamp, frequency);
							break;
					}
				}
			}
			catch (EndOfStreamException) { }

			//close dead activations
			foreach (Thread t in threads.Values)
			{
				while (t.activations.Count > 0)
				{
					Activation a = t.activations.Pop();
					a.Complete(timestamp, frequency);
				}
			}
		}
	}
}
