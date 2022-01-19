using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using VersionedCopy.Interfaces;

namespace VersionedCopy.Services
{
	public class Report : IReport
	{
		public void Add(Operation operation, string target)
		{
			report.Add((operation, target));
			Console.WriteLine($"{operation} '{target}'");
		}

		public void Error(string message)
		{
			Console.WriteLine($"ERROR: {message}");
		}

		public void Save(string fileName)
		{//TODO: better json
			string json = JsonConvert.SerializeObject(report, Formatting.Indented);
			if (0 < report.Count) File.WriteAllText(fileName, json);
		}

		private readonly List<(Operation, string)> report = new();
	}
}
