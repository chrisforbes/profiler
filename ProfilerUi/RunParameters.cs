using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ProfilerUi
{
	class RunParameters
	{
		public readonly string exePath;
		public readonly string workingDirectory;
		public readonly string parameters;

		public RunParameters(string exePath, string workingDirectory, string parameters)
		{
			this.exePath = exePath;
			this.workingDirectory = workingDirectory;
			this.parameters = parameters;
		}

		public override bool Equals(object obj)
		{
			RunParameters p = obj as RunParameters;
			return p.exePath == exePath && 
				p.workingDirectory == workingDirectory && 
				p.parameters == parameters;
		}

		public override int GetHashCode()
		{
			return exePath.GetHashCode() ^ workingDirectory.GetHashCode() ^ parameters.GetHashCode();
		}

		public RunParameters(XmlElement e)
		{
			exePath = e.SelectSingleNode("./cmd").InnerText;
			workingDirectory = e.SelectSingleNode("./dir").InnerText;
			parameters = e.SelectSingleNode("./args").InnerText;
		}

		public void WriteTo(XmlWriter writer)
		{
			writer.WriteStartElement("run");
			writer.WriteElementString("cmd", exePath.Replace('\\', '/'));
			writer.WriteElementString("dir", workingDirectory.Replace('\\', '/'));
			writer.WriteElementString("args", parameters);
			writer.WriteEndElement();
		}
	}
}
