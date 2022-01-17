using System;
using VersionedCopy.Interfaces;

namespace VersionedCopyTests.Services
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