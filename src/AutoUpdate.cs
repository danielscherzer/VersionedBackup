using AutoUpdateViaGitHubRelease;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

internal class AutoUpdate
{
	private readonly string updateArchive;
	private readonly Task<bool> updateTask;
	private readonly Assembly assembly;
	private readonly string tempDir;

	public AutoUpdate()
	{
		assembly = Assembly.GetExecutingAssembly();
		tempDir = Path.Combine(Path.GetTempPath(), nameof(VersionedCopy));
		Directory.CreateDirectory(tempDir);
		updateArchive = Path.Combine(tempDir, "update.zip");
		Console.WriteLine("Checking for new version...");
		updateTask = UpdateTools.CheckDownloadNewVersionAsync("danielScherzer", "VersionedCopy"
			, assembly.GetName().Version, updateArchive);
	}

	internal void CheckAndExecuteUpdateAsync()
	{
		if (updateTask.Result)
		{
			Console.WriteLine("New version found. Starting update...");
			var installer = Path.Combine(tempDir, UpdateTools.DownloadExtractInstallerToAsync(tempDir).Result);
			var destinationDir = Path.GetDirectoryName(assembly.Location);
			UpdateTools.StartInstall(installer, updateArchive, destinationDir);
			Environment.Exit(0);
		}
	}
}