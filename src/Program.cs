using CommandLine;
using System;
using System.Diagnostics;
using System.Threading;
using VersionedCopy.Interfaces;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public class Program
	{
		static void Run(IOptions options, Report report, CancellationToken token, Action<AlgorithmEnv> algo)
		{

			if (options.SourceDirectory == options.DestinationDirectory) throw new ArgumentException("Source and destination must be different!");
#if DEBUG
			Stopwatch stopwatch = Stopwatch.StartNew();
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
#if DEBUG
			stopwatch.Benchmark("Copy");
#endif
		}

		public static void Main(string[] args)
		{
#if !DEBUG
		var update = new AutoUpdate();
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

			//ServiceLocator.AddService<IFileSystem>(new FileSystem(report, options.DryRun));
			var parser = new Parser(with => with.CaseSensitive = false);
			parser.ParseArguments<MirrorOptions, UpdateOptions, SyncOptions>(args)
				.WithParsed<MirrorOptions>(options => Run(options, report, cts.Token, Mirror.Run))
				.WithParsed<UpdateOptions>(options => Run(options, report, cts.Token, Update.Run))
				.WithParsed<SyncOptions>(options => Run(options, report, cts.Token, Sync.Run))
				.WithParsed<StoreStateOptions>(options => StoreState.Run(options.Directory, options.DatabaseFileName, options.IgnoreDirectories, options.IgnoreFiles));

#if !DEBUG
update.CheckAndExecuteUpdateAsync();
#endif
		}
	}
}