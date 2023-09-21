using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591 // Disable missing documentation warnings

namespace Frends.File
{
    public static class CopyCommand
    {
        public static Task<IList<FileInBatchResult>> ExecuteAsync(CopyInput input, CopyOptions options, CancellationToken cancellationToken)
        {
            return FileBatchCommand.ExecuteAsync(input, options, CopyFileAsync, cancellationToken, actionNameInErrors: "copied");
        }

        /// <summary>
        /// Copies file using async stream operations
        /// </summary>
        public static async Task CopyFileAsync(string sourceFilePath, string targetFilePath, CancellationToken cancellationToken)
        {
            using (FileStream sourceStream = System.IO.File.Open(sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                using (FileStream destinationStream = System.IO.File.Open(targetFilePath, FileMode.CreateNew))
                {
                    await sourceStream.CopyToAsync(destinationStream, 81920, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    public class CopyInput : IFileBatchInput
    {
        /// <summary>
        /// Root directory where the pattern matching should start
        /// </summary>
        [DefaultValue("\"c:\\\"")]
        public string Directory { get; set; }

        /// <summary>
        /// Pattern to match for files.
        /// </summary>
        [DefaultValue("\"**\\Folder\\*.xml\"")]
        public string Pattern { get; set; }

        /// <summary>
        /// Target directory where the found files should be copied to
        /// </summary>
        [DefaultValue("\"d:\\backup\"")]
        public string TargetDirectory { get; set; }
    }

    public class CopyOptions : IFileBatchOptions
    {
        /// <summary>
        /// If set, allows you to give the user credentials to use to copy files on remote hosts.
        /// If not set, the agent service user credentials will be used.
        /// Note: For copying files on the local machine, the agent service user credentials will always be used, even if this option is set.
        /// </summary>
        public bool UseGivenUserCredentialsForRemoteConnections { get; set; }

        /// <summary>
        /// This needs to be of format domain\username
        /// </summary>
        [DefaultValue("\"domain\\username\"")]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections),"", true)]
        public string UserName { get; set; }

        [PasswordPropertyText]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections),"", true)]
        public string Password { get; set; }

        /// <summary>
        /// If set, will recreate the directory structure from the SourceDirectory under the TargetDirectory for copied files
        /// </summary>
        public bool PreserveDirectoryStructure { get; set; }

        /// <summary>
        /// If set, will create the target directory if it does not exist,
        /// as well as any sub directories if <see cref="PreserveDirectoryStructure"/> is set.
        /// </summary>
        [DefaultValue(true)]
        public bool CreateTargetDirectories { get; set; }

        /// <summary>
        /// What should happen if a file with the same name already exists in the target directory.
        /// * Throw - Throw an error and roll back all transfers
        /// * Overwrite - Overwrites the target file
        /// * Rename - Renames the transferred file by appending a number to the end
        /// </summary>
        public FileExistsAction IfTargetFileExists { get; set; }
    }
}
