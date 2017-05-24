using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Frends.File.Tests
{
    [TestFixture]
    public class CopyTest : FileTestBase
    {
        [Test]
        public async Task ShouldCopySingleFile()
        {
            TestFileContext.CreateFile("dir/sub/test.txt", "testing");

            var results = await File.Copy(
                new CopyInput
                {
                    Directory = TestFileContext.GetAbsolutePath("dir/sub"),
                    Pattern = "test.txt",
                    TargetDirectory = TestFileContext.GetAbsolutePath("out")
                },
                new CopyOptions { CreateTargetDirectories = true },
                CancellationToken.None);

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results[0];
            Assert.That(result.Path, Is.EqualTo(TestFileContext.GetAbsolutePath("out/test.txt")));
            Assert.That(result.SourcePath, Is.EqualTo(TestFileContext.GetAbsolutePath("dir/sub/test.txt")));

            Assert.That(TestFileContext.FileExists("out/test.txt"), "Output file should have been written");
        }

        [Test]
        public void ShouldThrowErrorIfTargetDirectoryDoesNotExist()
        {
            TestFileContext.CreateFile("dir/sub/test.txt", "testing");

            var error = Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await File.Copy(
                new CopyInput
                {
                    Directory = TestFileContext.GetAbsolutePath("dir/sub"),
                    Pattern = "test.txt",
                    TargetDirectory = TestFileContext.GetAbsolutePath("out")
                },
                new CopyOptions { CreateTargetDirectories = false },
                CancellationToken.None));

            Assert.That(error.Message, Does.Contain(TestFileContext.GetAbsolutePath("out")));
        }

        [Test]
        public async Task ShouldNotCopyFilesIfNoMatch()
        {
            TestFileContext.CreateFile("dir/sub/test.txt", "testing");

            var results = await File.Copy(
                new CopyInput
                {
                    Directory = TestFileContext.GetAbsolutePath("dir"),
                    Pattern = "**/*.xml",
                    TargetDirectory = TestFileContext.GetAbsolutePath("out")
                },
                new CopyOptions { CreateTargetDirectories = true },
                CancellationToken.None);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ShouldCopyFilesFromSubDirectories()
        {
            TestFileContext.CreateFile("dir/sub/test1.txt", "testing");
            TestFileContext.CreateFile("dir/sub/test2.txt", "testing");
            TestFileContext.CreateFile("dir/sub/other1.xml", "testing");
            TestFileContext.CreateFile("dir/sub/nestedSub/test3.txt", "testing");
            TestFileContext.CreateFile("dir/sub/nestedSub/other2.xml", "testing");

            var results = await File.Copy(
                new CopyInput
                {
                    Directory = TestFileContext.GetAbsolutePath("dir"),
                    Pattern = "**/*.txt",
                    TargetDirectory = TestFileContext.GetAbsolutePath("out"),
                },
                new CopyOptions { CreateTargetDirectories = true },
                CancellationToken.None);

            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results.Select(r => Path.GetFileName(r.Path)), Is.EquivalentTo(new[] { "test1.txt", "test2.txt", "test3.txt" }));

            Assert.That(TestFileContext.FileExists("out/test1.txt"), "Output file should have been written");
            Assert.That(TestFileContext.FileExists("out/test2.txt"), "Output file should have been written");
            Assert.That(TestFileContext.FileExists("out/test3.txt"), "Output file should have been written");
        }

        [Test]
        public async Task ShouldCopyFilesFromSubDirectoriesPreservingDirectoryStructure()
        {
            TestFileContext.CreateFile("dir/test1.txt", "testing");
            TestFileContext.CreateFile("dir/sub/test2.txt", "testing");
            TestFileContext.CreateFile("dir/sub/nestedSub/test3.txt", "testing");

            var results = await File.Copy(
                new CopyInput
                {
                    Directory = TestFileContext.GetAbsolutePath("dir"),
                    Pattern = "**/*.txt",
                    TargetDirectory = TestFileContext.GetAbsolutePath("out"),
                },
                new CopyOptions
                {
                    CreateTargetDirectories = true,
                    PreserveDirectoryStructure = true
                },
                CancellationToken.None);

            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results.Select(r => r.Path), Is.EquivalentTo(new[] {
                TestFileContext.GetAbsolutePath("out/test1.txt"),
                TestFileContext.GetAbsolutePath("out/sub/test2.txt"),
                TestFileContext.GetAbsolutePath("out/sub/nestedSub/test3.txt")
            }));

            Assert.That(TestFileContext.FileExists("out/test1.txt"), "Output file should have been written");
            Assert.That(TestFileContext.FileExists("out/sub/test2.txt"), "Output file should have been written");
            Assert.That(TestFileContext.FileExists("out/sub/nestedSub/test3.txt"), "Output file should have been written");
        }

        [Test]
        public async Task ShouldOverwriteExistingFiles()
        {
            var expectedFileContents = "testing " + DateTime.UtcNow.ToString("o");

            TestFileContext.CreateFile("source/test1.txt", "testing");
            TestFileContext.CreateFile("source/test2.txt", expectedFileContents);
            TestFileContext.CreateFile("target/test1.txt", "existing");

            var results = await File.Copy(
                new CopyInput
                {
                    Directory = TestFileContext.GetAbsolutePath("source"),
                    Pattern = "*.txt",
                    TargetDirectory = TestFileContext.GetAbsolutePath("target")
                },
                new CopyOptions
                {
                    CreateTargetDirectories = true,
                    IfTargetFileExists = FileExistsAction.Overwrite
                },
                CancellationToken.None);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.Select(r => r.Path), Is.EquivalentTo(new[] {
                TestFileContext.GetAbsolutePath("target/test1.txt"),
                TestFileContext.GetAbsolutePath("target/test2.txt")
            }));

            Assert.That(TestFileContext.FileExists("target/test1.txt"), "Output file should have been written");
            Assert.That(TestFileContext.FileExists("target/test2.txt"), "Output file should have been written");

            var resultFileContents = System.IO.File.ReadAllText(TestFileContext.GetAbsolutePath("target/test2.txt"));
            Assert.That(resultFileContents, Is.EqualTo(expectedFileContents));
        }

        [Test]
        public async Task ShouldOverwriteExistingFilesInSubDirectories()
        {
            var expectedFileContents = "testing " + DateTime.UtcNow.ToString("o");

            TestFileContext.CreateFile("source/test.txt", "testing");
            TestFileContext.CreateFile("source/sub/test.txt", expectedFileContents);
            TestFileContext.CreateFile("target/sub/test.txt", "existing");

            var results = await File.Copy(
                new CopyInput
                {
                    Directory = TestFileContext.GetAbsolutePath("source"),
                    Pattern = "**/*.txt",
                    TargetDirectory = TestFileContext.GetAbsolutePath("target")
                },
                new CopyOptions
                {
                    PreserveDirectoryStructure = true,
                    CreateTargetDirectories = true,
                    IfTargetFileExists = FileExistsAction.Overwrite
                },
                CancellationToken.None);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.Select(r => r.Path), Is.EquivalentTo(new[] {
                TestFileContext.GetAbsolutePath("target/test.txt"),
                TestFileContext.GetAbsolutePath("target/sub/test.txt")
            }));

            Assert.That(TestFileContext.FileExists("target/sub/test.txt"), "Output file should have been written");

            var resultFileContents = System.IO.File.ReadAllText(TestFileContext.GetAbsolutePath("target/sub/test.txt"));
            Assert.That(resultFileContents, Is.EqualTo(expectedFileContents));
        }

        [Test]
        public async Task ShouldRenameFilesIfTargetFilesExist()
        {
            TestFileContext.CreateFile("source/test.txt", "testing");
            TestFileContext.CreateFile("source/sub/test.txt", "testing");
            TestFileContext.CreateFile("source/otherSub/test.txt", "testing");
            TestFileContext.CreateFile("target/test.txt", "existing");
            TestFileContext.CreateFile("target/test(1).txt", "existing");

            var results = await File.Copy(
                new CopyInput
                {
                    Directory = TestFileContext.GetAbsolutePath("source"),
                    Pattern = "**/*.txt",
                    TargetDirectory = TestFileContext.GetAbsolutePath("target")
                },
                new CopyOptions
                {
                    PreserveDirectoryStructure = false,
                    CreateTargetDirectories = true,
                    IfTargetFileExists = FileExistsAction.Rename
                },
                CancellationToken.None);

            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results.Select(r => r.Path), Is.EquivalentTo(new[] {
                TestFileContext.GetAbsolutePath("target/test(2).txt"),
                TestFileContext.GetAbsolutePath("target/test(3).txt"),
                TestFileContext.GetAbsolutePath("target/test(4).txt")
            }));

            Assert.That(TestFileContext.FileExists("target/test(2).txt"), "Output file should have been written");
            Assert.That(TestFileContext.FileExists("target/test(3).txt"), "Output file should have been written");
            Assert.That(TestFileContext.FileExists("target/test(4).txt"), "Output file should have been written");
        }

        [Test]
        public async Task ShouldRenameFilesAndPreserveDirectoryStructureIfTargetFilesExist()
        {
            TestFileContext.CreateFile("source/test.txt", "testing");
            TestFileContext.CreateFile("source/sub/test.txt", "testing");
            TestFileContext.CreateFile("target/sub/test.txt", "existing");

            var results = await File.Copy(
                new CopyInput
                {
                    Directory = TestFileContext.GetAbsolutePath("source"),
                    Pattern = "**/*.txt",
                    TargetDirectory = TestFileContext.GetAbsolutePath("target")
                },
                new CopyOptions
                {
                    PreserveDirectoryStructure = true,
                    CreateTargetDirectories = true,
                    IfTargetFileExists = FileExistsAction.Rename
                },
                CancellationToken.None);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.Select(r => r.Path), Is.EquivalentTo(new[] {
                TestFileContext.GetAbsolutePath("target/test.txt"),
                TestFileContext.GetAbsolutePath("target/sub/test(1).txt")
            }));

            Assert.That(TestFileContext.FileExists("target/sub/test(1).txt"), "Output file should have been written");
        }

        [Test]
        public void ShouldThrowAndRollbackIfTargetFilesExist()
        {
            TestFileContext.CreateFile("source/test.txt", "testing");
            TestFileContext.CreateFile("source/sub/test.txt", "testing");
            TestFileContext.CreateFile("target/sub/test.txt", "existing");

            Assert.ThrowsAsync<IOException>(async () => await File.Copy(
                new CopyInput
                {
                    Directory = TestFileContext.GetAbsolutePath("source"),
                    Pattern = "**/*.txt",
                    TargetDirectory = TestFileContext.GetAbsolutePath("target")
                },
                new CopyOptions
                {
                    PreserveDirectoryStructure = true,
                    CreateTargetDirectories = true,
                    IfTargetFileExists = FileExistsAction.Throw
                },
                CancellationToken.None));

            Assert.That(!TestFileContext.FileExists("target/test.txt"), "Output file should have been rolled back");
            Assert.That(!TestFileContext.FileExists("target/test(2).txt"), "Output file should have been rolled back");
        }
    }
}

