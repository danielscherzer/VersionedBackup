using CommandLine;
using System;
using System.Threading;
using VersionedCopy;
using VersionedCopy.Interfaces;
using VersionedCopy.Services;
#if !DEBUG
using AutoUpdateViaGitHubRelease;
using System.Reflection;
#endif

// create logger service
Report report = new();
#if !DEBUG
var assembly = Assembly.GetExecutingAssembly();
var tempDir = Path.Combine(Path.GetTempPath(), nameof(VersionedCopy));
Directory.CreateDirectory(tempDir);
var updateArchive = Path.Combine(tempDir, "update.zip");
var updateTask = UpdateTools.CheckDownloadNewVersionAsync("danielScherzer", "VersionedCopy"
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
		report.Error("CANCEL received - stopping opperations!");
		cts.Cancel();
		args.Cancel = true; // means to continue the process!, no hard cancel, but give process time to cleanup
	};
#if DEBUG
	using Benchmark _ = new("Copy");
#endif

	var fileSystem = new FileSystem(report, options.DryRun);
	if (!fileSystem.ExistsDirectory(options.SourceDirectory))
	{
		report.Error($"Source directory '{options.SourceDirectory}' does not exist");
		return;
	}

	switch (options.Mode)
	{
		case AlgoMode.Mirror:
			new Mirror(options, report, fileSystem, cts.Token);
			break;
		case AlgoMode.Sync:
			new Sync(options, report, fileSystem, cts.Token);
			break;
		case AlgoMode.Update:
			new Update(options, report, fileSystem, cts.Token);
			break;
	}

	if (!options.DryRun)
	{
		report.Save(options.OldFilesFolder + "report.json");
	}
}
