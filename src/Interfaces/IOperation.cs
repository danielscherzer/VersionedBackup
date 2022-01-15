namespace VersionedBackup.Interfaces
{
	internal interface IOperation
	{
		string DestinationDirectory { get; }
		string OldFilesFolder { get; }
		string SourceDirectory { get; }
	}
}