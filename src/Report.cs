using System.Collections.Generic;
using System.IO;
using VersionedBackup.Interfaces;

namespace VersionedBackup
{
	internal class Report
	{
		public Report(ILogger logger)
		{
			this.logger = logger;
		}

		public void Add(string operation, string target)
		{
			report.Add($"{operation} '{target}'");
			logger.Log($"{operation} '{target}'");
		}

		public void Save(string fileName)
		{
			if (0 < report.Count) File.WriteAllLines(fileName, report);
		}

		private readonly List<string> report = new();
		private readonly ILogger logger;
	}
}
