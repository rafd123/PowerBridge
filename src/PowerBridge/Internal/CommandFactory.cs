using System;
using System.Globalization;
using System.Management.Automation.Runspaces;
using System.Text;

namespace PowerBridge.Internal
{
    internal sealed class CommandFactory
    {
        private readonly IFileSystem _fileSystem;
        private string _expression;
        private bool _expressionSpecified;

        private string _file;
        private bool _fileSpecifed;

        private string _arguments;
        private bool _argumentsSpecified;

        public CommandFactory(IFileSystem fileSystem = null)
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

        public string Arguments
        {
            get { return _arguments; }

            set
            {
                _arguments = value ?? string.Empty;
                _argumentsSpecified = true;
            }
        }

        public Command CreateCommand()
        {
            if (_expressionSpecified && _fileSpecifed)
            {
                throw new ArgumentException(Resources.ExpressionAndFileParametersCannotBeUsedSimultaneously);
            }

            if (_expressionSpecified)
            {
                if (_argumentsSpecified)
                {
                    throw new ArgumentException(Resources.ArgumentParameterNotValidWithExpressionParameter);
                }

                return new Command(_expression, isScript: true);
            }

            if (_fileSpecifed)
            {
                if (!_file.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                {
                    var error = string.Format(CultureInfo.CurrentCulture,
                        Resources.PowerShellScriptFileMustBeSpecifiedFormat,
                        _file);

                    throw new ArgumentException(error);
                }

                var filePath = _fileSystem.GetFullPath(_file);
                if (!_fileSystem.FileExists(filePath))
                {
                    var error = string.Format(CultureInfo.CurrentCulture,
                        Resources.PowerShellScriptFileDoesNotExistFormat,
                        _file);

                    throw new ArgumentException(error);
                }

                var commandBuilder = new StringBuilder();
                commandBuilder.Append("& '");
                commandBuilder.Append(filePath);
                commandBuilder.Append('\'');
                if (_argumentsSpecified)
                {
                    commandBuilder.Append(' ');
                    commandBuilder.Append(_arguments);
                }

                return new Command(commandBuilder.ToString(), true);
            }

            throw new ArgumentException(Resources.ExpressionOrFileParameterMustBeSpecified);
        }
    }
}
