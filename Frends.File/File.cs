using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using SimpleImpersonation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Frends.File
{
    public class File
    {
        /// <summary>
        /// Read contents of a file as a string. See: https://github.com/FrendsPlatform/Frends.File#Read
        /// </summary>
        /// <returns>Object {string Content, string Path, double SizeInMegaBytes, DateTime CreationTime, DateTime LastWriteTime }  </returns>
        public static async Task<ReadResult> Read([PropertyTab] ReadInput input, [PropertyTab] ReadOption options)
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
        public static async Task<ReadBytesResult> ReadBytes([PropertyTab] ReadInput input, [PropertyTab] ReadBytesOption options)
        {
            return await ExecuteActionAsync(
                    () => ExecuteReadBytes(input),
                    options.UseGivenUserCredentialsForRemoteConnections,
                    options.UserName,
                    options.Password)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Write string contents to a file. See: https://github.com/FrendsPlatform/Frends.File#Write
        /// </summary>
        /// <returns>Object {string Path, double SizeInMegaBytes}</returns>
        public static async Task<WriteResult> Write([PropertyTab] WriteInput input, [PropertyTab] WriteOption options)
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
        public static async Task<WriteResult> WriteBytes([PropertyTab] WriteBytesInput input, [PropertyTab] WriteBytesOption options)
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
        public static List<FindResult> Find([PropertyTab] FindInput input, [PropertyTab] FindOption options)
        {
            return ExecuteAction(() => ExecuteFind(input), options.UseGivenUserCredentialsForRemoteConnections, options.UserName, options.Password);
        }

        /// <summary>
        /// Move files. See: https://github.com/FrendsPlatform/Frends.File#Move
        /// </summary>
        /// <returns>List [ Object { string SourcePath, string Path } ]</returns>
        public static async Task<IList<FileInBatchResult>> Move(
            [PropertyTab] MoveInput input,
            [PropertyTab] MoveOptions options,
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
            [PropertyTab] CopyInput input,
            [PropertyTab] CopyOptions options,
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
            [PropertyTab] DeleteInput input,
            [PropertyTab] DeleteOption options,
            CancellationToken cancellationToken)
        {
            return ExecuteAction(() => ExecuteDelete(input, cancellationToken), options.UseGivenUserCredentialsForRemoteConnections, options.UserName, options.Password);
        }

        /// <summary>
        /// Rename a single file. See: https://github.com/FrendsPlatform/Frends.File#Rename
        /// </summary>
        ///  <returns>Object { string Path }</returns>
        public static RenameResult Rename([PropertyTab] RenameInput input, [PropertyTab] RenameOption options)
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
#if (NET461 || NET471)
            return await Impersonation.RunAsUser(
                            new UserCredentials(domainAndUserName[0], domainAndUserName[1], password), LogonType.NewCredentials,
                            async () => await action().ConfigureAwait(false));
     
#else
            throw new PlatformNotSupportedException("Impersonation not supported for this platform. Only works on full framework.");
#endif

        }

        private static TResult ExecuteAction<TResult>(Func<TResult> action, bool useGivenCredentials, string userName, string password)
        {
            if (!useGivenCredentials)
            {
                return action();
            }

            var domainAndUserName = GetDomainAndUserName(userName);

            return Impersonation.RunAsUser(new UserCredentials(domainAndUserName[0], domainAndUserName[1], password),
                LogonType.NewCredentials, action);
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

        private static async Task<ReadBytesResult> ExecuteReadBytes(ReadInput input)
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
            if (!System.IO.File.Exists(input.Path))
                throw new FileNotFoundException($@"Rename operation cancelled. File {input.Path} not found.");
            if (string.IsNullOrEmpty(input.NewFileName))
                throw new ArgumentNullException(@"Rename operation cancelled. Parameter 'NewFileName' cannot be empty.");

            var newFileFullPath = Path.Combine(Path.GetDirectoryName(input.Path), input.NewFileName);

            if (System.IO.File.Exists(newFileFullPath))
            {
                switch (fileExistsAction)
                {
                    case FileExistsAction.Rename:
                        newFileFullPath = GetNonConflictingDestinationFilePath(null, newFileFullPath);
                        System.IO.File.Move(input.Path, newFileFullPath);
                        break;
                    case FileExistsAction.Overwrite:
                        System.IO.File.Copy(input.Path, newFileFullPath, true);
                        var orgFileInfo = new FileInfo(input.Path);
                        var newFileInfo = new FileInfo(newFileFullPath);

                        if (orgFileInfo.Length != newFileInfo.Length)
                            throw new Exception($@"The original file, {orgFileInfo.FullName}, was successfully copied to {newFileInfo.FullName} for overwrite operation. However, the original file was not deleted because these files do not match in size.");
                        else
                            System.IO.File.Delete(orgFileInfo.FullName);
                        break;
                    case FileExistsAction.Throw:
                        throw new IOException($"File already exists {newFileFullPath}. No file renamed.");
                }
            }
            else
                System.IO.File.Move(input.Path, newFileFullPath);

            return new RenameResult(newFileFullPath);
        }

        internal static string GetNonConflictingDestinationFilePath(string sourceFilePath, string destFilePath)
        {
            var count = 1;
            while (System.IO.File.Exists(destFilePath))
            {
                string tempFileName = $"{Path.GetFileNameWithoutExtension(!string.IsNullOrEmpty(sourceFilePath) ? sourceFilePath : destFilePath)}({count++})";
                destFilePath = Path.Combine(Path.GetDirectoryName(destFilePath), path2: tempFileName + Path.GetExtension(!string.IsNullOrEmpty(sourceFilePath) ? sourceFilePath : destFilePath));
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

            if(options.CreateTargetDirectories)
            {
                var directoryPath = Path.GetDirectoryName(input.Path);
                if(!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }

            using (var fileStream = new FileStream(input.Path, fileMode, FileAccess.Write, FileShare.Write, 4096, useAsync: true))
            using (var writer = new StreamWriter(fileStream, encoding))
            {
                await writer.WriteAsync(input.Content).ConfigureAwait(false);
            }

            return new WriteResult(new FileInfo(input.Path));
        }

        private static async Task<WriteResult> ExecuteWriteBytes(WriteBytesInput input, WriteBytesOption options)
        {
            if (options.CreateTargetDirectories)
            {
                var directoryPath = Path.GetDirectoryName(input.Path);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            var bytes = input?.ContentBytes as byte[] ?? throw new ArgumentException("Input.ContentBytes must be a byte array", nameof(input.ContentBytes)); // TODO: Use corrctly typed input once UI support expression default editor for arrays

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
