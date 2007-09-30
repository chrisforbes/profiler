using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using IjwFramework.Ui;
using IjwFramework.Collections;
using IjwFramework.Types;
using Ijw.Profiler.Core;

namespace Ijw.Profiler.Model
{
	public class CallTree
	{
		public Dictionary<uint, Thread> threads = new Dictionary<uint, Thread>();
		public Set<uint> allCalledFunctions = new Set<uint>();
		Activation<Thread> currentThread = null;
		Converter<uint,Name> names;
		ulong frequency;

		public CallTree(string filename, Converter<uint, Name> names, Action<float> progressCallback)
		{
			ulong finalTime = 0;
			float lastFrac = -1;

			this.names = names;

			using (Stream s = File.OpenRead(filename))
			using (BinaryReader reader = new BinaryReader(s))
			{
				foreach (ProfileEvent e in ProfileEvent.GetEvents(reader))
				{
					switch (e.opcode)
					{
						case Opcode.SetClockFrequency: 
							frequency = e.timestamp; break;
						
						case Opcode.ThreadTransition: 
							OnThreadTransition(e); break;
						
						case Opcode.EnterFunction: 
							OnEnterFunction(e); break;

						case Opcode.LeaveViaTailCall:
						case Opcode.LeaveFunction: 
							OnLeaveFunction(e); break;

						case Opcode.TimeBase:
							finalTime = e.timestamp; break;
					}

					finalTime = e.timestamp;

					float frac = (float)s.Position / (float)s.Length;
					if (frac >= lastFrac + 1e-2f)
					{
						progressCallback(frac);
						lastFrac = frac;
					}
				}
			}

			foreach (Thread t in threads.Values)
				while (t.activations.Count > 0)
					t.activations.Pop().Complete(finalTime, frequency);

			if (currentThread != null)
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

			Function parent = (t.activations.Count == 0)
				? null : t.activations.Peek().Target;

			IDictionary<uint, Function> dict = (parent == null)
				? t.roots : parent.children;

			Function f;
			Name name = names(e.id);

			if (!dict.TryGetValue(e.id, out f))
				dict.Add(e.id, f = new Function(e.id, name, parent));

			t.activations.Push(new Activation<Function>(f, e.timestamp));

			if (!allCalledFunctions.Contains(e.id))
				allCalledFunctions.Add(e.id);
		}

		void OnLeaveFunction(ProfileEvent e)
		{
			currentThread.Target.activations.Pop().Complete(e.timestamp, frequency);
		}

		public void WriteTo(XmlWriter writer)
		{
			writer.WriteStartElement("run");
			writer.WriteStartElement("trace");

			foreach (Thread t in threads.Values)
				t.WriteTo(writer);

			writer.WriteEndElement();
			writer.WriteStartElement("names");

			foreach (uint f in allCalledFunctions)
			{
				Name name = names(f);
				writer.WriteStartElement("func");
				writer.WriteAttributeString("id", f.ToString());
				writer.WriteAttributeString("name", name.MethodName);
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		public void WriteTo(string filename)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "  ";

			using (XmlWriter writer = XmlWriter.Create(filename, settings))
				WriteTo(writer);
		}

		public List<Function> CollectInvocations(uint functionId)
		{
			List<Function> invocations = new List<Function>();

			foreach (Thread t in threads.Values)
				invocations.AddRange(t.CollectInvocations(functionId));

			return invocations;
		}

		public CallerFunction GetBacktrace(Function f)
		{
			List<Function> invocations = CollectInvocations(f.Id);
			CallerFunction cf = null;
			foreach (Function inv in invocations)
				if (cf == null)
					cf = new CallerFunction(inv);
				else
					cf.Merge(inv);

			return cf;
		}

		public List<CallerFunction> GetHotspots( int count )
		{
			List<Pair<Function, double>> withOwnTime = new List<Pair<Function, double>>();

			foreach (uint f in allCalledFunctions)
			{
				double ownTime = 0.0;
				Function g = null;
				foreach (Function ff in CollectInvocations(f))
				{
					ownTime += ff.OwnTime;
					g = ff;
				}

				withOwnTime.Add(new Pair<Function, double>(g, ownTime));
			}

			withOwnTime.Sort(PairComparison.DescendingSecond);

			if (withOwnTime.Count > count)
				withOwnTime.RemoveRange(count, withOwnTime.Count - count);

			List<CallerFunction> result = new List<CallerFunction>();
			foreach (Pair<Function, double> p in withOwnTime)
				result.Add(GetBacktrace(p.First));

			return result;
		}
	}
}
