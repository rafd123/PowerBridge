using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using Microsoft.Build.Framework;

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

        private ITaskItem[] _autoParameters;
        private bool _autoParametersSpecified;

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

        public ITaskItem[] AutoParameters
        {
            get { return _autoParameters; }

            set
            {
                _autoParameters = value;
                _autoParametersSpecified = true;
            }
        }

        public Command CreateCommand(IPowerShellCommandParameterProvider commandParameterProvider)
        {
            if (_expressionSpecified && _fileSpecifed)
            {
                throw new ArgumentException(Resources.ExpressionAndFileParametersCannotBeUsedSimultaneously);
            }

            if (_expressionSpecified)
            {
                return CreateExpressionCommand();
            }

            if (_fileSpecifed)
            {
                return CreateFileCommand(commandParameterProvider);
            }

            throw new ArgumentException(Resources.ExpressionOrFileParameterMustBeSpecified);
        }

        private Command CreateExpressionCommand()
        {
            if (_argumentsSpecified)
            {
                throw new ArgumentException(Resources.ArgumentsParameterNotValidWithExpressionParameter);
            }

            if (_autoParametersSpecified)
            {
                throw new ArgumentException(Resources.AutoParametersParameterNotValidWithExpressionParameter);
            }

            return new Command(_expression, isScript: true);
        }

        private Command CreateFileCommand(IPowerShellCommandParameterProvider commandParameterProvider)
        {
            if (_argumentsSpecified && _autoParametersSpecified)
            {
                throw new ArgumentException(Resources.ArgumentsAndAutoParametersParametersCannotBeUsedSimultaneously);
            }

            if (!_file.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                var error = string.Format(CultureInfo.CurrentCulture,
                    Resources.PowerShellScriptFileMustBeSpecifiedFormat,
                    _file);

                throw new ArgumentException(error);
            }

            var filePath = ConvertPowerShellFileParameterValueToFullPath.Execute(_file, _fileSystem);

            return _autoParametersSpecified
                ? CreateFileCommandWithAutoParameters(filePath, commandParameterProvider)
                : CreateFileCommand(filePath);
        }

        private Command CreateFileCommand(string filePath)
        {
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

        private Command CreateFileCommandWithAutoParameters(
            string filePath,
            IPowerShellCommandParameterProvider commandParameterProvider)
        {
            var command = new Command(filePath, isScript: false);
            foreach (var kvp in GetCommandParameterValuesThatMatchAutoParameters(filePath, commandParameterProvider))
            {
                command.Parameters.Add(kvp.Key, kvp.Value);
            }

            return command;
        }

        private IEnumerable<KeyValuePair<string, string>> GetCommandParameterValuesThatMatchAutoParameters(
            string commandName,
            IPowerShellCommandParameterProvider commandParameterProvider)
        {
            var parameterNames = commandParameterProvider.GetDefaultParameterSetParameterNames(commandName);
            var autoParametersLookup = AutoParameters
                .Select(x => new
                {
                    ParameterName = x.ItemSpec,
                    ParameterValue = x.GetMetadata("Value")
                })
                .Where(x => !string.IsNullOrEmpty(x.ParameterValue))
                .ToLookup(x => x.ParameterName, x => x.ParameterValue, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Last());

            foreach (var parameterName in parameterNames)
            {
                string parameterValue;
                if (autoParametersLookup.TryGetValue(parameterName, out parameterValue))
                {
                    yield return new KeyValuePair<string, string>(parameterName, parameterValue);
                }
            }
        }
    }
}
