using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services
{
	public class Operations
	{
		private readonly IFileSystem fileSystem;
		private readonly IReport report;
		private readonly string src;
		private readonly string dst;
		private readonly string old;

		public Operations(IReport report, IDirectories directories, IFileSystem fileSystem)
		{
			this.report = report;
			this.fileSystem = fileSystem;
			src = directories.SourceDirectory;
			dst = directories.DestinationDirectory;
			old = directories.OldFilesFolder;
		}

		internal void CopyNewFile(string fileName)
		{
			var srcFilePath = src + fileName;
			var dstFilePath = dst + fileName;
			if (fileSystem.Copy(srcFilePath, dstFilePath))
			{
				report.Add(Operation.NewFile, fileName);
			}
		}

		internal void CopyNewFileToSrc(string fileName)
		{
			var srcFilePath = src + fileName;
			var dstFilePath = dst + fileName;
			if (fileSystem.Copy(dstFilePath, srcFilePath))
			{
				report.Add(Operation.NewFile, fileName);
			}
		}


		internal void CopyUpdatedFile(string fileName)
		{
			var srcFilePath = src + fileName;
			var dstFilePath = dst + fileName;
			if (0 < fileSystem.CompareAge(srcFilePath, dstFilePath))
			{
				// move old to oldFilesFolder
				if (fileSystem.MoveFile(dstFilePath, old + fileName))
				{
					// copy updated to dst
					if (fileSystem.Copy(srcFilePath, dstFilePath))
					{
						report.Add(Operation.UpdateFile, fileName);
					}
				}
			}
		}

		internal void CreateDirectory(string subDir)
		{
			string directory = dst + subDir;
			if (fileSystem.CreateDirectory(directory))
			{
				report.Add(Operation.CreateDir, subDir);
			}
		}

		internal void CreateSrcDirectory(string subDir)
		{
			string directory = src + subDir;
			if (fileSystem.CreateDirectory(directory))
			{
				report.Add(Operation.CreateDir, subDir);
			}
		}

		internal void MoveAwayDeletedDir(string subDir)
		{
			string source = dst + subDir;
			if (fileSystem.ExistsDirectory(source))
			{
				string destination = old + subDir;
				if (fileSystem.MoveDirectory(source, destination))
				{
					report.Add(Operation.DeleteDir, subDir);
				}
			}
		}

		internal void MoveAwayDeletedFile(string fileName)
		{
			string moveAwayFileName = dst + fileName;
			if (fileSystem.ExistsFile(moveAwayFileName))
			{
				string destination = old + fileName;
				if (fileSystem.MoveFile(moveAwayFileName, destination))
				{
					report.Add(Operation.DeleteFile, fileName);
				}
			}
		}

		internal void ReplaceChangedFile(string fileName)
		{
			var srcFilePath = src + fileName;
			var dstFilePath = dst + fileName;
			if (fileSystem.HasChanged(srcFilePath, dstFilePath))
			{
				// move old to oldFilesFolder
				if (fileSystem.MoveFile(dstFilePath, old + fileName))
				{
					// copy new to dst
					if (fileSystem.Copy(srcFilePath, dstFilePath))
					{
						report.Add(Operation.ReplaceFile, fileName);
					}
				}
			}
		}

		internal void CopyNewerFileToOtherSide(string fileName)
		{
			var srcFilePath = src + fileName;
			var dstFilePath = dst + fileName;
			switch (fileSystem.CompareAge(srcFilePath, dstFilePath))
			{
				case -1: var temp = srcFilePath; srcFilePath = dstFilePath; dstFilePath = temp; break; // dstFile newer
				case 1: break; // srcFile newer
				default: return;
			}
			// move old to oldFilesFolder
			if (fileSystem.MoveFile(dstFilePath, old + fileName))
			{
				// copy new to dst
				if (fileSystem.Copy(srcFilePath, dstFilePath))
				{
					report.Add(Operation.ReplaceFile, fileName);
				}
			}
		}
	}
}

