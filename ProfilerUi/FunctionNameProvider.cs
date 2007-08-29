using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Forms;

namespace ProfilerUi
{
	class FunctionNameProvider
	{
		Dictionary<uint, Name> names = new Dictionary<uint,Name>();

		public FunctionNameProvider(string filename)
		{
			Regex r = new Regex("^0x([0-9a-fA-F]*)=(.*)$");
			foreach (string s in File.ReadAllLines(filename))
			{
				Match m = r.Match(s);
				if (m == null || !m.Success)
					continue;

				uint functionId = uint.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
				names.Add(functionId, new Name(m.Groups[2].Value));
			}
		}

		public Name this[uint functionId]
		{
			get
			{
				Name name;
				if (!names.TryGetValue(functionId, out name))
					return new Name("<unbound " + functionId + " >");

				return name;
			}
		}
	}
}
