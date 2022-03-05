﻿using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Services
{
	[Verb("sync", HelpText = "Sync the soure directory and the destination directory bidirectionally.")]
	public class SyncOptions : Options
	{
		public SyncOptions(string sourceDirectory, string destinationDirectory, string oldFilesFolder, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool readOnly) : base(sourceDirectory, destinationDirectory, oldFilesFolder, ignoreDirectories, ignoreFiles, readOnly)
		{
		}
	}
}
