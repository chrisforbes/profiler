using System;
using System.Collections.Generic;
using System.Text;

namespace Ijw.Profiler.Core
{
	public static class Util
	{
		public static IEnumerable<U> Convert<T, U>(IEnumerable<T> src, Converter<T, U> conv)
		{
			foreach (T t in src)
				yield return conv(t);
		}

		public static IEnumerable<T> Skip<T>(IEnumerable<T> src, int count)
		{
			foreach (T t in src)
				if (count-- <= 0)
					yield return t;
		}

		public static IEnumerable<T> Take<T>(IEnumerable<T> src, int count)
		{
			foreach (T t in src)
				if (count-- <= 0)
					yield break;
				else
					yield return t;
		}

		public static T First<T>(IEnumerable<T> src)
		{
			using (IEnumerator<T> e = src.GetEnumerator())
				if (!e.MoveNext())
					throw new IndexOutOfRangeException("Empty sequence");
				else
					return e.Current;
		}

		public static string Join(IEnumerable<string> src, string joinWith)
		{
			StringBuilder result = new StringBuilder();

			foreach (string s in Take(src, 1))
				result.Append(s);
			foreach (string s in Skip(src, 1))
				result.Append(joinWith + s);

			return result.ToString();
		}

		public static T[] Rest<T>(T[] a)
		{
			T[] result = new T[a.Length - 1];
			Array.Copy(a, 1, result, 0, result.Length);
			return result;
		}
	}
}
