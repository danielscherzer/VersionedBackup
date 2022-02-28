using System;
using VersionedCopy.Interfaces;

namespace VersionedCopy.Services
{
	public class Output : IOutput
	{
		public void Error(string message)
		{
			Console.Error.WriteLine($"ERROR: {message}");
		}

		public void Report(string message)
		{
			Console.WriteLine(message);
		}
	}
}
