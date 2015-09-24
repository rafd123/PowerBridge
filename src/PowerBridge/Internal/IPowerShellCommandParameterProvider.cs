using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerBridge.Internal
{
    internal interface IPowerShellCommandParameterProvider
    {
        string[] GetDefaultParameterSetParameterNames(string command);
    }

    internal class PowerShellCommandParameterProvider : IPowerShellCommandParameterProvider
    {
        public string[] GetDefaultParameterSetParameterNames(string commandName)
        {
            using (var powerShell = PowerShell.Create(RunspaceMode.NewRunspace))
            {
                var command = new Command("Get-Command");
                command.Parameters.Add("Name", commandName);
                powerShell.Commands.AddCommand(command);

                var commandInfo = powerShell.Invoke<CommandInfo>()[0];

                var parameterSet = commandInfo.ParameterSets.Count == 1
                    ? commandInfo.ParameterSets[0]
                    : commandInfo.ParameterSets.FirstOrDefault(x => x.IsDefault);

                if (parameterSet == null)
                {
                    throw new InvalidOperationException(commandName + " does not have a default parameter set.");
                }

                return parameterSet.Parameters.Select(x => x.Name).ToArray();
            }
        }
    }
}