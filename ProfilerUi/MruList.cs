using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace ProfilerUi
{
	static class MruList
	{
		const int maxItems = 5;

		public static void AddRun(RunParameters p)
		{
			List<RunParameters> l = new List<RunParameters>();
			XmlDocument doc = new XmlDocument();
			doc.Load(Application.StartupPath + "/mru.xml");

			foreach (XmlElement e in doc.SelectNodes("/mru/run"))
				l.Add(new RunParameters(e));

			UpdateList(l, p);

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "  ";

			using (XmlWriter writer = XmlWriter.Create(Application.StartupPath + "/mru.xml", settings))
			{
				writer.WriteProcessingInstruction("xml-stylesheet", "href=\"mru.xslt\" type=\"text/xsl\"");
				writer.WriteStartElement("mru");
				foreach (RunParameters r in l)
					r.WriteTo(writer);
				writer.WriteEndElement();
			}
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
