using System;
using System.IO;
using VersionedCopy.Interfaces;

namespace VersionedCopy.Services
{
	public class Output : IOutput
	{
		private string logFile = string.Empty;

		public void Error(string message)
		{
			Console.Error.WriteLine($"ERROR: {message}");
			Log($"ERROR: {message}");
		}

		public void SetLogFile(string path)
		{
			logFile = path;
			var dir = Path.GetDirectoryName(path);
			if(dir is not null) Directory.CreateDirectory(dir);
		}

		public void Report(string message)
		{
			Console.WriteLine(message);
			Log(message);
		}

		private void Log(string message)
		{
			if (!string.IsNullOrEmpty(logFile))
			{
				File.AppendAllText(logFile, $"{DateTime.Now:yyyy-MM-dd_HHmmss} : {message}");
			}
		}
	}
}
