using System.Linq;
using System.Management.Automation;

namespace PowerBridge.Internal
{
    internal interface IPowerShellStringProvider
    {
        string ToString(object o);
    }

    internal class PowerShellStringProvider : IPowerShellStringProvider
    {
        private readonly PowerShell _powerShell;

        public PowerShellStringProvider(PowerShell powerShell)
        {
            _powerShell = powerShell;
        }

        public string ToString(object o)
        {
            using (var nestedPowerShell = _powerShell.CreateNestedPowerShell())
            {
                nestedPowerShell.AddCommand("Out-String");
                nestedPowerShell.AddParameter("InputObject", o);
                var result = nestedPowerShell.Invoke<string>();

                if (!result.Any())
                {
                    return string.Empty;
                }

                return result[0];
            }
        }
    }
}