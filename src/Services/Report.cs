using System.Collections.Generic;
using System.IO;
using VersionedBackup.Interfaces;

namespace VersionedBackup.Services
{
	public class Report : IReport
	{
		public Report(ILogger logger)
		{
			this.logger = logger;
		}

		public void Add(Operation operation, string target)
		{
			report.Add($"{operation} '{target}'");
			logger.Add($"{operation} '{target}'");
		}

		public void Save(string fileName)
		{
			if (0 < report.Count) File.WriteAllLines(fileName, report);
		}

		private readonly List<string> report = new();
		private readonly ILogger logger;
	}
}
