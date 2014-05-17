using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PowerBridge.Internal
{
    internal interface IBuildTaskLog
    {
        void LogMessage(MessageImportance messageImportance, string message);

        void LogWarning(string message);

        void LogError(string message);

        void LogError(
            string subcategory,
            string errorCode,
            string helpKeyword,
            string file,
            int lineNumber,
            int columnNumber,
            int endLineNumber,
            int endColumnNumber,
            string message);
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

        public void LogWarning(string message)
        {
            _taskLoggingHelper.LogWarning(message);
        }

        public void LogError(string message)
        {
            _taskLoggingHelper.LogError(message);
        }

        public void LogError(
            string subcategory,
            string errorCode,
            string helpKeyword,
            string file,
            int lineNumber,
            int columnNumber,
            int endLineNumber,
            int endColumnNumber,
            string message)
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