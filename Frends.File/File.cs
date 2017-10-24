using SimpleImpersonation;
using Frends.Tasks.Attributes;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

#pragma warning disable 1591

namespace Frends.File
{
    public class File
    {
        /// <summary>
        /// Read contents of a file as a string. See: https://github.com/FrendsPlatform/Frends.File#Read
        /// </summary>
        /// <returns>Object {string Content, string Path, double SizeInMegaBytes, DateTime CreationTime, DateTime LastWriteTime }  </returns>
        public static async Task<ReadResult> Read([CustomDisplay(DisplayOption.Tab)] ReadInput input, [CustomDisplay(DisplayOption.Tab)] ReadOption options)
        {
            return await ExecuteActionAsync(
                    () => ExecuteRead(input, options),
                    options.UseGivenUserCredentialsForRemoteConnections,
                    options.UserName,
                    options.Password)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Read contents of a file as a byte array. See: hhttps://github.com/FrendsPlatform/Frends.File#ReadBytes
        /// </summary>
        /// <returns>Object {byte[] ContentBytes, string Path, double SizeInMegaBytes, DateTime CreationTime, DateTime LastWriteTime }  </returns>
        public static async Task<ReadBytesResult> ReadBytes([CustomDisplay(DisplayOption.Tab)] ReadInput input, [CustomDisplay(DisplayOption.Tab)] ReadBytesOption options)
        {
            return await ExecuteActionAsync(
                    () => ExecuteReadBytes(input, options),
                    options.UseGivenUserCredentialsForRemoteConnections,
                    options.UserName,
                    options.Password)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Write string contents to a file. See: https://github.com/FrendsPlatform/Frends.File#Write
        /// </summary>
        /// <returns>Object {string Path, double SizeInMegaBytes}</returns>
        public static async Task<WriteResult> Write([CustomDisplay(DisplayOption.Tab)] WriteInput input, [CustomDisplay(DisplayOption.Tab)] WriteOption options)
        {
            return await ExecuteActionAsync(
                    () => ExecuteWrite(input, options),
                    options.UseGivenUserCredentialsForRemoteConnections,
                    options.UserName,
                    options.Password)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Write byte array to a file. See: https://github.com/FrendsPlatform/Frends.File#WriteBytes
        /// </summary>
        /// <returns>Object {string Path, double SizeInMegaBytes}</returns>
        public static async Task<WriteResult> WriteBytes([CustomDisplay(DisplayOption.Tab)] WriteBytesInput input, [CustomDisplay(DisplayOption.Tab)] WriteBytesOption options)
        {
            return await ExecuteActionAsync(
                    () => ExecuteWriteBytes(input, options),
                    options.UseGivenUserCredentialsForRemoteConnections,
                    options.UserName,
                    options.Password)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get file information for files. See: https://github.com/FrendsPlatform/Frends.File#Find
        /// </summary>
        /// <returns>List [ Object  { string Extension, string DirectoryName, string FullPath,
        /// string FileName, bool IsReadOnly, double SizeInMegaBytes, DateTime CreationTime,
        /// DateTime CreationTimeUtc, DateTime LastAccessTime, DateTime LastAccessTimeUtc, DateTime LastWriteTime, DateTime LastWriteTimeUtc} ]</returns>
        public static List<FindResult> Find([CustomDisplay(DisplayOption.Tab)] FindInput input, [CustomDisplay(DisplayOption.Tab)] FindOption options)
        {
            return ExecuteAction(() => ExecuteFind(input), options.UseGivenUserCredentialsForRemoteConnections, options.UserName, options.Password);
        }

        /// <summary>
        /// Move files. See: https://github.com/FrendsPlatform/Frends.File#Move
        /// </summary>
        /// <returns>List [ Object { string SourcePath, string Path } ]</returns>
        public static async Task<IList<FileInBatchResult>> Move(
            [CustomDisplay(DisplayOption.Tab)] MoveInput input,
            [CustomDisplay(DisplayOption.Tab)] MoveOptions options,
            CancellationToken cancellationToken)
        {
            return await ExecuteActionAsync(() => MoveCommand.ExecuteAsync(input, options, cancellationToken),
                options.UseGivenUserCredentialsForRemoteConnections, options.UserName, options.Password);
        }

        /// <summary>
        /// Copy files. See: https://github.com/FrendsPlatform/Frends.File#Copy
        /// </summary>
        /// <returns>List [ Object { string SourcePath, string Path } ]</returns>
        public static async Task<IList<FileInBatchResult>> Copy(
            [CustomDisplay(DisplayOption.Tab)] CopyInput input,
            [CustomDisplay(DisplayOption.Tab)] CopyOptions options,
            CancellationToken cancellationToken)
        {
            return await ExecuteActionAsync(() => CopyCommand.ExecuteAsync(input, options, cancellationToken),
                options.UseGivenUserCredentialsForRemoteConnections, options.UserName, options.Password).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete files. See: https://github.com/FrendsPlatform/Frends.File#Delete
        /// </summary>
        /// <returns>List [ Object { string Path, string SizeInMegaBytes } ]</returns>
        public static List<DeleteResult> Delete(
            [CustomDisplay(DisplayOption.Tab)] DeleteInput input,
            [CustomDisplay(DisplayOption.Tab)] DeleteOption options,
            CancellationToken cancellationToken)
        {
            return ExecuteAction(() => ExecuteDelete(input, cancellationToken), options.UseGivenUserCredentialsForRemoteConnections, options.UserName, options.Password);
        }

        /// <summary>
        /// Rename a single file. See: https://github.com/FrendsPlatform/Frends.File#Rename
        /// </summary>
        ///  <returns>Object { string Path }</returns>
        public static RenameResult Rename([CustomDisplay(DisplayOption.Tab)] RenameInput input, [CustomDisplay(DisplayOption.Tab)] RenameOption options)
        {
            return ExecuteAction(() => ExecuteRename(input, options.RenameBehaviour), options.UseGivenUserCredentialsForRemoteConnections, options.UserName, options.Password);
        }

        private static async Task<TResult> ExecuteActionAsync<TResult>(Func<Task<TResult>> action, bool useGivenCredentials, string userName, string password)
        {
            if (!useGivenCredentials)
            {
                return await action().ConfigureAwait(false);
            }

            var domainAndUserName = GetDomainAndUserName(userName);
            using (Impersonation.LogonUser(domainAndUserName[0], domainAndUserName[1], password, LogonType.NewCredentials))
            {
                return await action().ConfigureAwait(false);
            }
        }

        private static TResult ExecuteAction<TResult>(Func<TResult> action, bool useGivenCredentials, string userName, string password)
        {
            if (!useGivenCredentials)
            {
                return action();
            }

            var domainAndUserName = GetDomainAndUserName(userName);
            using (Impersonation.LogonUser(domainAndUserName[0], domainAndUserName[1], password, LogonType.NewCredentials))
            {
                return action();
            }
        }

        internal static PatternMatchingResult FindMatchingFiles(string directoryPath, string pattern)
        {
            // Check the user can access the folder
            // This will return false if the path does not exist or you do not have read permissions.
            if (!Directory.Exists(directoryPath))
            {
                throw new Exception($"Directory does not exist or you do not have read access. Tried to access directory '{directoryPath}'");
            }

            var matcher = new Matcher();
            matcher.AddInclude(pattern);
            var results = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(directoryPath)));
            return results;
        }

        #region Executes for that public static tasks.
        private static async Task<ReadResult> ExecuteRead(ReadInput input, ReadOption options)
        {
            var encoding = GetEncoding(options.FileEncoding, options.EnableBom, options.EncodingInString);

            using (var fileStream = new FileStream(input.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            using (var reader = new StreamReader(fileStream, encoding, detectEncodingFromByteOrderMarks: true))
            {
                var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                return new ReadResult(new FileInfo(input.Path), content);
            }
        }

        private static async Task<ReadBytesResult> ExecuteReadBytes(ReadInput input, ReadBytesOption options)
        {
            using (var file = new FileStream(input.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                var buffer = new byte[file.Length];
                await file.ReadAsync(buffer, 0, (int)file.Length).ConfigureAwait(false);

                return new ReadBytesResult(new FileInfo(input.Path), buffer);
            }
        }

        private static List<FindResult> ExecuteFind(FindInput input)
        {
            var results = FindMatchingFiles(input.Directory, input.Pattern);
            var foundFiles = results.Files.Select(match => Path.Combine(input.Directory, match.Path)).ToArray();
            return foundFiles.Select(fullPath => new FindResult(new FileInfo(fullPath))).ToList();
        }

        private static RenameResult ExecuteRename(RenameInput input, FileExistsAction fileExistsAction)
        {
            var directoryPath = Path.GetDirectoryName(input.Path);
            var newFileFullPath = Path.Combine(directoryPath, input.NewFileName);

            switch (fileExistsAction)
            {
                case FileExistsAction.Rename:
                    newFileFullPath = GetNonConflictingDestinationFilePath(input.Path, newFileFullPath);
                    break;
                case FileExistsAction.Overwrite:
                    if (System.IO.File.Exists(newFileFullPath))
                    {
                        System.IO.File.Delete(newFileFullPath);
                    }
                    break;
                case FileExistsAction.Throw:
                    if (System.IO.File.Exists(newFileFullPath))
                    {
                        throw new IOException($"File already exists {newFileFullPath}. No file renamed.");
                    }
                    break;
            }
            System.IO.File.Move(input.Path, newFileFullPath);
            return new RenameResult(newFileFullPath);
        }

        internal static string GetNonConflictingDestinationFilePath(string sourceFilePath, string destFilePath)
        {
            var count = 1;
            while (System.IO.File.Exists(destFilePath))
            {
                string tempFileName = $"{Path.GetFileNameWithoutExtension(sourceFilePath)}({count++})";
                destFilePath = Path.Combine(Path.GetDirectoryName(destFilePath), path2: tempFileName + Path.GetExtension(sourceFilePath));
            }

            return destFilePath;
        }

        private static List<DeleteResult> ExecuteDelete(DeleteInput input, CancellationToken cancellationToken)
        {
            var results = FindMatchingFiles(input.Directory, input.Pattern);

            var fileResults = new List<DeleteResult>();
            try
            {
                foreach (var path in results.Files.Select(match => Path.Combine(input.Directory, match.Path)))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var fileSizeInBytes = new FileInfo(path).Length;
                    fileResults.Add(new DeleteResult(path, fileSizeInBytes));
                    System.IO.File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                var deletedfilesMsg = fileResults.Any() ? ": " + string.Join(",", fileResults.Select(f => f.Path)) : string.Empty;
                throw new Exception($"Could not delete all files. Error: {ex.Message}. Deleted {fileResults.Count} files{deletedfilesMsg}", ex);
            }

            return fileResults;
        }

        private static async Task<WriteResult> ExecuteWrite(WriteInput input, WriteOption options)
        {
            var encoding = GetEncoding(options.FileEncoding, options.EnableBom, options.EncodingInString);
            var fileMode = GetAndCheckWriteMode(options.WriteBehaviour, input.Path);

            using (var fileStream = new FileStream(input.Path, fileMode, FileAccess.Write, FileShare.Write, 4096, useAsync: true))
            using (var writer = new StreamWriter(fileStream, encoding))
            {
                await writer.WriteAsync(input.Content).ConfigureAwait(false);
            }

            return new WriteResult(new FileInfo(input.Path));
        }

        private static async Task<WriteResult> ExecuteWriteBytes(WriteBytesInput input, WriteBytesOption options)
        {
            var bytes = input?.ContentBytes as byte[]; // TODO: Use corrctly typed input once UI support expression default editor for arrays
            if (bytes == null)
            {
                throw new ArgumentException("Input.ContentBytes must be a byte array", nameof(input.ContentBytes));
            }

            var fileMode = GetAndCheckWriteMode(options.WriteBehaviour, input.Path);

            using (var fileStream = new FileStream(input.Path, fileMode, FileAccess.Write, FileShare.Write, 4096, useAsync: true))
            {
                var memoryStream = new MemoryStream(bytes);
                await memoryStream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            return new WriteResult(new FileInfo(input.Path));
        }

        private static FileMode GetAndCheckWriteMode(WriteBehaviour givenWriteBehaviour, string filePath)
        {
            switch (givenWriteBehaviour)
            {
                case WriteBehaviour.Append:
                    return FileMode.Append;

                case WriteBehaviour.Overwrite:
                    return FileMode.Create;

                case WriteBehaviour.Throw:
                    if (System.IO.File.Exists(filePath))
                    {
                        throw new IOException($"File already exists: {filePath}");
                    }
                    return FileMode.Create;
                default:
                    throw new ArgumentException("Unsupported write option: " + givenWriteBehaviour);
            }
        }

        #endregion

        private static Encoding GetEncoding(FileEncoding optionsFileEncoding, bool optionsEnableBom, string optionsEncodingInString)
        {
            switch (optionsFileEncoding)
            {
                case FileEncoding.Other:
                    return Encoding.GetEncoding(optionsEncodingInString);
                case FileEncoding.ASCII:
                    return Encoding.ASCII;
                case FileEncoding.ANSI:
                    return Encoding.Default;
                case FileEncoding.UTF8:
                    return optionsEnableBom ? new UTF8Encoding(true) : new UTF8Encoding(false);
                case FileEncoding.Unicode:
                    return Encoding.Unicode;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static void DeleteExistingFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                if (System.IO.File.Exists(file))
                {
                    System.IO.File.Delete(file); // TODO: Add error handling?
                }
            }
        }

        private static string[] GetDomainAndUserName(string username)
        {
            var domainAndUserName = username.Split('\\');
            if (domainAndUserName.Length != 2)
            {
                throw new ArgumentException($@"UserName field must be of format domain\username was: {username}");
            }
            return domainAndUserName;
        }
    }
}
