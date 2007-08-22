using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using IjwFramework.Types;

namespace ProfilerUi
{
	enum MethodType
	{
		Method,
		PropertyGet,
		PropertySet,
		EventAdd,
		EventRemove,
		Constructor,
	}

	class Name
	{
		public readonly string MethodName;
		public readonly string ClassName;
		public readonly MethodType Type;

		static Regex genericReplacer = new Regex(@"`\d+");
		static List<Pair<string, MethodType>> specialTypes = new List<Pair<string, MethodType>>();

		static Name()
		{
			specialTypes.Add(new Pair<string, MethodType>("get_", MethodType.PropertyGet));
			specialTypes.Add(new Pair<string, MethodType>("set_", MethodType.PropertySet));
			specialTypes.Add(new Pair<string, MethodType>("add_", MethodType.EventAdd));
			specialTypes.Add(new Pair<string, MethodType>("remove_", MethodType.EventRemove));
		}

		public Name(string rawName)
		{
			try
			{
				rawName = genericReplacer.Replace(rawName, "<>");

				string[] parts = rawName.Split(new char[] { ':' },
					StringSplitOptions.RemoveEmptyEntries);

				ClassName = parts[0].Replace('$', '.');
				MethodName = parts[1];

				Type = MethodType.Method;

				foreach (Pair<string, MethodType> p in specialTypes)
					if (MethodName.Contains(p.First))
					{
						MethodName = MethodName.Replace(p.First, "");
						Type = p.Second;
						return;
					}

				if (MethodName.Contains(".ctor"))
				{
					MethodName = "new";
					Type = MethodType.Constructor;
					return;
				}
			}
			catch (Exception)
			{
				MethodName = "(processing error)";
				ClassName = "(processing error)";
				Type = MethodType.Method;
			}
		}
	}
}
