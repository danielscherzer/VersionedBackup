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
var assembly = Assembly.GetExecutingAssembly();
var tempDir = Path.Combine(Path.GetTempPath(), nameof(VersionedBackup));
Directory.CreateDirectory(tempDir);
var updateArchive = Path.Combine(tempDir, "update.zip");
var updateTask = UpdateTools.CheckDownloadNewVersionAsync("danielScherzer", "VersionedBackup"
	, assembly.GetName().Version, updateArchive);
var v = assembly.GetName().Version;
Parser.Default.ParseArguments<Options>(args).WithParsed(Run);

var updateAvailable = updateTask.Result;
if (updateAvailable)
{
	Console.Write("Update? (Y/N)");
	if (ConsoleKey.Y == Console.ReadKey().Key)
	{
		var installer = Path.Combine(tempDir, UpdateTools.DownloadExtractInstallerToAsync(tempDir).Result);
		var destinationDir = Path.GetDirectoryName(assembly.Location);
		UpdateTools.InstallAsync(installer, updateArchive, destinationDir);
		Environment.Exit(0);
	}
}

void Run(IOptions options)
{
	// create file sysem service
	IFileSystem fileSystem = options.DryRun ? new NullFileSystem() : new FileSystem(logger, options.LogErrors);
	var cts = new CancellationTokenSource();
	Console.CancelKeyPress += (_, _) =>
	{
		logger.Log("CANCEL received - stopping opperations!");
		cts.Cancel();
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
