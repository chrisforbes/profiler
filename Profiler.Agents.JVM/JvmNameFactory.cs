using System;
using System.Collections.Generic;
using System.Text;
using Ijw.Profiler.Core;

namespace Ijw.Profiler.Agents.JVM
{
	class JvmNameFactory : INameFactory
	{
		readonly Predicate<string> isFunctionInteresting;

		public JvmNameFactory(Predicate<string> isFunctionInteresting)
		{
			this.isFunctionInteresting = isFunctionInteresting;
		}

		public Name Create(string rawName)
		{
			string[] frags = rawName.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
			string className = frags[0];
			string methodName = frags[1];
			MethodType type = MethodType.Method;

			bool interesting = isFunctionInteresting(className);

			if (methodName == "<init>")
			{
				type = MethodType.Constructor;
				methodName = ".ctor()";
			}
			else
				methodName += "()";

			return new Name(methodName, className, type, interesting);
		}
	}
}
