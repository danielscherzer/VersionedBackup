namespace VersionedBackup.Interfaces
{
	public interface IReport : IErrorOutput
	{
		void Add(Operation operation, string target);
	}
}