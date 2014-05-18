using System;
using System.Globalization;
using System.Management.Automation.Runspaces;

namespace PowerBridge.Internal
{
    internal sealed class ExecuteParameters
    {
        private readonly IFileSystem _fileSystem;
        private string _expression;
        private bool _expressionSpecified;

        private string _file;
        private bool _fileSpecifed;

        public ExecuteParameters(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public string Expression
        {
            get { return _expression; }

            set
            {
                _expression = value ?? string.Empty;
                _expressionSpecified = true;
            }
        }

        public string File
        {
            get { return _file; }

            set
            {
                _file = value ?? string.Empty;
                _fileSpecifed = true;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "PowerBridge.Internal.IBuildTaskLog.LogError(System.String)")]
        public bool TryGetCommand(IBuildTaskLog taskLog, out Command command)
        {
            if (taskLog == null)
            {
                throw new ArgumentNullException("taskLog");
            }

            command = null;

            if (_expressionSpecified && _fileSpecifed)
            {
                taskLog.LogError("You cannot specify both the Expression and File parameters simultaneously.");
                return false;
            }

            if (_expressionSpecified)
            {
                command = new Command(_expression, isScript: true);
                return true;
            }

            if (_fileSpecifed)
            {
                if (!_file.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                {
                    var error = string.Format(CultureInfo.CurrentCulture,
                        "Processing File '{0}' failed because the file does not have a '.ps1' extension. Specify a valid Windows PowerShell script file name, and then try again.",
                        _file);

                    taskLog.LogError(error);
                    return false;
                }

                var filePath = _fileSystem.GetFullPath(_file);
                if (!_fileSystem.FileExists(filePath))
                {
                    var error = string.Format(CultureInfo.CurrentCulture,
                        "The argument '{0}' to the File parameter does not exist. Provide the path to an existing '.ps1' file as an argument to the File parameter.",
                        _file);

                    taskLog.LogError(error);
                    return false;
                }

                command = new Command(filePath, false);
                return true;
            }

            taskLog.LogError("Either the Expression or File parameter must be specified.");
            return false;
        }
    }
}
