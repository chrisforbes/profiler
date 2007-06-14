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
						case Opcode.LeaveViaTailCall:
							MessageBox.Show("tailcall");
							break;

						case Opcode.SetClockFrequency:
							frequency = timestamp;
							break;

						case Opcode.ThreadTransition:
							if (!threads.TryGetValue(id, out currentThread))
								threads.Add(id, currentThread = new Thread((int)id));
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
							if (currentThread.current.id != id )
							{
								MessageBox.Show("trying to leave function not in; current=" + 
									names.GetName(currentThread.current.id) + "\n tried=" + names.GetName(id));
							}

							ulong entryTime = currentThread.entryTimes.Pop();
							ulong timeTaken = timestamp - entryTime;

							double milliseconds = 1000.0 * (double)timeTaken / (double)frequency;

							if (currentThread.current != null)
							{
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

	class Function
	{
		public Function(Function caller, string name, uint id)
		{
			this.caller = caller;
			this.name = name;
			this.id = id;
		}

		public int calls = 0;
		public Dictionary<uint, Function> children = new Dictionary<uint, Function>();
		public Function caller;
		public string name;
		public uint id;

		public double time;

		public TreeNode CreateView()
		{
			TreeNode n = new TreeNode(name + "    (" + calls + " calls)    " + time.ToString("F1") + "ms");
			n.Tag = this;

			foreach (Function f in children.Values)
				n.Nodes.Add(f.CreateView());

			return n;
		}
	}

	class Thread
	{
		public Dictionary<uint, Function> roots = new Dictionary<uint, Function>();
		public Function current;
		public Stack<ulong> entryTimes = new Stack<ulong>();

		readonly int id;

		public int Id { get { return id; } }

		public Thread(int id)
		{
			this.id = id;
		}

		public TreeNode CreateView()
		{
			TreeNode n = new TreeNode("Thread #" + id.ToString());
			n.Tag = this;

			foreach (Function f in roots.Values)
				n.Nodes.Add(f.CreateView());

			return n;
		}
	}
}
