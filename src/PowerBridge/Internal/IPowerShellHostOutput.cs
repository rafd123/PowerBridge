using System;
using System.Management.Automation;
using System.Text;
using Microsoft.Build.Framework;

namespace PowerBridge.Internal
{
    internal interface IPowerShellHostOutput : IDisposable
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
        private readonly IPowerShellCallStackProvider _callStackProvider;
        private readonly StringBuilder _writeBuffer = new StringBuilder();

        public PowerShellHostOutput(IBuildTaskLog log, IPowerShellCallStackProvider callStackProvider)
        {
            _log = log;
            _callStackProvider = callStackProvider;
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
            var flushBuffer = false;

            if (message.EndsWith("\r\n", StringComparison.Ordinal))
            {
                message = message.Substring(0, message.Length - 2);
                flushBuffer = true;
            }
            else if (message.EndsWith("\n", StringComparison.Ordinal))
            {
                message = message.Substring(0, message.Length - 1);
                flushBuffer = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                _writeBuffer.Append(message);
            }

            if (flushBuffer && _writeBuffer.Length != 0)
            {
                _log.LogMessage(MessageImportance.High, _writeBuffer.ToString());
                _writeBuffer.Clear();
            }
        }

        public void WriteLine(string value)
        {
            Write(value);
            Write("\n");
        }

        public void WriteWarningLine(string value)
        {
            var currentCallStackFrame = _callStackProvider.GetCurrentCallStackFrame();
            var info = LogEntryInfo.FromMessage(value, currentCallStackFrame);

            _log.LogWarning(
                subcategory: null,
                warningCode: null,
                helpKeyword: null,
                file: info.File,
                lineNumber: info.LineNumber,
                columnNumber: info.ColumnNumber,
                endLineNumber: 0,
                endColumnNumber: 0,
                message: info.Message);
        }

        public void WriteErrorLine(string value)
        {
            var currentCallStackFrame = _callStackProvider.GetCurrentCallStackFrame();
            var info = LogEntryInfo.FromMessage(value, currentCallStackFrame);

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

        public void WriteError(ErrorRecord errorRecord)
        {
            var info = LogEntryInfo.FromErrorRecord(errorRecord);

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

        public void Dispose()
        {
            if (_writeBuffer.Length != 0)
            {
                Write("\n");
            }
        }
    }
}