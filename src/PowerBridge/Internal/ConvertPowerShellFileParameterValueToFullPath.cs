using System;
using System.Globalization;

namespace PowerBridge.Internal
{
    internal static class ConvertPowerShellFileParameterValueToFullPath
    {
        public static string Execute(string fileParameterValue, IFileSystem fileSystem)
        {
            if (string.IsNullOrEmpty(fileParameterValue))
            {
                throw new ArgumentException(Resources.FileParameterMustBeSpecified);
            }

            if (!fileParameterValue.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                var error = string.Format(CultureInfo.CurrentCulture,
                    Resources.PowerShellScriptFileMustBeSpecifiedFormat,
                    fileParameterValue);

                throw new ArgumentException(error);
            }

            var filePath = fileSystem.GetFullPath(fileParameterValue);
            if (!fileSystem.FileExists(filePath))
            {
                var error = string.Format(CultureInfo.CurrentCulture,
                    Resources.PowerShellScriptFileDoesNotExistFormat,
                    fileParameterValue);

                throw new ArgumentException(error);
            }

            return filePath;
        }
    }
}