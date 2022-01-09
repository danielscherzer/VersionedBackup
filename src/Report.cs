using System.Collections.Generic;
using System.IO;

namespace VersionedBackup
{
	internal class Report
	{
		internal enum Operation { Delete }
		public void Add(string message/*Operation operation*/)
		{
			report.Add(message);
		}
		public void Save(string fileName)
		{
			if (0 < report.Count) File.WriteAllLines(fileName, report);
		}

		private readonly List<string> report = new();
	}
}
