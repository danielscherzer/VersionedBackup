using CommandLine;
using System;
using System.Threading;
using VersionedCopy;
using VersionedCopy.Interfaces;
using VersionedCopy.Services;

void Run(IOptions options)
{
	// create logger service
	Report report = new();
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

#if !DEBUG
var update = new UpdateAssembly();
#endif
Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
#if !DEBUG
update.CheckAndExecuteUpdateAsync();
#endif

