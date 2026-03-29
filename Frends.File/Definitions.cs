using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
#pragma warning disable 1591

namespace Frends.File
{
    public class DeleteInput
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
    }

    public class DeleteOption
    {
        /// <summary>
        /// If set, allows you to give the user credentials to use to delete files on remote hosts.
        /// If not set, the agent service user credentials will be used.
        /// Note: For deleting files on the local machine, the agent service user credentials will always be used, even if this option is set.
        /// </summary>
        public bool UseGivenUserCredentialsForRemoteConnections { get; set; }

        /// <summary>
        /// This needs to be of format domain\username
        /// </summary>
        [DefaultValue("\"domain\\username\"")]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string UserName { get; set; }

        [PasswordPropertyText]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string Password { get; set; }
    }

    public class DeleteResult
    {
        public DeleteResult(string path, long fileSizeInBytes)
        {
            Path = path;
            SizeInMegaBytes = fileSizeInBytes / 1024d / 1024d;
        }
        public string Path { get; set; }
        public double SizeInMegaBytes { get; set; }
    }



    public class RenameInput
    {
        /// <summary>
        /// Full path of the file to be renamed.
        /// </summary>
        /// <example>C:\temp\foo.txt</example>
        [DefaultValue("\"c:\\temp\\foo.txt\"")]
        public string Path { get; set; }

        /// <summary>
        /// New filename with file extension. Final filename can differ depending on RenameOptions.RenameBehaviour and can be found from RenameResult.Path. Cannot be empty.
        /// </summary>
        /// <example>bar.txt</example>
        [DefaultValue("\"bar.txt\"")]
        public string NewFileName { get; set; }
    }

    public class RenameOption
    {
        /// <summary>
        /// If set, allows you to give the user credentials to use to rename files on remote hosts.
        /// If not set, the agent service user credentials will be used.
        /// Note: For renaming files on the local machine, the agent service user credentials will always be used, even if this option is set.
        /// </summary>
        public bool UseGivenUserCredentialsForRemoteConnections { get; set; }

        /// <summary>
        /// This needs to be of format domain\username
        /// </summary>
        [DefaultValue("\"domain\\username\"")]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string UserName { get; set; }

        [PasswordPropertyText]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string Password { get; set; }

        /// <summary>
        /// How the file rename should work if a file with the new name already exists.
        /// Throw, an error and roll back all transfers.
        /// Overwrite, the target file.
        /// Rename, the transferred file by appending a number to the end.
        /// </summary>
        public FileExistsAction RenameBehaviour { get; set; }
    }

    public class RenameResult
    {
        public RenameResult(string path)
        {
            Path = path;
        }
        public string Path { get; set; }
    }

    public class FindInput
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
    }

    public class FindOption
    {
        /// <summary>
        /// If set, allows you to give the user credentials to use to search files on remote hosts.
        /// If not set, the agent service user credentials will be used.
        /// Note: For searching files on the local machine, the agent service user credentials will always be used, even if this option is set.
        /// </summary>
        public bool UseGivenUserCredentialsForRemoteConnections { get; set; }

        /// <summary>
        /// This needs to be of format domain\username
        /// </summary>
        [DefaultValue("\"domain\\username\"")]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string UserName { get; set; }

        [PasswordPropertyText]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string Password { get; set; }
    }

    public class FindResult
    {
        public FindResult(FileInfo fileInfo)
        {
            Extension = fileInfo.Extension;
            DirectoryName = fileInfo.DirectoryName;
            FullPath = fileInfo.FullName;
            FileName = fileInfo.Name;
            IsReadOnly = fileInfo.IsReadOnly;
            SizeInMegaBytes = fileInfo.Length / 1024d / 1024d;
            CreationTime = fileInfo.CreationTime;
            CreationTimeUtc = fileInfo.CreationTimeUtc;
            LastAccessTime = fileInfo.LastAccessTime;
            LastAccessTimeUtc = fileInfo.LastAccessTimeUtc;
            LastWriteTime = fileInfo.LastWriteTime;
            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
        }

        public string Extension { get; set; }
        public string DirectoryName { get; set; }
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public bool IsReadOnly { get; set; }
        public double SizeInMegaBytes { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime CreationTimeUtc { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
    }

    public enum WriteBehaviour { Append, Overwrite, Throw }

    public enum FileEncoding { UTF8, ANSI, ASCII, Unicode, Other }

    public class WriteInput
    {
        /// <summary>
        /// Text content to be written to the file
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Full path of the target file to be written
        /// </summary>
        [DefaultValue("\"c:\\temp\\foo.txt\"")]
        public string Path { get; set; }
    }

    public class WriteBytesInput
    {
        /// <summary>
        /// Byte array to be written to the file
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public object ContentBytes { get; set; }

        /// <summary>
        /// Full path of the target file to be written
        /// </summary>
        [DefaultValue("\"c:\\temp\\foo.png\"")]
        public string Path { get; set; }
    }

    public class WriteOption
    {
        /// <summary>
        /// If set, allows you to give the user credentials to use to write files on remote hosts.
        /// If not set, the agent service user credentials will be used.
        /// Note: For writing files on the local machine, the agent service user credentials will always be used, even if this option is set.
        /// </summary>
        public bool UseGivenUserCredentialsForRemoteConnections { get; set; }

        /// <summary>
        /// This needs to be of format domain\username
        /// </summary>
        [DefaultValue("\"domain\\username\"")]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string UserName { get; set; }

        [PasswordPropertyText]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string Password { get; set; }

        /// <summary>
        /// Encoding for the written content. By selecting 'Other' you can use any encoding.
        /// </summary>
        public FileEncoding FileEncoding { get; set; }

        [UIHint(nameof(FileEncoding), "", FileEncoding.UTF8)]
        public bool EnableBom { get; set; }

        /// <summary>
        /// File encoding to be used. A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List
        /// </summary>
        [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
        public string EncodingInString { get; set; }

        /// <summary>
        /// How the file write should work if a file with the new name already exists
        /// </summary>
        public WriteBehaviour WriteBehaviour { get; set; }

        /// <summary>
        /// If set, will create the target directory if it does not exist,
        /// as well as any sub directories.
        /// </summary>
        [DefaultValue(false)]
        public bool CreateTargetDirectories { get; set; }
    }

    public class WriteBytesOption
    {
        /// <summary>
        /// If set, allows you to give the user credentials to use to write files on remote hosts.
        /// If not set, the agent service user credentials will be used.
        /// Note: For writing files on the local machine, the agent service user credentials will always be used, even if this option is set.
        /// </summary>
        public bool UseGivenUserCredentialsForRemoteConnections { get; set; }

        /// <summary>
        /// This needs to be of format domain\username
        /// </summary>
        [DefaultValue("\"domain\\username\"")]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string UserName { get; set; }

        [PasswordPropertyText]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string Password { get; set; }

        /// <summary>
        /// How the file write should work if a file with the new name already exists
        /// </summary>
        public WriteBehaviour WriteBehaviour { get; set; }

        /// <summary>
        /// If set, will create the target directory if it does not exist,
        /// as well as any sub directories.
        /// </summary>
        [DefaultValue(false)]
        public bool CreateTargetDirectories { get; set; }
    }

    public class WriteResult
    {
        public WriteResult(FileInfo info)
        {
            Path = info.FullName;
            SizeInMegaBytes = Math.Round((info.Length / 1024d / 1024d), 3);
        }
        public string Path { get; set; }
        public double SizeInMegaBytes { get; set; }
    }

    public class ReadInput
    {
        /// <summary>
        /// Full path of the file
        /// </summary>
        [DefaultValue("\"c:\\temp\\foo.txt\"")]
        public string Path { get; set; }
    }

    public class ReadOption
    {
        /// <summary>
        /// If set, allows you to give the user credentials to use to read files on remote hosts.
        /// If not set, the agent service user credentials will be used.
        /// Note: For reading files on the local machine, the agent service user credentials will always be used, even if this option is set.
        /// </summary>
        public bool UseGivenUserCredentialsForRemoteConnections { get; set; }

        /// <summary>
        /// This needs to be of format domain\username
        /// </summary>
        [DefaultValue("\"domain\\username\"")]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string UserName { get; set; }

        [PasswordPropertyText]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string Password { get; set; }

        /// <summary>
        /// Encoding for the read content. By selecting 'Other' you can use any encoding.
        /// </summary>
        public FileEncoding FileEncoding { get; set; }

        [UIHint(nameof(FileEncoding), "", FileEncoding.UTF8)]
        public bool EnableBom { get; set; }

        /// <summary>
        /// File encoding to be used. A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List
        /// </summary>
        [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
        public string EncodingInString { get; set; }
    }

    public class ReadBytesOption
    {
        /// <summary>
        /// If set, allows you to give the user credentials to use to read files on remote hosts.
        /// If not set, the agent service user credentials will be used.
        /// Note: For reading files on the local machine, the agent service user credentials will always be used, even if this option is set.
        /// </summary>
        public bool UseGivenUserCredentialsForRemoteConnections { get; set; }

        /// <summary>
        /// This needs to be of format domain\username
        /// </summary>
        [DefaultValue("\"domain\\username\"")]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string UserName { get; set; }

        [PasswordPropertyText]
        [UIHint(nameof(UseGivenUserCredentialsForRemoteConnections), "", true)]
        public string Password { get; set; }
    }

    public class ReadResult
    {
        public ReadResult(FileInfo info, string content)
        {
            Path = info.FullName;
            SizeInMegaBytes = Math.Round((info.Length / 1024d / 1024d), 3);
            Content = content;
            CreationTime = info.CreationTime;
            LastWriteTime = info.LastWriteTime;
        }
        public string Content { get; set; }
        public string Path { get; set; }
        public double SizeInMegaBytes { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
    }

    public class ReadBytesResult
    {
        public ReadBytesResult(FileInfo info, byte[] content)
        {
            Path = info.FullName;
            SizeInMegaBytes = Math.Round((info.Length / 1024d / 1024d), 3);
            ContentBytes = content;
            CreationTime = info.CreationTime;
            LastWriteTime = info.LastWriteTime;
        }
        public byte[] ContentBytes { get; set; }
        public string Path { get; set; }
        public double SizeInMegaBytes { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
    }
}
