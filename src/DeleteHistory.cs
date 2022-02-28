using System.Collections.Generic;
using System;

namespace VersionedCopy
{
	public class DeleteHistory : Dictionary<string, DateTime>
	{
		public void Update(Snapshot old, Snapshot current)
		{
			if(old.TimeStamp > current.TimeStamp)
			{
				throw new ArgumentException("Old time stamp is newer then current time stamp.");
			}
			var deletedDirs = old.DirectorySingles(current);
			foreach(var deletedDir in deletedDirs)
			{
				this[deletedDir.Key] = current.TimeStamp;
			}
			var deletedFiles = old.FileSingles(current);
			foreach(var deletedFile in deletedFiles)
			{
				this[deletedFile.Key] = current.TimeStamp;
			}
			// remove newly created ones
			var createdDirs = current.DirectorySingles(old);
			foreach(var createdDir in createdDirs)
			{
				_ = Remove(createdDir.Key);
			}
			var createdFiles = current.FileSingles(old);
			foreach(var createdFile in createdFiles)
			{
				_ = Remove(createdFile.Key);
			}
		}
	}
}
