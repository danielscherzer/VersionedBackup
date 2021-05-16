//TODO: if program stopped last saved files is invalid

using CommandLine;
using System;
using System.IO;
using System.Threading;
using VersionedCopy;
using VersionedCopy.Interfaces;
using VersionedCopy.Services;

Parser.Default.ParseArguments<Options>(args).WithParsed(options => Run(options));

void Run(IOptions options)
{
	// create services
	ILogger logger = new Logger();
	IFileSystem fileSystem = new FileSystem(logger, options.LogErrors);
	var cts = new CancellationTokenSource();
	Console.CancelKeyPress += (_, _) => cts.Cancel();
	using var _ = new Benchmark("Copy");
	
	if (!Directory.Exists(options.SourceDirectory))
	{
		if (options.LogErrors) logger.Log($"Source directory '{options.SourceDirectory}' does not exist");
		return;
	}
	Backup.Run(logger, options, fileSystem, cts.Token);
}
