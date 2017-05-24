using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.File.Tests
{
    public abstract class FileTestBase : IDisposable
    {
        protected DisposableFileSystem TestFileContext;

        [SetUp]
        public void Setup()
        {
            TestFileContext = new DisposableFileSystem();
        }

        [TearDown]
        public void ClearContext()
        {
            TestFileContext?.Dispose();
            TestFileContext = null;
        }

        public void Dispose()
        {
            TestFileContext?.Dispose();
        }
    }
}
