namespace VersionedBackup.Interfaces
{
	public interface IReport
	{
		void Add(Operation operation, string target);
	}
}