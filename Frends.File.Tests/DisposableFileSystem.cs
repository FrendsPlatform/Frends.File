﻿using System;
using System.IO;

namespace Frends.File.Tests
{
    public class DisposableFileSystem : IDisposable
    {
        public DisposableFileSystem()
        {
            RootPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(RootPath);
            DirectoryInfo = new DirectoryInfo(RootPath);
        }

        public string RootPath { get; }

        public DirectoryInfo DirectoryInfo { get; }

        public DisposableFileSystem CreateFolder(string path)
        {
            Directory.CreateDirectory(Path.Combine(RootPath, path));
            return this;
        }

        public DisposableFileSystem CreateFile(string path, string content)
        {
            string filePath = Path.Combine(RootPath, path);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            System.IO.File.WriteAllText(filePath, content);
            return this;
        }

        public DisposableFileSystem CreateBinaryFile(string path, byte[] contentBytes)
        {
            var filePath = Path.Combine(RootPath, path);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            System.IO.File.WriteAllBytes(filePath, contentBytes);

            return this;
        }

        public DisposableFileSystem CreateFiles(params string[] fileRelativePaths)
        {
            foreach (var path in fileRelativePaths)
            {
                var fullPath = Path.Combine(RootPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                System.IO.File.WriteAllText(
                    fullPath,
                    string.Format("Automatically generated for testing on {0:yyyy}/{0:MM}/{0:dd} {0:hh}:{0:mm}:{0:ss}", DateTime.UtcNow));
            }

            return this;
        }

        public bool FileExists(string path)
        {
            return System.IO.File.Exists(Path.Combine(RootPath, path));
        }

        /// <summary>
        /// Returns the given path rooted to the context root path
        /// </summary>
        public string GetAbsolutePath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(RootPath, relativePath));
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(RootPath, true);
            }
            catch
            {
                // Don't throw if this fails.
            }
        }
    }
}
