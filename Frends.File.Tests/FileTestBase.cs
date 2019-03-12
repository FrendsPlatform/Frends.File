
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

        protected FileTestBase()
        {
            TestFileContext = new DisposableFileSystem();
        }


        public void Dispose()
        {
            TestFileContext?.Dispose();
            TestFileContext = null;
        }
    }
}
