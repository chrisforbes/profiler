using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using IjwFramework.Types;

namespace ProfilerUi
{
	class ClrNameFactory : INameFactory
	{
		static Regex genericReplacer = new Regex(@"`\d+");
		static List<Pair<string, MethodType>> specialTypes = new List<Pair<string, MethodType>>();

		static void AddRule(string prefix, MethodType result)
		{
			specialTypes.Add(new Pair<string, MethodType>(prefix, result));
		}

		static ClrNameFactory()
		{
			AddRule("get_", MethodType.PropertyGet);
			AddRule("set_", MethodType.PropertySet);
			AddRule("add_", MethodType.EventAdd);
			AddRule("remove_", MethodType.EventRemove);
		}

		public Name Create(string rawName)
		{
			try
			{
				rawName = genericReplacer.Replace(rawName, "<>");

				string[] parts = rawName.Split(new char[] { ':' },
					StringSplitOptions.RemoveEmptyEntries);

				string ClassName = parts[0].Replace('$', '.');
				string MethodName = parts[1];

				foreach (Pair<string, MethodType> p in specialTypes)
					if (MethodName.Contains(p.First))
						return new Name(MethodName.Replace(p.First, ""), ClassName, p.Second);

				if (MethodName.Contains(".ctor"))
					return new Name("new", ClassName, MethodType.Constructor);

				return new Name(MethodName, ClassName, MethodType.Method);
			}
			catch (Exception)
			{
				return new Name("(error)", "(error)", MethodType.Method);
			}
		}
	}
}
