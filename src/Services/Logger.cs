using System;
using VersionedBackup.Interfaces;

namespace VersionedBackup.Services
{
	internal class Logger : ILogger
	{
		public void Add(string message)
		{
			Console.WriteLine(message);
		}
	}
}
