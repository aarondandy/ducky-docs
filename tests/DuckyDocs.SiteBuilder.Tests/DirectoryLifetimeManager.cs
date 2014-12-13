using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuckyDocs.SiteBuilder.Tests
{
    internal class DirectoryLifetimeManager : IDisposable
    {

        public static DirectoryLifetimeManager Create()
        {
            var directoryInfo = new DirectoryInfo("./" + Guid.NewGuid().ToString("D"));
            directoryInfo.Create();
            return new DirectoryLifetimeManager(directoryInfo);
        }

        public DirectoryLifetimeManager(DirectoryInfo directory)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            if (!directory.Exists) throw new InvalidOperationException("directory must exist");
            Directory = directory;
        }

        public DirectoryInfo Directory { get; private set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Directory.Exists)
            {
                Directory.Delete(true);
            }
        }

        ~DirectoryLifetimeManager()
        {
            Dispose(false);
        }
    }
}
