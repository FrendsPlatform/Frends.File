using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Threading;

namespace Frends.File.Tests
{
    [TestFixture]
    public class UnitTest : FileTestBase
    {
        [SetUp]
        public void CreateFileContext()
        {
            TestFileContext.CreateFiles(
                    "folder/foo/sub/test.xml",
                    "folder/bar/sub/example.xml");
        }

        [Test]
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

            Assert.That(results.Count, Is.EqualTo(2));
        }

        [Test]
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

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task FileMoveOverWrite()
        {
            const string contentForFileToBeOverwritten = "firstFile";
            TestFileContext.CreateFile("folder/test.xml", contentForFileToBeOverwritten);
            var createdFile = System.IO.File.ReadAllText(Path.Combine(TestFileContext.RootPath, "folder/test.xml"));
            Assert.That(createdFile, Is.EqualTo(contentForFileToBeOverwritten));

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
            Assert.That(overWrittenFIle, Is.Not.EqualTo(contentForFileToBeOverwritten));
            Assert.That(results.Count.Equals(2));

            var destinationFilesLength = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder")).Length;
            Assert.That(destinationFilesLength, Is.EqualTo(2));

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

            Assert.That(secondMoveShouldBeEmpty, Is.Empty);
        }

        [Test]
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

            Assert.That(results.Count, Is.EqualTo(2));
            var originalFile = System.IO.File.ReadAllText(Path.Combine(TestFileContext.RootPath, "folder\\test.xml"));
            var copiedFile = System.IO.File.ReadAllText(Path.Combine(TestFileContext.RootPath, "folder\\test(1).xml"));
            Assert.That(originalFile, Is.EqualTo(contentForOriginalFile));
            Assert.That(copiedFile, Does.StartWith("Automatically generated for testing on"));

            var destinationFilesLength = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder")).Length;
            Assert.That(destinationFilesLength, Is.EqualTo(3));

