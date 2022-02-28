namespace VersionedCopy.Interfaces
{
	public interface IOutput
	{
		void Report(string message);
		void Error(string message);
	}
}
