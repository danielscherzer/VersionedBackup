using AutoUpdateViaGitHubRelease;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

internal class UpdateAssembly
{
	private readonly string updateArchive;
	private readonly Task<bool> updateTask;
	private readonly Assembly assembly;
	private string tempDir;

	public UpdateAssembly()
	{
		assembly = Assembly.GetExecutingAssembly();
		tempDir = Path.Combine(Path.GetTempPath(), nameof(VersionedCopy));
		Directory.CreateDirectory(tempDir);
		updateArchive = Path.Combine(tempDir, "update.zip");
		updateTask = UpdateTools.CheckDownloadNewVersionAsync("danielScherzer", "VersionedCopy"
			, assembly.GetName().Version, updateArchive);
		var v = assembly.GetName().Version;
	}

	internal async void CheckAndExecuteUpdateAsync()
	{
		if (await updateTask)
		{
			var installer = Path.Combine(tempDir, UpdateTools.DownloadExtractInstallerToAsync(tempDir).Result);
			var destinationDir = Path.GetDirectoryName(assembly.Location);
			UpdateTools.StartInstall(installer, updateArchive, destinationDir);
			Environment.Exit(0);
		}
	}
}