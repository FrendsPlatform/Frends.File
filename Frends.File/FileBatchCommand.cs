using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591 // Disable missing documentation warnings

namespace Frends.File
{
    public static class FileBatchCommand
    {
        public static async Task<IList<FileInBatchResult>> ExecuteAsync(IFileBatchInput input, IFileBatchOptions options,
            Func<string, string, CancellationToken, Task> batchAction, CancellationToken cancellationToken, string actionNameInErrors)
        {
            var results = File.FindMatchingFiles(input.Directory, input.Pattern);
            var fileTransferEntries = GetFileTransferEntries(results.Files, input.Directory, input.TargetDirectory, options.PreserveDirectoryStructure);

            if (options.IfTargetFileExists == FileExistsAction.Throw)
            {
                AssertNoTargetFileConflicts(fileTransferEntries.Values, actionNameInErrors);
            }

            if (options.CreateTargetDirectories)
            {
                Directory.CreateDirectory(input.TargetDirectory);
            }

            var fileResults = new List<FileInBatchResult>();
            try
            {
                foreach (var entry in fileTransferEntries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var sourceFilePath = entry.Key;
                    var targetFilePath = entry.Value;

                    if (options.CreateTargetDirectories)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
                    }

                    switch (options.IfTargetFileExists)
                    {
                        case FileExistsAction.Rename:
                            targetFilePath = Frends.File.File.GetNonConflictingDestinationFilePath(sourceFilePath, targetFilePath);
                            await batchAction.Invoke(sourceFilePath, targetFilePath, cancellationToken).ConfigureAwait(false);
                            break;
                        case FileExistsAction.Overwrite:
                            if (System.IO.File.Exists(targetFilePath))
                            {
                                System.IO.File.Delete(targetFilePath);
                            }
                            await batchAction.Invoke(sourceFilePath, targetFilePath, cancellationToken).ConfigureAwait(false);
                            break;
                        case FileExistsAction.Throw:
                            if (System.IO.File.Exists(targetFilePath))
                            {
                                throw new IOException($"File '{targetFilePath}' already exists. No files {actionNameInErrors}.");
                            }
                            await batchAction.Invoke(sourceFilePath, targetFilePath, cancellationToken).ConfigureAwait(false);
                            break;
                    }
                    fileResults.Add(new FileInBatchResult(sourceFilePath, targetFilePath));
                }
            }
            catch (Exception)
            {
                //Delete the target files that were already moved before a file that exists breaks the move command
                Frends.File.File.DeleteExistingFiles(fileResults.Select(x => x.Path));
                throw;
            }

            return fileResults;
        }

        private static void AssertNoTargetFileConflicts(IEnumerable<string> filePaths, string errorOp)
        {
            // check the target file list to see there should not be conflicts before doing anything
            var duplicateTargetPaths = GetDuplicateValues(filePaths);
            if (duplicateTargetPaths.Any())
            {
                throw new IOException($"Multiple files written to {string.Join(", ", duplicateTargetPaths)}. The files would get overwritten. No files {errorOp}.");
            }

            foreach (var targetFilePath in filePaths)
            {
                if (System.IO.File.Exists(targetFilePath))
                {
                    throw new IOException($"File '{targetFilePath}' already exists. No files {errorOp}.");
                }
            }
        }

        private static IList<string> GetDuplicateValues(IEnumerable<string> values)
        {
            return values.GroupBy(v => v).Where(x => x.Count() > 1).Select(k => k.Key).ToList();
        }

        private static Dictionary<string, string> GetFileTransferEntries(IEnumerable<FilePatternMatch> fileMatches, string sourceDirectory, string targetDirectory, bool preserveDirectoryStructure)
        {
            return fileMatches
                .ToDictionary(
                    f => Path.Combine(sourceDirectory, f.Path),
                    f => preserveDirectoryStructure
                     ? Path.GetFullPath(Path.Combine(targetDirectory, f.Path))
                     : Path.GetFullPath(Path.Combine(targetDirectory, Path.GetFileName(f.Path))));
        }
    }

    public interface IFileBatchInput
    {
        string Directory { get; set; }
        string Pattern { get; set; }
        string TargetDirectory { get; set; }
    }

    public interface IFileBatchOptions
    {
        bool UseGivenUserCredentialsForRemoteConnections { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        bool PreserveDirectoryStructure { get; set; }
        bool CreateTargetDirectories { get; set; }
        FileExistsAction IfTargetFileExists { get; set; }
    }

    public class FileInBatchResult
    {
        public FileInBatchResult(string sourcePath, string targetPath)
        {
            SourcePath = sourcePath;
            Path = targetPath;
        }
        public string SourcePath { get; set; }
        public string Path { get; set; }
    }

    public enum FileExistsAction
    {
        /// <summary>
        /// Throw an error and roll back all transfers
        /// </summary>
        Throw,

        /// <summary>
        /// Overwrite the target file
        /// </summary>
        Overwrite,

        /// <summary>
        /// Rename the transferred file by appending a number to the end
        /// </summary>
        Rename
    }
}
