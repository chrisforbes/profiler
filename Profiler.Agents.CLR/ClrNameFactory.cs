using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using IjwFramework.Types;
using Ijw.Profiler.Core;

namespace Ijw.Profiler.Agents.CLR
{
	class ClrNameFactory : INameFactory
	{
		static Regex genericReplacer = new Regex(@"`\d+");
		static List<Pair<string, MethodType>> specialTypes = new List<Pair<string, MethodType>>();

		Predicate<string> isFunctionInteresting;

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

		public ClrNameFactory(Predicate<string> isFunctionInteresting)
		{
			this.isFunctionInteresting = isFunctionInteresting;
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

				bool interesting = isFunctionInteresting( ClassName );

				foreach (Pair<string, MethodType> p in specialTypes)
					if (MethodName.Contains(p.First))
						return new Name(MethodName.Replace(p.First, ""), ClassName, p.Second, interesting);

				if (MethodName.Contains(".ctor"))
					return new Name("new", ClassName, MethodType.Constructor, interesting);

				return new Name(MethodName, ClassName, MethodType.Method, interesting);
			}
			catch (Exception)
			{
				return new Name("(error)", "(error)", MethodType.Method, false);
			}
		}
	}
}
