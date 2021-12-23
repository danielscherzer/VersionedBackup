//TODO: if program stopped last saved files is invalid

using AutoUpdateViaGitHubRelease;
using CommandLine;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using VersionedBackup;
using VersionedBackup.Interfaces;
using VersionedBackup.Services;

// create logger service
ILogger logger = new VersionedBackup.Services.Logger();
#if !DEBUG
var assembly = Assembly.GetExecutingAssembly();
var tempDir = Path.Combine(Path.GetTempPath(), nameof(VersionedBackup));
Directory.CreateDirectory(tempDir);
var updateArchive = Path.Combine(tempDir, "update.zip");
var updateTask = UpdateTools.CheckDownloadNewVersionAsync("danielScherzer", "VersionedBackup"
	, assembly.GetName().Version, updateArchive);
var v = assembly.GetName().Version;
#endif
Parser.Default.ParseArguments<Options>(args).WithParsed(Run);

#if !DEBUG
if (await updateTask)
{
	var installer = Path.Combine(tempDir, UpdateTools.DownloadExtractInstallerToAsync(tempDir).Result);
	var destinationDir = Path.GetDirectoryName(assembly.Location);
	UpdateTools.StartInstall(installer, updateArchive, destinationDir);
	Environment.Exit(0);
}
#endif

void Run(IOptions options)
{
	// create file sysem service
	IFileSystem fileSystem = options.DryRun ? new NullFileSystem() : new FileSystem(logger, options.LogErrors);
	using CancellationTokenSource cts = new();
	Console.CancelKeyPress += (_, args) =>
	{
		logger.Log("CANCEL received - stopping opperations!");
		cts.Cancel();
		args.Cancel = true; // means to continue the process!
	};
#if DEBUG
	using var _ = new Benchmark("Copy");
#endif

	if (!Directory.Exists(options.SourceDirectory))
	{
		if (options.LogErrors) logger.Log($"Source directory '{options.SourceDirectory}' does not exist");
		return;
	}
	Backup.Run(logger, options, fileSystem, cts.Token);
}
