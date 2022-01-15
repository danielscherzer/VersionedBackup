using System.Collections.Generic;

namespace VersionedBackup.Interfaces
{
	public interface IOptions : IDirectories
	{
		IEnumerable<string> IgnoreDirectories { get; }
		IEnumerable<string> IgnoreFiles { get; }
		bool DryRun { get; }
	}
}