using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using IjwFramework.Ui.Tree;

namespace ProfilerUi
{
	interface IProfilerElement
	{
		Node CreateView(double totalTime);
		string TabTitle { get; }
		double TotalTime { get; }

		List<Function> CollectInvocations(uint functionId);
		void WriteTo(XmlWriter writer);
	}
}
