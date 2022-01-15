using CommandLine;
using System;
using System.IO;
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
	using CancellationTokenSource cts = new();
	Console.CancelKeyPress += (_, args) =>
	{
		logger.Add("CANCEL received - stopping opperations!");
		cts.Cancel();
		args.Cancel = true; // means to continue the process!, no hard cancel, but give process time to cleanup
	};
#if DEBUG
	using Benchmark _ = new("Copy");
#endif

	if (!Directory.Exists(options.SourceDirectory))
	{
		logger.Add($"Source directory '{options.SourceDirectory}' does not exist");
		return;
	}
	var fileSystem = new FileSystem(logger, options.DryRun);
	Report report = new(logger);
	FileSystemOperations op = new(report, options, fileSystem);
	Backup.Run(options, op, fileSystem, cts.Token);
	if (!options.DryRun)
	{
		report.Save(options.OldFilesFolder + "report.txt");
	}
}
