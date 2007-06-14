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
		Dictionary<uint, string> names = new Dictionary<uint,string>();

		public FunctionNameProvider( string filename )
		{
			Regex r = new Regex( "^(.*)=(.*)$" );
			foreach( string s in File.ReadAllLines(filename))
			{
				Match m = r.Match( s );
				if (m == null || !m.Success)
					continue;

				uint functionId = uint.Parse(m.Groups[1].Value.Substring(2), NumberStyles.HexNumber);
				string functionName = m.Groups[2].Value;

				names.Add(functionId, functionName);
			}
		}

		public string GetName(uint functionId)
		{
			string value;
			if (!names.TryGetValue(functionId, out value))
				return "<unbound " + functionId + " >";

			return value;
		}
	}
}
