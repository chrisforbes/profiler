using System;
using System.Collections.Generic;
using System.Text;
using Ijw.Profiler.Core;

namespace Ijw.Profiler.Agents.JVM
{
	class JvmNameFactory : INameFactory
	{
		public Name Create(string rawName)
		{
			string[] frags = rawName.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
			string className = frags[0];
			string methodName = frags[1];

			return new Name(methodName, className, MethodType.Method, true);
		}
	}
}
