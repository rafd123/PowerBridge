using System.Management.Automation;
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
            var info = errorRecord.GetErrorInfo();

            _log.LogError(
                subcategory: null,
                errorCode: null,
                helpKeyword: null,
                file: info.File,
                lineNumber: info.LineNumber,
                columnNumber: info.ColumnNumber,
                endLineNumber: 0,
                endColumnNumber: 0,
                message: info.Message);
        }
    }
}