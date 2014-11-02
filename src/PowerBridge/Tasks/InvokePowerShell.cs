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

        public override bool Execute()
        {
            Execute(_commandFactory, new BuildTaskLog(Log));

            return !Log.HasLoggedErrors;
        }

        internal static void Execute(CommandFactory commandFactory, IBuildTaskLog log)
        {
            var command = commandFactory.CreateCommand();

            PowerShellHost.WithPowerShell(log, (shell, output) =>
            {
                shell.Commands.AddCommand(command);

                var outputList = new PowerShellOutputList(output, new PowerShellStringProvider(shell));
                shell.Invoke(null, outputList);
            });  
        }
    }
}
