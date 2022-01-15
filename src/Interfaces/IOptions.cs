using System.Collections.Generic;

namespace VersionedBackup.Interfaces
{
	internal interface IOptions : IOperation
	{
		IEnumerable<string> IgnoreDirectories { get; }
		IEnumerable<string> IgnoreFiles { get; }
		bool DryRun { get; }
	}
}