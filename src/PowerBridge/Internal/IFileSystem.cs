using System.IO;

namespace PowerBridge.Internal
{
    internal interface IFileSystem
    {
        bool FileExists(string filePath);

        string GetFullPath(string path);
    }

    internal sealed class FileSystem : IFileSystem
    {
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}