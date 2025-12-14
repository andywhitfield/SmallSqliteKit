namespace SmallSqliteKit.Service.Tests.Services;

public static class FilesystemExtensions
{
    public static IEnumerable<string> ToFilenames(this IEnumerable<string> filePaths) => [.. filePaths.Select(Path.GetFileName).Where(x => x != null).Cast<string>()];

    public static IEnumerable<string> GetFilenames(this DirectoryInfo directory)
    {
        directory.Refresh();
        return directory.EnumerateFiles().Select(f => f.Name);
    }
}