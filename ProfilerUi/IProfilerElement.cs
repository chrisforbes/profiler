using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ProfilerUi
{
	interface IProfilerElement
	{
		TreeNode CreateView(double totalTime);
		string TabTitle { get; }
		double TotalTime { get; }

		List<Function> CollectInvocations(uint functionId);
	}
}
