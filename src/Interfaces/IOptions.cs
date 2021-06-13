using System.Collections.Generic;

namespace VersionedBackup.Interfaces
{
	internal interface IOptions
	{
		string DestinationDirectory { get; }
		IEnumerable<string> IgnoreDirectories { get; }
		IEnumerable<string> IgnoreFiles { get; }
		string SourceDirectory { get; }
		string OldFilesFolder { get; }
		bool LogErrors { get; }
		bool LogOperations { get; }
		bool DryRun { get; }
	}
}