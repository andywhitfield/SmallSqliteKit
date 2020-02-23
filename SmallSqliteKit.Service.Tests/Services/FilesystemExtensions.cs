using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmallSqliteKit.Service.Tests.Services
{
    public static class FilesystemExtensions
    {
        public static IEnumerable<string> ToFilenames(this IEnumerable<string> filePaths) => filePaths.Select(Path.GetFileName).ToList();

        public static IEnumerable<string> GetFilenames(this DirectoryInfo directory)
        {
            directory.Refresh();
            return directory.EnumerateFiles().Select(f => f.Name);
        }
    }
}