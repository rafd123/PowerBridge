using System.Linq;
using System.Management.Automation;

namespace PowerBridge.Internal
{
    internal interface IPowerShellCallStackProvider
    {
        CallStackFrame GetCurrentCallStackFrame();
    }

    internal class PowerShellCallStackProvider : IPowerShellCallStackProvider
    {
        private readonly PowerShell _powerShell;

        public PowerShellCallStackProvider(PowerShell powerShell)
        {
            _powerShell = powerShell;
        }

        public CallStackFrame GetCurrentCallStackFrame()
        {
            using (var nestedPowerShell = _powerShell.CreateNestedPowerShell())
            {
                nestedPowerShell.AddCommand("Get-PSCallStack");
                var powerShellCallStack = nestedPowerShell.Invoke<CallStackFrame>();

                if (!powerShellCallStack.Any())
                {
                    return null;
                }

                return powerShellCallStack[0];
            }
        }
    }
}