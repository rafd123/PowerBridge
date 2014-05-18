using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.Build.Utilities;
using PowerBridge.Internal;

namespace PowerBridge.Tasks
{
    public class InvokePowerShell : AppDomainIsolatedTask
    {
        private readonly ExecuteParameters _parameters = new ExecuteParameters();

        public string Expression
        {
            get { return _parameters.Expression; }
            set { _parameters.Expression = value; }
        }

        public string File
        {
            get { return _parameters.File; }
            set { _parameters.File = value; }
        }

        public override bool Execute()
        {
            Execute(_parameters, new BuildTaskLog(Log));

            return !Log.HasLoggedErrors;
        }

        internal static void Execute(ExecuteParameters parameters, IBuildTaskLog taskLog)
        {
            Command command;
            if (!parameters.TryGetCommand(taskLog, out command))
            {
                return;
            }

            Environment.SetEnvironmentVariable("PSExecutionPolicyPreference", "Bypass");

            using (var powerShellOutput = new PowerShellHostOutput(taskLog))
            {
                var host = new PowerShellHost(powerShellOutput);
                using (var runspace = RunspaceFactory.CreateRunspace(host))
                using (var powerShell = PowerShell.Create())
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
                        powerShell.Invoke();
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
