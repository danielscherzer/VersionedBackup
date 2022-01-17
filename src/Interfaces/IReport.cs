namespace VersionedCopy.Interfaces
{
	public interface IReport : IErrorOutput
	{
		void Add(Operation operation, string target);
	}
}