using CommandLine;
using System;
using System.Threading;
using VersionedCopy;
using VersionedCopy.Interfaces;
using VersionedCopy.Services;

static void Run(IOptions options, Report report, CancellationToken token, Action<AlgorithmEnv> algo)
{
#if DEBUG
	using Benchmark _ = new("Copy");
#endif
	var fileSystem = new FileSystem(report, options.DryRun);
	if (!fileSystem.ExistsDirectory(options.SourceDirectory))
	{
		report.Error($"Source directory '{options.SourceDirectory}' does not exist");
		return;
	}
	algo(new AlgorithmEnv(options, report, fileSystem, token));
	if (!options.DryRun && fileSystem.ExistsDirectory(options.OldFilesFolder))
	{
		report.Save(options.OldFilesFolder + "report.json");
	}
}

#if !DEBUG
var update = new UpdateAssembly();
#endif

// create logger service
Report report = new();
using CancellationTokenSource cts = new();
Console.CancelKeyPress += (_, args) =>
{
	report.Error("CANCEL received - stopping opperations!");
	cts.Cancel();
	args.Cancel = true; // means to continue the process!, no hard cancel, but give process time to cleanup
};

Parser.Default.ParseArguments<MirrorOptions, UpdateOptions, SyncOptions>(args)
	.WithParsed<MirrorOptions>(options => Run(options, report, cts.Token, Mirror.Run))
	.WithParsed<UpdateOptions>(options => Run(options, report, cts.Token, Update.Run))
	.WithParsed<SyncOptions>(options => Run(options, report, cts.Token, Sync.Run));

#if !DEBUG
update.CheckAndExecuteUpdateAsync();
#endif

