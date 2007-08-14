using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ProfilerUi
{
	static class MruList
	{
		static RunParameters Load(XmlElement e)
		{
			return new RunParameters(e.SelectSingleNode("./cmd").InnerText,
				e.SelectSingleNode("./dir").InnerText,
				e.SelectSingleNode("./args").InnerText);
		}

		const int maxItems = 5;

		public static void AddRun(RunParameters p)
		{
			List<RunParameters> l = new List<RunParameters>();
			XmlDocument doc = new XmlDocument();
			doc.Load("mru.xml");

			foreach (XmlElement e in doc.SelectNodes("/mru/run"))
				l.Add(Load(e));

			UpdateList(l, p);

			XmlWriterSettings settings= new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "  ";

			XmlWriter writer = XmlWriter.Create("mru.xml", settings);
			writer.WriteStartElement("mru");

			foreach (RunParameters r in l)
			{
				writer.WriteStartElement("run");
				writer.WriteElementString("cmd", r.exePath);
				writer.WriteElementString("dir", r.workingDirectory);
				writer.WriteElementString("args", r.parameters);
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
		}

		static void UpdateList( List<RunParameters> list, RunParameters p )
		{
			list.Insert(0, p);
			for( int i = 1; i < list.Count; i++ )
				if (list[i].Equals(p))
				{
					list.RemoveAt(i);
					return;
				}

			if (list.Count > maxItems)
				list.RemoveRange(maxItems, list.Count - maxItems);
		}
	}
}
