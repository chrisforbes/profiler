using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ProfilerUi
{
	class CallTree
	{
		public Dictionary<uint, Thread> threads = new Dictionary<uint, Thread>();
		Activation<Thread> currentThread = null;
		FunctionNameProvider names;
		ulong frequency;

		public CallTree(string filename, FunctionNameProvider names)
		{
			this.names = names;
			Stream s = new FileStream(filename, FileMode.Open, FileAccess.Read);
			BinaryReader reader = new BinaryReader(s);

			ulong finalTime = 0;

			foreach (ProfileEvent e in ProfileEvent.GetEvents(reader))
			{
				switch (e.opcode)
				{
					case Opcode.SetClockFrequency: frequency = e.timestamp; break;
					case Opcode.ThreadTransition: OnThreadTransition(e); break;
					case Opcode.EnterFunction: OnEnterFunction(e); break;
					case Opcode.LeaveFunction: OnLeaveFunction(e); break;
				}

				finalTime = e.timestamp;
			}

			foreach (Thread t in threads.Values)
				while (t.activations.Count > 0)
					t.activations.Pop().Complete(finalTime, frequency);

			currentThread.Complete(finalTime, frequency);
		}

		void OnThreadTransition(ProfileEvent e)
		{
			if (currentThread != null)
				currentThread.Complete(e.timestamp, frequency);

			Thread t;

			if (!threads.TryGetValue(e.id, out t))
				threads.Add(e.id, t = new Thread(e.id));

			currentThread = new Activation<Thread>(t, e.timestamp);
		}

		void OnEnterFunction(ProfileEvent e)
		{
			Thread t = currentThread.Target;
			Dictionary<uint, Function> dict = (t.activations.Count == 0)
				? t.roots : t.activations.Peek().Target.children;

			Function f;

			if (!dict.TryGetValue(e.id, out f))
				dict.Add(e.id, f = new Function(e.id, names.GetName(e.id)));

			t.activations.Push(new Activation<Function>(f, e.timestamp));
		}

		void OnLeaveFunction(ProfileEvent e)
		{
			currentThread.Target.activations.Pop().Complete(e.timestamp, frequency);
		}
	}
}
