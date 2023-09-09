using System.Collections.Generic;
using System.Threading;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services;

public class Env
{
	public Env(IOptions options, IOutput output, CancellationToken token)
	{
		if (options.ReadOnly) output.Report("Read only mode");
		output.Report($"Ignore directories: {string.Join(';', options.IgnoreDirectories)}");
		output.Report($"Ignore files: {string.Join(';', options.IgnoreFiles)}");
		FileSystem = new FileSystem(output, options.ReadOnly);
		IgnoreDirectories = options.IgnoreDirectories;
		IgnoreFiles = options.IgnoreFiles;
		Output = output;
		Token = token;
		ReadOnly = options.ReadOnly;
	}

	public FileSystem FileSystem { get; }
	public IOutput Output { get; }
	public CancellationToken Token { get; }
	public bool ReadOnly { get; }

	internal Snapshot CreateSnapshot(string root)
	{
		return Snapshot.Create(root, IgnoreDirectories, IgnoreFiles, Token);
	}

	private IEnumerable<string> IgnoreDirectories { get; }
	private IEnumerable<string> IgnoreFiles { get; }
}
