using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VersionedBackup.Interfaces;

namespace VersionedBackupTests.Services
{
	internal class NullReport : IReport
	{
		public void Add(Operation operation, string target)
		{
		}

		public void Error(string message)
		{
			throw new Exception(message);
		}
	}
}