//TODO: if program stopped last saved files is invalid

using AutoUpdateViaGitHubRelease;
using CommandLine;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using VersionedCopy;
using VersionedCopy.Interfaces;
using VersionedCopy.Services;

Parser.Default.ParseArguments<Options>(args).WithParsed(options => Run(options));

void Run(IOptions options)
{
// create services
	ILogger logger = new Logger();
	//var update = new Update("danielscherzer", nameof(VersionedCopy), Assembly.GetExecutingAssembly());

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
	//if(update.Available)
	//{
	//	Console.Write("Update? (Y/N)");
	//	if(ConsoleKey.Y == Console.ReadKey().Key)
	//	{
	//		update.Install();
	//	}
	//}
}
