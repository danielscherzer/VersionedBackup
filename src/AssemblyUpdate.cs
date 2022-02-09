using AutoUpdateViaGitHubRelease;
using System;
using System.IO;
using System.Reflection;

namespace VersionedCopy
{
	internal static class AssemblyUpdate
	{
		public static void Update()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var tempDir = Path.Combine(Path.GetTempPath(), nameof(VersionedCopy));
			Directory.CreateDirectory(tempDir);
			var updateArchive = Path.Combine(tempDir, "update.zip");
			var version = assembly.GetName().Version;
			Console.WriteLine($"Versioned Copy {version}");
			Console.WriteLine($"Checking for newer version ...");
			var updateTask = UpdateTools.CheckDownloadNewVersionAsync("danielScherzer", "VersionedCopy", version, updateArchive);

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
}