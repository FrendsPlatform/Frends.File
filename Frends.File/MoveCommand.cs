using Frends.Tasks.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591 // Disable missing documentation warnings

namespace Frends.File
{
    public static class MoveCommand
    {
        public static async Task<IList<FileInBatchResult>> ExecuteAsync(MoveInput input, MoveOptions moveOptions, CancellationToken cancellationToken)
        {
            var result = await FileBatchCommand.ExecuteAsync(input, moveOptions, MoveFilesAsync, cancellationToken, actionNameInErrors: "moved");

            // delete source files 
            File.DeleteExistingFiles(result.Select(x => x.SourcePath));

            return result;
        }

        public static async Task MoveFilesAsync(string sourceFilePath, string targetFilePath, CancellationToken cancellationToken)
        {
            // reuse copy, source files will be deleted after all have finished
            // TODO: for move between directories on the same disk, the File.Move would be more performant, but also could not be rolled back 
            await CopyCommand.CopyFileAsync(sourceFilePath, targetFilePath, cancellationToken).ConfigureAwait(false);
        }
    }

    public class MoveInput : IFileBatchInput
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
        /// Target directory where the found files should be moved
        /// </summary>
        [DefaultValue("\"d:\\backup\"")]
        public string TargetDirectory { get; set; }
    }

    public class MoveOptions : IFileBatchOptions
    {
        /// <summary>
        /// If set, allows you to give the user credentials to use to move files on remote hosts. 
        /// If not set, the agent service user credentials will be used.
        /// Note: For moving files on the local machine, the agent service user credentials will always be used, even if this option is set.
        /// </summary>
        public bool UseGivenUserCredentialsForRemoteConnections { get; set; }

        /// <summary>
        /// This needs to be of format domain\username
        /// </summary>
        [DefaultValue("\"domain\\username\"")]
        [ConditionalDisplay(nameof(UseGivenUserCredentialsForRemoteConnections), true)]
        public string UserName { get; set; }

        [PasswordPropertyText]
        [ConditionalDisplay(nameof(UseGivenUserCredentialsForRemoteConnections), true)]
        public string Password { get; set; }

        /// <summary>
        /// If set, will recreate the directory structure from the SourceDirectory under the TargetDirectory for moved files
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
