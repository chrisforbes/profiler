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

		public Name(string methodName, string className, MethodType type)
		{
			MethodName = methodName;
			ClassName = className;
			Type = type;
		}
	}
}
