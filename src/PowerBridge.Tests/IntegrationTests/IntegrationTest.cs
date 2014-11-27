using System.IO;
using System.Runtime.CompilerServices;

namespace PowerBridge.Tests.IntegrationTests
{
    public abstract class IntegrationTest
    {
        protected static string GetTestResourceFilePath(string testResource, [CallerFilePath] string sourceFilePath = "")
        {
            sourceFilePath = NormalizePath(sourceFilePath);

            return Path.Combine(
                // ReSharper disable once AssignNullToNotNullAttribute
                Path.GetDirectoryName(sourceFilePath),
                Path.GetFileNameWithoutExtension(sourceFilePath) + "Resources",
                testResource);
        }

        private static string NormalizePath(string path)
        {
            // Capitalize the drive is necessary
            if (Path.IsPathRooted(path))
            {
                if (path[0] != char.ToUpperInvariant(path[0]))
                {
                    path = char.ToUpperInvariant(path[0]) + path.Substring(1);
                }
            }

            return path;
        }
    }
}