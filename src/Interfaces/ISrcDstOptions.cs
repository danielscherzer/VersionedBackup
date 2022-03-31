namespace VersionedCopy.Interfaces
{
	public interface ISrcDstOptions : IOptions
	{
		string DestinationDirectory { get; }
		string SourceDirectory { get; }
	}
}