            var secondMoveShouldBeEmpty = await File.Move(
                new MoveInput()
                {
                    Directory = TestFileContext.RootPath,
                    Pattern = "**/sub/*.xml",
                    TargetDirectory = Path.Combine(TestFileContext.RootPath, "folder")
                },
                new MoveOptions() { IfTargetFileExists = FileExistsAction.Rename },
                CancellationToken.None);
            Assert.That(secondMoveShouldBeEmpty, Is.Empty);
        }

        [Test]
        public void FileMoveThrowShouldThrowIfFIleExistsAtDestination()
        {
            const string contentForOriginalFile = "firstFile";
            TestFileContext.CreateFile("folder/test.xml", contentForOriginalFile);

            var ex = Assert.ThrowsAsync<IOException>(async () => await File.Move(
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

            Assert.That(ex.Message, Is.EqualTo($"File '{Path.Combine(TestFileContext.RootPath, "folder\\test.xml")}' already exists. No files moved."));

            var originalFile = System.IO.File.ReadAllText(Path.Combine(TestFileContext.RootPath, "folder\\test.xml"));
            Assert.That(originalFile, Is.EqualTo(contentForOriginalFile));

            var destinationFilesLength = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder")).Length;
            Assert.That(destinationFilesLength, Is.EqualTo(1));

            var sourceFolder1 = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder/foo/sub/")).Length;
            Assert.That(sourceFolder1, Is.EqualTo(1));

            var sourceFolder2 = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder/bar/sub/")).Length;
            Assert.That(sourceFolder2, Is.EqualTo(1));
        }

        [Test]
        public void RenameFile()
        {
            var resultsOverWrite = File.Rename(
                new RenameInput()
                {
                    Path = Path.Combine(TestFileContext.RootPath, "folder/foo/sub/test.xml"),
                    NewFileName = "newTest.xml"
                },
                new RenameOption()
                {
                    RenameBehaviour = FileExistsAction.Overwrite
                });

            Assert.That(resultsOverWrite.Path, Is.EqualTo(Path.Combine(TestFileContext.RootPath, "folder\\foo\\sub\\newTest.xml")));

            var resultsCopy = File.Rename(
                new RenameInput()
                {
                    Path = Path.Combine(TestFileContext.RootPath, "folder/foo/sub/newTest.xml"),
                    NewFileName = "newTest.xml"
                },
                new RenameOption()
                {
                    RenameBehaviour = FileExistsAction.Rename
                });

            Assert.That(resultsCopy.Path, Is.EqualTo(Path.Combine(TestFileContext.RootPath, "folder\\foo\\sub\\newTest(1).xml")));

            var results = File.Rename(
                new RenameInput()
                {
                    Path = Path.Combine(TestFileContext.RootPath, "folder/foo/sub/newTest(1).xml"),
                    NewFileName = "newTest.xml"
                },
                new RenameOption()
                {
                    RenameBehaviour = FileExistsAction.Throw
                });

            Assert.That(results.Path, Is.EqualTo(Path.Combine(TestFileContext.RootPath, "folder\\foo\\sub\\newTest.xml")));
            var folderFilesLength = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder/foo/sub/")).Length;
            Assert.That(folderFilesLength, Is.EqualTo(1));
            TestFileContext.CreateFile("folder/foo/sub/throwTest.xml", "temp");

            var ex = Assert.Throws<IOException>(() => File.Rename(
                new RenameInput()
                {
                    Path = Path.Combine(TestFileContext.RootPath, "folder/foo/sub/newTest.xml"),
                    NewFileName = "throwTest.xml"
                },
                new RenameOption()
                {
                    RenameBehaviour = FileExistsAction.Throw
                }));

            Assert.That(ex.Message, Does.Contain("throwTest.xml"));

            folderFilesLength = Directory.GetFiles(Path.Combine(TestFileContext.RootPath, "folder/foo/sub/")).Length;
            Assert.That(folderFilesLength, Is.EqualTo(2));
        }

        [Test]
        public void FindFiles()
        {
            TestFileContext.CreateFiles("folder/foo/test.txt",
                "folder/foo/sub/test3.txt",
                "folder/foo/sub/test.txt",
                "folder/bar/sub/example2.json");
            var results = File.Find(new FindInput() { Directory = TestFileContext.RootPath, Pattern = "**/*.xml" }, new FindOption());
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.All(x => x.Extension.Equals(".xml")));
        }

        [Test]
        public void FindFilesShouldThrowIfFolderNotExist()
        {
            var ex = Assert.Throws<Exception>(() => File.Find(
                new FindInput()
                {
                    Directory = "DoesNotExist",
                    Pattern = "**.*"
                },
                new FindOption()));

            Assert.That(ex.Message, Does.Contain("Directory does not exist or you do not have read access"));
        }

        [Test]
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
            Assert.That(fileContent, Is.EqualTo("old contentnew content"));
        }

        [Test]
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
            Assert.That(fileContent, Is.EqualTo("new content"));
        }

        [Test]
        public void WriteFileThrow()
        {
            TestFileContext.CreateFile("test.txt", "old content");
            var ex = Assert.ThrowsAsync<IOException>(async () => await File.Write(
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
            Assert.That(fileContent, Is.EqualTo("old content"));
            Assert.That(ex.Message, Is.EqualTo($"File already exists: {Path.Combine(TestFileContext.RootPath, "test.txt")}"));
        }

        [Test]
        public async Task ReadFileContent()
        {
            TestFileContext.CreateFile("Folder/test.txt", "Well this is content hi");
            var result = await File.Read(new ReadInput() { Path = Path.Combine(TestFileContext.RootPath, "folder/test.txt") }, new ReadOption() { });
            Assert.That(result.Content, Is.EqualTo("Well this is content hi"));
        }


        [Test]
        public async Task WriteReadFileWithLatin1()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ansi.txt"); //ansi is Latin1 here
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
            Assert.That(fileContent, Is.Not.EqualTo("ÅÖÄåöÄ"));
            fileContent = System.IO.File.ReadAllText(result.Path, Encoding.GetEncoding("Latin1"));
            Assert.That(fileContent, Is.EqualTo("ÅÖÄåöä"));
        }

        [Test]
        public async Task WriteReadFileWithUtf8NoBom()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "utf8nobom.txt");
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
            Assert.That(fileContent, Is.EqualTo("ÅÖÄåöä"));
        }



    }
}
