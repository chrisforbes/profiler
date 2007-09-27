using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using IjwFramework.Ui;

namespace Ijw.Profiler.Model
{
	public interface IProfilerElement
	{
		Node CreateView(double totalTime);
		string TabTitle { get; }
		double TotalTime { get; }
		bool Interesting { get; }
		uint Id { get; }

		List<Function> CollectInvocations(uint functionId);
		void WriteTo(XmlWriter writer);
	}
}
