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

	Version3MT.Run(benchmark, src, dst, oldFilesFolder);
	benchmark.Delta("startup");
	benchmark.Repeat(3, () => Version3MT.Run(benchmark, src, dst, oldFilesFolder), nameof(Version3MT));
	//benchmark.Repeat(3, () => Version1.Run(benchmark, src, dst, oldFilesFolder), nameof(Version1));
	//benchmark.Repeat(3, () => Version2.Run(benchmark, src, dst, oldFilesFolder), nameof(Version2));
	//benchmark.Repeat(3, () => Version3.Run(benchmark, src, dst, oldFilesFolder), nameof(Version3));
}
