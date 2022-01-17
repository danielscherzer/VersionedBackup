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
			report.Add($"{operation} '{target}'");
			Console.WriteLine($"{operation} '{target}'");
		}

		public void Error(string message)
		{
			Console.WriteLine($"ERROR: {message}");
		}

		public void Save(string fileName)
		{
			if (0 < report.Count) File.WriteAllLines(fileName, report);
		}

		private readonly List<string> report = new();
	}
}
