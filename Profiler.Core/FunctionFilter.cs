using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ijw.Profiler.Core
{
	public class NameFilter
	{
		Regex regex;

		public NameFilter(params string[] patterns)
		{
			string p = "";
			foreach (string s in patterns)
			{
				string r = "(" + Regex.Escape(s) + ")";
				p = string.IsNullOrEmpty(p) ? r : p + "|" + r;
			}

			regex = new Regex("^" + p);
		}

		public bool IsFunctionInteresting(string f) { return !regex.IsMatch(f); }
	}
}
