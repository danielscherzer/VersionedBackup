using VersionedBackup.Interfaces;
using VersionedBackup.Services;

namespace VersionedBackupTests.Services
{
	internal class NullReport : IReport
	{
		public void Add(Operation operation, string target)
		{
		}
	}
}