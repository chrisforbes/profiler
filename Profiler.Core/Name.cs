using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using IjwFramework.Types;

namespace Ijw.Profiler.Core
{
	public enum MethodType
	{
		Method,
		PropertyGet,
		PropertySet,
		EventAdd,
		EventRemove,
		Constructor,
	}

	public class Name
	{
		public readonly string MethodName;
		public readonly string ClassName;
		public readonly MethodType Type;
		public readonly bool Interesting;

		public Name(string methodName, string className, MethodType type, bool interesting)
		{
			MethodName = methodName;
			ClassName = className;
			Type = type;
			Interesting = interesting;
		}
	}
}
