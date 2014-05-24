using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PowerBridge.Internal
{
    internal interface IBuildTaskLog
    {
        void LogMessage(MessageImportance messageImportance, string message);

        void LogWarning(
            string message,
            string subcategory = null,
            string warningCode = null,
            string helpKeyword = null,
            string file = null,
            int lineNumber = 0,
            int columnNumber = 0,
            int endLineNumber = 0,
            int endColumnNumber = 0);

        void LogError(
            string message,
            string subcategory = null,
            string errorCode = null,
            string helpKeyword = null,
            string file = null,
            int lineNumber = 0,
            int columnNumber = 0,
            int endLineNumber = 0,
            int endColumnNumber = 0);
    }

    internal sealed class BuildTaskLog : IBuildTaskLog
    {
        private readonly TaskLoggingHelper _taskLoggingHelper;

        public BuildTaskLog(TaskLoggingHelper taskLoggingHelper)
        {
            _taskLoggingHelper = taskLoggingHelper;
        }

        public void LogMessage(MessageImportance messageImportance, string message)
        {
            _taskLoggingHelper.LogMessage(messageImportance, message);
        }

        public void LogWarning(
            string message,
            string subcategory = null,
            string warningCode = null,
            string helpKeyword = null,
            string file = null,
            int lineNumber = 0,
            int columnNumber = 0,
            int endLineNumber = 0,
            int endColumnNumber = 0)
        {
            _taskLoggingHelper.LogWarning(
                subcategory,
                warningCode,
                helpKeyword,
                file,
                lineNumber,
                columnNumber,
                endLineNumber,
                endColumnNumber,
                message);
        }

        public void LogError(
            string message,
            string subcategory = null,
            string errorCode = null,
            string helpKeyword = null,
            string file = null,
            int lineNumber = 0,
            int columnNumber = 0,
            int endLineNumber = 0,
            int endColumnNumber = 0)
        {
            _taskLoggingHelper.LogError(
                subcategory,
                errorCode,
                helpKeyword,
                file,
                lineNumber,
                columnNumber,
                endLineNumber,
                endColumnNumber,
                message);
        }
    }
}