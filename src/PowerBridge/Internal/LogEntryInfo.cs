using System;
using System.Globalization;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PowerBridge.Internal
{
    internal class LogEntryInfo
    {
        private static readonly Func<ErrorRecord, LogEntryInfo>[] ErrorRecordParsers =
        {
            GetLogEntryInfoFromMessage,
            GetLogEntryInfoFromScriptStackTrace
        };

        private LogEntryInfo(string message, string file, int lineNumber, int columnNumber)
        {
            Message = message;
            File = file;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public static LogEntryInfo FromErrorRecord(ErrorRecord errorRecord)
        {
            if (errorRecord == null)
            {
                throw new ArgumentNullException("errorRecord");
            }

            foreach (var parser in ErrorRecordParsers)
            {
                var errorRecordInfo = parser(errorRecord);
                if (errorRecordInfo != null)
                {
                    return errorRecordInfo;
                }
            }

            // The ErrorRecord's invocation info doesn't necessarily
            // contain the information where the error was thrown but
            // rather what call was made that eventually let to the error.
            // Nonetheless, let's use it as a fallback.
            var file = errorRecord.InvocationInfo.ScriptName;
            var lineNumber = errorRecord.InvocationInfo.ScriptLineNumber;
            var message = errorRecord + Environment.NewLine + errorRecord.ScriptStackTrace;

            return new LogEntryInfo(
                message: message,
                file: file,
                lineNumber: lineNumber,
                columnNumber: 0);
        }

        public static LogEntryInfo FromMessage(string message, CallStackFrame callStackFrame = null)
        {
            string messageWithoutLineInfo;
            string file;
            int lineNumber;
            int columnNumber;

            if (TryGetLineInfoFromMessages(message, out messageWithoutLineInfo, out file, out lineNumber, out columnNumber))
            {
                return new LogEntryInfo(
                    message: messageWithoutLineInfo,
                    file: file,
                    lineNumber: lineNumber,
                    columnNumber: columnNumber);
            }

            if (callStackFrame != null)
            {
                return new LogEntryInfo(
                    message: message,
                    file: callStackFrame.ScriptName,
                    lineNumber: string.IsNullOrEmpty(callStackFrame.ScriptName) ? 0 : callStackFrame.ScriptLineNumber,
                    columnNumber: 0);
            }

            return new LogEntryInfo(
                message: message,
                file: null,
                lineNumber: 0,
                columnNumber: 0);


        }

        private static LogEntryInfo GetLogEntryInfoFromScriptStackTrace(ErrorRecord errorRecord)
        {
            // Ideally, we'd be able to use a System.Management.Automation.CallStackFrame
            // objects in order to determine where the error was thrown, however the
            // ErrorRecord object doesn't expose them. As a result, we have to resort
            // to parsing the ErrorRecord.ScriptStackTrace.
            var match = Regex.Match(errorRecord.ScriptStackTrace, @".*, (?<file>.+): line (?<line>[\d]+)");
            if (!match.Success)
            {
                return null;
            }

            var file = match.Groups["file"].Value;
            var lineNumber = int.Parse(match.Groups["line"].Value, CultureInfo.CurrentCulture);
            var message = errorRecord + Environment.NewLine +
                          errorRecord.ScriptStackTrace;

            return new LogEntryInfo(
                message: message,
                file: file,
                lineNumber: lineNumber,
                columnNumber: 0);
        }

        private static LogEntryInfo GetLogEntryInfoFromMessage(ErrorRecord errorRecord)
        {
            string message;
            string file;
            int lineNumber;
            int columnNumber;

            if (!TryGetLineInfoFromMessages(errorRecord.ToString(), out message, out file, out lineNumber, out columnNumber))
            {
                return null;
            }

            message = message + Environment.NewLine +
                      string.Format(CultureInfo.CurrentCulture, Resources.CustomFileLineNumberFormat, file, lineNumber) + Environment.NewLine +
                      errorRecord.ScriptStackTrace;

            return new LogEntryInfo(
                message: message,
                file: file,
                lineNumber: lineNumber,
                columnNumber: columnNumber);
        }

        private static bool TryGetLineInfoFromMessages(
            string originalMessage,
            out string messageWithoutLineInfo,
            out string file,
            out int lineNumber,
            out int columnNumber)
        {
            messageWithoutLineInfo = null;
            file = null;
            lineNumber = 0;
            columnNumber = 0;

            // If the error message conforms to the perscribed custom build step error formatting
            // (see http://msdn.microsoft.com/en-us/library/yxkt8b26.aspx) let's use this information
            // for the file and line info since its more contextual
            var match = Regex.Match(originalMessage, @"^(?<file>.+?)\((?<line>\d+)(,(?<column>\d+))?\) : (?<message>.*)");
            if (!match.Success)
            {
                return false;
            }

            file = match.Groups["file"].Value;
            lineNumber = int.Parse(match.Groups["line"].Value, CultureInfo.CurrentCulture);

            var columnValue = match.Groups["column"].Value;
            if (!string.IsNullOrEmpty(columnValue))
            {
                columnNumber = int.Parse(columnValue, CultureInfo.CurrentCulture);
            }

            messageWithoutLineInfo = match.Groups["message"].Value;

            return true;
        }

        public string Message { get; private set; }

        public string File { get; private set; }

        public int LineNumber { get; private set; }

        public int ColumnNumber { get; private set; }
    }
}