//TODO: if program stopped last saved file is invalid

using CommandLine;
using System;
using System.IO;
using VersionedCopy;

Benchmark benchmark = new();
Parser.Default.ParseArguments<Options>(args).WithParsed(options => Run(options));

void Run(Options options)
{
	var src = options.SourceDirectory;
	var dst = options.DestinationDirectory;
	var oldFilesFolder = $"{dst[0..^1]}-{DateTime.Now:yyyy-MM-dd_HHmmss}{Path.DirectorySeparatorChar}";

	benchmark.Delta("startup");
	Benchmark.Repeat(5, () => Version3MT.Run(benchmark, src, dst, oldFilesFolder));
	Benchmark.Repeat(5, () => Version3.Run(benchmark, src, dst, oldFilesFolder));
	//Version2.Run(benchmark, src, dst, oldFilesFolder);
	//benchmark.Total("-------------------------------");
	//benchmark.Reset();
	//Version1.Run(benchmark, src, dst, oldFilesFolder);
}
