using System;
using VersionedCopy.Interfaces;

namespace VersionedCopy.Services
{
	internal class Logger : ILogger
	{
		public void Log(string message)
		{
			Console.WriteLine(message);
		}
	}
}
