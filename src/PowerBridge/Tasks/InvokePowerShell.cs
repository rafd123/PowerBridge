using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using PowerBridge.Internal;

namespace PowerBridge.Tasks
{
    public class InvokePowerShell : AppDomainIsolatedTask
    {
        private readonly CommandFactory _commandFactory = new CommandFactory();

        public string Expression
        {
            get { return _commandFactory.Expression; }
            set { _commandFactory.Expression = value; }
        }

        public string File
        {
            get { return _commandFactory.File; }
            set { _commandFactory.File = value; }
        }

        public string Arguments
        {
            get { return _commandFactory.Arguments; }
            set { _commandFactory.Arguments = value; }
        }

        public ITaskItem[] AutoParameters
        {
            get { return _commandFactory.AutoParameters; }
            set { _commandFactory.AutoParameters = value; }
        }

        private bool UseFSharpLineNumbersOffByOneQuirkMode
        {
            get
            {
                return (AutoParameters ?? Enumerable.Empty<ITaskItem>())
                    .Any(x => string.Equals(x.ItemSpec, "PowerBridgeUseFSharpLineNumbersOffByOneQuirkMode", StringComparison.OrdinalIgnoreCase) &&
                              x.GetMetadata("Value") != null &&
                              string.Equals(x.GetMetadata("Value").Trim(), "true", StringComparison.OrdinalIgnoreCase));
            }
        }

        public override bool Execute()
        {
            Execute(_commandFactory, new BuildTaskLog(Log, useFSharpLineNumbersOffByOneQuirkMode: UseFSharpLineNumbersOffByOneQuirkMode));

            return !Log.HasLoggedErrors;
        }

        internal static void Execute(CommandFactory commandFactory, IBuildTaskLog log)
        {
            PowerShellHost.WithPowerShell(log, (shell, output) =>
            {
                var command = commandFactory.CreateCommand(new PowerShellCommandParameterProvider());

                shell.Commands.AddCommand(command);

                var outputList = new PowerShellOutputList(output, new PowerShellStringProvider(shell));
                shell.Invoke(null, outputList);
            });  
        }
    }
}
