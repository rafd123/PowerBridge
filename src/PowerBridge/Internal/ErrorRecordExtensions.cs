using System;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PowerBridge.Internal
{
    internal static class ErrorRecordExtensions
    {
        public static ErrorRecordInfo GetErrorInfo(this ErrorRecord errorRecord)
        {
            if (errorRecord == null)
            {
                throw new ArgumentNullException("errorRecord");
            }

            var message = errorRecord + Environment.NewLine + errorRecord.ScriptStackTrace;

            string file;
            int lineNumber;
            int columnNumber;
            if (!TryGetFileAndLineFromErrorRecordMessage(errorRecord.ToString(), out file, out lineNumber, out columnNumber))
            {
                if (!TryGetFileAndLineFromScriptStackTrace(errorRecord.ScriptStackTrace, out file, out lineNumber))
                {
                    // The ErrorRecord's invocation info doesn't necessarily
                    // contain the information where the error was thrown but
                    // rather what call was made that eventually let to the error.
                    // Nonetheless, let's use it as a fallback.
                    file = errorRecord.InvocationInfo.ScriptName;
                    lineNumber = errorRecord.InvocationInfo.ScriptLineNumber;
                }                
            }

            return new ErrorRecordInfo(
                message: message,
                file: file,
                lineNumber: lineNumber,
                columnNumber: columnNumber);
        }

        private static bool TryGetFileAndLineFromErrorRecordMessage(string errorRecordMessage, out string file, out int lineNumber, out int columnNumber)
        {
            file = null;
            lineNumber = 0;
            columnNumber = 0;

            // If the error message conforms to the perscribed custom build step error formatting
            // (see http://msdn.microsoft.com/en-us/library/yxkt8b26.aspx) let's use this information
            // for the file and line info since its more contextual
            var match = Regex.Match(errorRecordMessage, @"^(?<file>.+?)\((?<line>\d+)(,(?<column>\d+))?\) : ");
            if (!match.Success)
            {
                return false;
            }

            file = match.Groups["file"].Value;
            lineNumber = int.Parse(match.Groups["line"].Value);

            var columnValue = match.Groups["column"].Value;
            if (!string.IsNullOrEmpty(columnValue))
            {
                columnNumber = int.Parse(columnValue);    
            }

            return true;
        }

        private static bool TryGetFileAndLineFromScriptStackTrace(string scriptStackTrace, out string file, out int lineNumber)
        {
            file = null;
            lineNumber = 0;

            // Ideally, we'd be able to use a System.Management.Automation.CallStackFrame
            // objects in order to determine where the error was thrown, however the
            // ErrorRecord object doesn't expose them. As a result, we have to resort
            // to parsing the ErrorRecord.ScriptStackTrace.
            var match = Regex.Match(scriptStackTrace, @".*, (?<file>.+): line (?<line>[\d]+)");
            if (!match.Success)
            {
                return false;
            }

            file = match.Groups["file"].Value;
            lineNumber = int.Parse(match.Groups["line"].Value);
            return true;
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