using System;
using System.Management.Automation.Runspaces;

namespace PowerBridge.Internal
{
    internal sealed class ExecuteParameters
    {
        private string _expression;
        private bool _expressionSpecified;

        public string Expression
        {
            get { return _expression; }

            set
            {
                _expression = value ?? string.Empty;
                _expressionSpecified = true;
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

            if (_expressionSpecified)
            {
                command = new Command(_expression, isScript: true);
                return true;
            }

            taskLog.LogError("The Expression parameter must be specified.");
            return false;
        }
    }
}