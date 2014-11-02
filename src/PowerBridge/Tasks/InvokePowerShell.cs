using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
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

        internal static void Execute(CommandFactory commandFactory, IBuildTaskLog taskLog)
        {
            var command = commandFactory.CreateCommand();

            Environment.SetEnvironmentVariable("PSExecutionPolicyPreference", "Bypass");

            using (var powerShell = PowerShell.Create())
            using (var powerShellOutput = new PowerShellHostOutput(taskLog, new PowerShellCallStackProvider(powerShell)))
            {
                var host = new PowerShellHost(powerShellOutput);
                using (var runspace = RunspaceFactory.CreateRunspace(host))
                {
                    powerShell.Runspace = runspace;
                    powerShell.Streams.Error.DataAdded += (sender, args) =>
                    {
                        powerShellOutput.WriteError(powerShell.Streams.Error[args.Index]);
                    };

                    runspace.Open();
                    powerShell.Commands.AddCommand(command);

                    try
                    {
                        var output = new PowerShellOutputList(powerShellOutput, new PowerShellStringProvider(powerShell));
                        powerShell.Invoke(null, output);
                    }
                    catch (RuntimeException e)
                    {
                        if (e.ErrorRecord == null)
                        {
                            throw;
                        }

                        powerShellOutput.WriteError(e.ErrorRecord);
                    }                    
                }
            }   
        }
    }
}
