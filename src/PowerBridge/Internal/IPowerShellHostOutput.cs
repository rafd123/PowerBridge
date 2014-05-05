using System;
using System.Management.Automation;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;

namespace PowerBridge.Internal
{
    internal interface IPowerShellHostOutput
    {
        void WriteDebugLine(string message);

        void WriteVerboseLine(string message);

        void Write(string message);

        void WriteLine(string value);

        void WriteWarningLine(string value);

        void WriteErrorLine(string value);

        void WriteError(ErrorRecord errorRecord);         
    }

    internal sealed class PowerShellHostOutput : IPowerShellHostOutput
    {
        private readonly IBuildTaskLog _log;

        public PowerShellHostOutput(IBuildTaskLog log)
        {
            _log = log;
        }

        public void WriteDebugLine(string message)
        {
            _log.LogMessage(MessageImportance.Low, message);
        }

        public void WriteVerboseLine(string message)
        {
            _log.LogMessage(MessageImportance.Low, message);
        }

        public void Write(string message)
        {
            _log.LogMessage(MessageImportance.High, message);
        }

        public void WriteLine(string value)
        {
            Write(value);
        }

        public void WriteWarningLine(string value)
        {
            _log.LogWarning(value);
        }

        public void WriteErrorLine(string value)
        {
            _log.LogError(value);
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            // The ErrorRecord's invocation info doesn't necessarily
            // contain the information where the error was thrown but
            // rather what call was made that eventually let to the error.
            // Nonetheless, let's use it as a fallback.
            var file = errorRecord.InvocationInfo.ScriptName;
            var lineNumber = errorRecord.InvocationInfo.ScriptLineNumber;
            var message = errorRecord + Environment.NewLine + errorRecord.ScriptStackTrace;

            // Ideally, we'd be able to use a System.Management.Automation.CallStackFrame
            // objects in order to determine where the error was thrown, however the
            // ErrorRecord object doesn't expose them. As a result, we have to resort
            // to parsing the ErrorRecord.ScriptStackTrace.
            var match = Regex.Match(errorRecord.ScriptStackTrace, @".*, (?<file>.+): line (?<line>[\d]+)");
            if (match.Success)
            {
                file = match.Groups["file"].Value;
                lineNumber = int.Parse(match.Groups["line"].Value);
            }

            _log.LogError(
                subcategory: null,
                errorCode: null,
                helpKeyword: null,
                file: file,
                lineNumber: lineNumber,
                columnNumber: 0,
                endLineNumber: 0,
                endColumnNumber: 0,
                message: message);
        }
    }
}