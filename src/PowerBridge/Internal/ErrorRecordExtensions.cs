using System;
using System.Globalization;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PowerBridge.Internal
{
    internal static class ErrorRecordExtensions
    {
        private static readonly Func<ErrorRecord, ErrorRecordInfo>[] ErrorRecordParsers =
        {
            GetErrorInfoFromMessage,
            GetErrorInfoFromScriptStackTrace
        };

        public static ErrorRecordInfo GetErrorInfo(this ErrorRecord errorRecord)
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

            return new ErrorRecordInfo(
                message: message,
                file: file,
                lineNumber: lineNumber,
                columnNumber: 0);
        }

        private static ErrorRecordInfo GetErrorInfoFromMessage(ErrorRecord errorRecord)
        {
            // If the error message conforms to the perscribed custom build step error formatting
            // (see http://msdn.microsoft.com/en-us/library/yxkt8b26.aspx) let's use this information
            // for the file and line info since its more contextual
            var match = Regex.Match(errorRecord.ToString(), @"^(?<file>.+?)\((?<line>\d+)(,(?<column>\d+))?\) : (?<message>.*)");
            if (!match.Success)
            {
                return null;
            }

            var file = match.Groups["file"].Value;
            var lineNumber = int.Parse(match.Groups["line"].Value);

            var columnNumber = 0;
            var columnValue = match.Groups["column"].Value;
            if (!string.IsNullOrEmpty(columnValue))
            {
                columnNumber = int.Parse(columnValue);
            }

            var message = match.Groups["message"].Value + Environment.NewLine +
                          string.Format(CultureInfo.CurrentCulture, "at {0}: line {1}", file, lineNumber) + Environment.NewLine +
                          errorRecord.ScriptStackTrace;

            return new ErrorRecordInfo(
                message: message,
                file: file,
                lineNumber: lineNumber,
                columnNumber: columnNumber);
        }

        private static ErrorRecordInfo GetErrorInfoFromScriptStackTrace(ErrorRecord errorRecord)
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
            var lineNumber = int.Parse(match.Groups["line"].Value);
            var message = errorRecord + Environment.NewLine +
                          errorRecord.ScriptStackTrace;

            return new ErrorRecordInfo(
                message: message,
                file: file,
                lineNumber: lineNumber,
                columnNumber: 0);  
        }
    }

    internal class ErrorRecordInfo
    {
        public ErrorRecordInfo(string message, string file, int lineNumber, int columnNumber)
        {
            Message = message;
            File = file;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public string Message { get; private set; }

        public string File { get; private set; }

        public int LineNumber { get; private set; }

        public int ColumnNumber { get; private set; }
    }
}