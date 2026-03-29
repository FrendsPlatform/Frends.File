using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Frends.File.Tests
{

    public class UnitTest : FileTestBase
    {
        public UnitTest()
        {
            TestFileContext.CreateFiles(
                    "folder/foo/sub/test.xml",
                    "folder/foo/sub/rename.xml",
                    "folder/bar/sub/example.xml");
        }

        [Fact]
        public void FileDeleteWithPatternMatching()
        {
            var results = File.Delete(
                new DeleteInput()
                {
                    Directory = TestFileContext.RootPath,
                    Pattern = "**/*.xml"
                },
                new DeleteOption() { },
                CancellationToken.None);

            Assert.Equal(3, results.Count);
        }

        [Fact]
        public void FileDeleteShouldNotThrowIfNoFilesFound()
        {
            var results = File.Delete(
                new DeleteInput()
                {
                    Directory = TestFileContext.RootPath,
                    Pattern = "**/*.unknown"
                },
                new DeleteOption() { },
                CancellationToken.None);

            Assert.Empty(results);
        }

        [Fact]
        public async Task FileMoveOverWrite()
        {
            const string contentForFileToBeOverwritten = "firstFile";
            TestFileContext.CreateFile("folder/test.xml", contentForFileToBeOverwritten);
            var createdFile = System.IO.File.ReadAllText(Path.Combine(TestFileContext.RootPath, "folder/test.xml"));
            Assert.Equal(contentForFileToBeOverwritten, createdFile);

            var results = await File.Move(
                new MoveInput()
                {
                    Directory = TestFileContext.RootPath,
                    Pattern = "**/sub/*.xml",
                    TargetDirectory = Path.Combine(TestFileContext.RootPath, "folder")
                },
                new MoveOptions()
                {
                    IfTargetFileExists = FileExistsAction.Overwrite
                },
                CancellationToken.None);

            var overWrittenFIle = System.IO.File.ReadAllText(Path.Combine(TestFileContext.RootPath, "folder/test.xml"));
            Assert.NotEqual(contentForFileToBeOverwritten, overWrittenFIle);
            Assert.Equal(3, results.Count);

            var destinationFilesLength = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder")).Length;
            Assert.Equal(3, destinationFilesLength);

            var secondMoveShouldBeEmpty = await File.Move(
                new MoveInput()
                {
                    Directory = TestFileContext.RootPath,
                    Pattern = "**/sub/*.xml",
                    TargetDirectory = Path.Combine(TestFileContext.RootPath, "folder")
                },
                new MoveOptions()
                {
                    IfTargetFileExists = FileExistsAction.Overwrite
                },
                CancellationToken.None);

            Assert.Empty(secondMoveShouldBeEmpty);
        }

        [Fact]
        public async Task FileMoveCopy()
        {
            const string contentForOriginalFile = "firstFile";
            TestFileContext.CreateFile("folder/test.xml", contentForOriginalFile);

            var results = await File.Move(
                new MoveInput()
                {
                    Directory = TestFileContext.RootPath,
                    Pattern = "**/sub/*.xml",
                    TargetDirectory = Path.Combine(TestFileContext.RootPath, "folder")
                },
                new MoveOptions() { IfTargetFileExists = FileExistsAction.Rename },
                CancellationToken.None);

            Assert.Equal(3, results.Count);
            var originalFile = System.IO.File.ReadAllText(Path.Combine(TestFileContext.RootPath, "folder\\test.xml"));
            var copiedFile = System.IO.File.ReadAllText(Path.Combine(TestFileContext.RootPath, "folder\\test(1).xml"));
            Assert.Equal(contentForOriginalFile, originalFile);
            Assert.StartsWith("Automatically generated for testing on", copiedFile);

            var destinationFilesLength = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder")).Length;
            Assert.Equal(4, destinationFilesLength);

            var secondMoveShouldBeEmpty = await File.Move(
                new MoveInput()
                {
                    Directory = TestFileContext.RootPath,
                    Pattern = "**/sub/*.xml",
                    TargetDirectory = Path.Combine(TestFileContext.RootPath, "folder")
                },
                new MoveOptions() { IfTargetFileExists = FileExistsAction.Rename },
                CancellationToken.None);
            Assert.Empty(secondMoveShouldBeEmpty);
        }

        [Fact]
        public async Task FileMoveThrowShouldThrowIfFIleExistsAtDestination()
        {
            const string contentForOriginalFile = "firstFile";
            TestFileContext.CreateFile("folder/test.xml", contentForOriginalFile);

            var ex = await Assert.ThrowsAsync<IOException>(async () => await File.Move(
                new MoveInput()
                {
                    Directory = TestFileContext.RootPath,
                    Pattern = "**/sub/*.xml",
                    TargetDirectory = Path.Combine(TestFileContext.RootPath, "folder")
                },
                new MoveOptions()
                {
                    IfTargetFileExists = FileExistsAction.Throw
                },
                CancellationToken.None));

            Assert.Equal($"File '{Path.Combine(TestFileContext.RootPath, "folder\\test.xml")}' already exists. No files moved.", ex.Message);

            var originalFile = System.IO.File.ReadAllText(Path.Combine(TestFileContext.RootPath, "folder\\test.xml"));
            Assert.Equal(contentForOriginalFile, originalFile);

            var destinationFilesLength = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder"));
            Assert.Single(destinationFilesLength);

            var sourceFolder1 = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder/foo/sub/"));
            Assert.Equal(2, sourceFolder1.Length);

            var sourceFolder2 = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder/bar/sub/"));
            Assert.Single(sourceFolder2);
        }

        [Fact]
        public void RenameFile_Overwrite_DestinationFileExists()
        {
            var input = new RenameInput()
            {
                Path = Path.Combine(TestFileContext.RootPath, "folder/foo/sub/test.xml"),
                NewFileName = "rename.xml"
            };

            var options = new RenameOption()
            {
                RenameBehaviour = FileExistsAction.Overwrite
            };

            var result = File.Rename(input, options);

            Assert.Contains("rename.xml", result.Path);
        }

        [Fact]
        public void RenameFile_Overwrite_DestinationFileNotExists()
        {
            var input = new RenameInput()
            {
                Path = Path.Combine(TestFileContext.RootPath, "folder/foo/sub/test.xml"),
                NewFileName = "rename.xml"
            };

            var options = new RenameOption()
            {
                RenameBehaviour = FileExistsAction.Overwrite
            };

            System.IO.File.Delete(Path.Combine(TestFileContext.RootPath, "folder/foo/sub/rename.xml"));

            var result = File.Rename(input, options);
            Assert.Contains("rename.xml", result.Path);
        }

        [Fact]
        public void RenameFile_Overwrite_NewNameNotSet()
        {
            var input = new RenameInput()
            {
                Path = Path.Combine(TestFileContext.RootPath, "folder/foo/sub/test.xml"),
                NewFileName = null,
            };

            var options = new RenameOption()
            {
                RenameBehaviour = FileExistsAction.Overwrite
            };

            System.IO.File.Delete(Path.Combine(TestFileContext.RootPath, "folder/foo/sub/rename.xml"));

            Assert.Throws<ArgumentNullException>(() => File.Rename(input, options));
        }

        [Fact]
        public void RenameFile_Rename_DestinationFileExists()
        {
            var input = new RenameInput()
            {
                Path = Path.Combine(TestFileContext.RootPath, "folder/foo/sub/test.xml"),
                NewFileName = "rename.xml"
            };

            var options = new RenameOption()
            {
                RenameBehaviour = FileExistsAction.Rename
            };

            var result = File.Rename(input, options);

            Assert.Contains("rename(1).xml", result.Path);
        }

        [Fact]
        public void RenameFile_Rename_DestinationFileNotExists()
        {
            var input = new RenameInput()
            {
                Path = Path.Combine(TestFileContext.RootPath, "folder/foo/sub/test.xml"),
                NewFileName = "rename.xml"
            };

            var options = new RenameOption()
            {
                RenameBehaviour = FileExistsAction.Rename
            };

            System.IO.File.Delete(Path.Combine(TestFileContext.RootPath, "folder/foo/sub/rename.xml"));
            var result = File.Rename(input, options);
            Assert.Contains("rename.xml", result.Path);
        }

        [Fact]
        public void RenameFile_Throw()
        {
            var input = new RenameInput()
            {
                Path = Path.Combine(TestFileContext.RootPath, "folder/foo/sub/test.xml"),
                NewFileName = "rename.xml"
            };

            var options = new RenameOption()
            {
                RenameBehaviour = FileExistsAction.Throw
            };

            Assert.Throws<IOException>(() => File.Rename(input, options));
        }

        [Fact]
        public void FindFiles()
        {
            TestFileContext.CreateFiles("folder/foo/test.txt",
                "folder/foo/sub/test3.txt",
                "folder/foo/sub/test.txt",
                "folder/bar/sub/example2.json");
            var results = File.Find(new FindInput() { Directory = TestFileContext.RootPath, Pattern = "**/*.xml" }, new FindOption());
            Assert.Equal(3, results.Count);
            Assert.True(results.All(x => x.Extension.Equals(".xml")));
        }

        [Fact]
        public void FindFilesShouldThrowIfFolderNotExist()
        {
            var ex = Assert.Throws<Exception>(() => File.Find(
                new FindInput()
                {
                    Directory = "DoesNotExist",
                    Pattern = "**.*"
                },
                new FindOption()));

            Assert.Contains("Directory does not exist or you do not have read access", ex.Message);
        }

        [Fact]
        public async Task WriteFileAppend()
        {
            TestFileContext.CreateFile("test.txt", "old content");
            var result = await File.Write(
                new WriteInput()
                {
                    Content = "new content",
                    Path = Path.Combine(TestFileContext.RootPath, "test.txt")
                },
                new WriteOption()
                {
                    WriteBehaviour = WriteBehaviour.Append
                });

            var fileContent = System.IO.File.ReadAllText(result.Path);
            Assert.Equal("old contentnew content", fileContent);
        }

        [Fact]
        public async Task WriteFileOverWrite()
        {
            TestFileContext.CreateFile("test.txt", "old content");
            var result = await File.Write(
                new WriteInput()
                {
                    Content = "new content",
                    Path = Path.Combine(TestFileContext.RootPath, "test.txt")
                },
                new WriteOption()
                {
                    WriteBehaviour = WriteBehaviour.Overwrite
                });

            var fileContent = System.IO.File.ReadAllText(result.Path);
            Assert.Equal("new content", fileContent);
        }

        [Fact]
        public async Task WriteFileThrow()
        {
            TestFileContext.CreateFile("test.txt", "old content");
            var ex = await Assert.ThrowsAsync<IOException>(async () => await File.Write(
                new WriteInput()
                {
                    Content = "new content",
                    Path = Path.Combine(TestFileContext.RootPath, "test.txt")
                },
                new WriteOption()
                {
                    WriteBehaviour = WriteBehaviour.Throw
                }));

            var fileContent = System.IO.File.ReadAllText(Path.Combine(TestFileContext.RootPath, "test.txt"));
            Assert.Equal("old content", fileContent);
            Assert.Equal($"File already exists: {Path.Combine(TestFileContext.RootPath, "test.txt")}", ex.Message);
        }
        [Fact]
        public async Task WriteFileCreateDirectory()
        {
            var result = await File.Write(
                new WriteInput()
                {
                    Content = "Created with a subdirectory",
                    Path = Path.Combine(TestFileContext.RootPath, "folder/Subdir/subdir.txt")
                },
                new WriteOption()
                {
                    CreateTargetDirectories = true
                });
            var fileContent = System.IO.File.ReadAllText(result.Path);
            Console.WriteLine(result.Path);
            Assert.Equal("Created with a subdirectory", fileContent);
        }

        [Fact]
        public async Task WriteFileCreateDirectoryThrow()
        {
            var result = await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await File.Write(
                new WriteInput()
                {
                    Content = "Task should throw",
                    Path = Path.Combine(TestFileContext.RootPath, "folder/Subdir/subdir.txt")
                },
                new WriteOption()
                {
                    CreateTargetDirectories = false
                }));
            
        }

        [Fact]
        public async Task WriteFileBytesAppend()
        {
            var imageBytes = System.IO.File.ReadAllBytes(BinaryTestFilePath);

            TestFileContext.CreateBinaryFile("test.png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }); // empty png
            var result = await File.WriteBytes(
                new WriteBytesInput()
                {
                    ContentBytes = imageBytes,
                    Path = Path.Combine(TestFileContext.RootPath, "test.png")
                },
                new WriteBytesOption()
                {
                    WriteBehaviour = WriteBehaviour.Append
                });

            var fileContentBytes = System.IO.File.ReadAllBytes(result.Path);

            Assert.Equal(8 + imageBytes.Length, fileContentBytes.Length);
        }

        [Fact]
        public async Task WriteFileBytesOverwrite()
        {
            var imageBytes = System.IO.File.ReadAllBytes(BinaryTestFilePath);

            TestFileContext.CreateBinaryFile("test.png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }); // empty png
            var result = await File.WriteBytes(
                new WriteBytesInput()
                {
                    ContentBytes = imageBytes,
                    Path = Path.Combine(TestFileContext.RootPath, "test.png")
                },
                new WriteBytesOption()
                {
                    WriteBehaviour = WriteBehaviour.Overwrite
                });

            var fileContentBytes = System.IO.File.ReadAllBytes(result.Path);

            Assert.Equal(imageBytes.Length, fileContentBytes.Length);
            Assert.Equal(imageBytes, fileContentBytes);
        }

        [Fact]
        public async Task WriteFileBytesThrow()
        {
            var imageBytes = System.IO.File.ReadAllBytes(BinaryTestFilePath);

            TestFileContext.CreateBinaryFile("test.png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }); // empty png
            var result = await File.WriteBytes(
                new WriteBytesInput()
                {
                    ContentBytes = imageBytes,
                    Path = Path.Combine(TestFileContext.RootPath, "test.png")
                },
                new WriteBytesOption()
                {
                    WriteBehaviour = WriteBehaviour.Overwrite
                });

            var fileContentBytes = System.IO.File.ReadAllBytes(result.Path);

            Assert.Equal(imageBytes.Length, fileContentBytes.Length);
            Assert.Equal(imageBytes, fileContentBytes);
        }

        [Fact]
        public async Task WriteFileBytesCreateDirectory()
        {
            var imageBytes = System.IO.File.ReadAllBytes(BinaryTestFilePath);
            TestFileContext.CreateBinaryFile("test.png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }); // empty png
            var result = await File.WriteBytes(
                new WriteBytesInput()
                {
                    ContentBytes = imageBytes,
                    Path = Path.Combine(TestFileContext.RootPath, "folder/Subdir/test.png")
                },
                new WriteBytesOption()
                {
                    CreateTargetDirectories = true,
                });
            var fileContentBytes = System.IO.File.ReadAllBytes(result.Path);
            Assert.Equal(imageBytes.Length, fileContentBytes.Length);
            Assert.Equal(imageBytes, fileContentBytes);
        }
        [Fact]
        public async Task WriteFileBytesCreateDirectoryThrow()
        {
            var imageBytes = System.IO.File.ReadAllBytes(BinaryTestFilePath);
            TestFileContext.CreateBinaryFile("test.png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }); // empty png
            var result = await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await File.WriteBytes(
                new WriteBytesInput()
                {
                    ContentBytes = imageBytes,
                    Path = Path.Combine(TestFileContext.RootPath, "folder/Subdir/test.png")
                },
                new WriteBytesOption()
                {
                    CreateTargetDirectories = false,
                }));
        }

        [Fact]
        public async Task ReadFileContent()
        {
            var fileContent = "Well this is content with some extra nice ümlauts: ÄÖåå 你好!";
            TestFileContext.CreateFile("Folder/test.txt", fileContent);
            var result = await File.Read(new ReadInput() { Path = Path.Combine(TestFileContext.RootPath, "folder/test.txt") }, new ReadOption() { });
            Assert.Equal(fileContent, result.Content);
        }

        [Fact]
        public async Task ReadFileContentBytes()
        {
            var result = await File.ReadBytes(new ReadInput() { Path = BinaryTestFilePath }, new ReadBytesOption() { });

            var expectedData = System.IO.File.ReadAllBytes(BinaryTestFilePath);

            Assert.Equal(expectedData.Length, result.ContentBytes.Length);
            Assert.Equal(expectedData, result.ContentBytes);
        }

        [Fact]
        public async Task WriteReadFileWithLatin1()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles/ansi.txt"); //ansi is Latin1 here
            var res = await File.Read(
                new ReadInput()
                {
                    Path = path
                },
                new ReadOption()
                {
                    FileEncoding = FileEncoding.Other,
                    EncodingInString = "Latin1"
                });

            var result = await File.Write(new WriteInput() { Content = res.Content, Path = Path.Combine(TestFileContext.RootPath, "test.txt") }, new WriteOption() { FileEncoding = FileEncoding.Other, EncodingInString = "Latin1", WriteBehaviour = WriteBehaviour.Append });

            var fileContent = System.IO.File.ReadAllText(result.Path); //Without encoding it will use UTF-8 and the text will be scrambled
            Assert.NotEqual("ÅÖÄåöÄ", fileContent);
            fileContent = System.IO.File.ReadAllText(result.Path, Encoding.GetEncoding("Latin1"));
            Assert.Equal("ÅÖÄåöä", fileContent);
        }

        [Fact]
        public async Task WriteReadFileWithUtf8NoBom()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles/utf8nobom.txt");
            var res = await File.Read(
                new ReadInput()
                {
                    Path = path
                },
                new ReadOption()
                {
                    FileEncoding = FileEncoding.UTF8,
                    EnableBom = false
                });

            var result = await File.Write(
                new WriteInput()
                {
                    Content = res.Content,
                    Path = Path.Combine(TestFileContext.RootPath, "test.txt")
                },
                new WriteOption()
                {
                    FileEncoding = FileEncoding.UTF8,
                    EnableBom = false
                });

            var fileContent = System.IO.File.ReadAllText(result.Path, Encoding.UTF8);
            Assert.Equal("ÅÖÄåöä", fileContent);
        }

        private static string BinaryTestFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles/frends_favicon.png");
    }
}
