using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using NUnit.Framework;
using PowerBridge.Internal;

namespace PowerBridge.Tests.Mocks
{
    public sealed class MockBuildTaskLog : IBuildTaskLog
    {
        private readonly List<LogEntry> _actualEntries = new List<LogEntry>();

        public void AssertLogEntriesAre(params LogEntry[] expectedEntries)
        {
            if (expectedEntries == null)
            {
                throw new ArgumentNullException("expectedEntries");
            }

            Assert.AreEqual(expectedEntries.Length, _actualEntries.Count);
            for (var i = 0; i < expectedEntries.Length; i++)
            {
                var expectedEntry = expectedEntries[i];
                var actualEntry = _actualEntries[i];
                expectedEntry.AssertIsEqual(actualEntry);
            }
        }

        public void LogMessage(MessageImportance messageImportance, string message)
        {
            _actualEntries.Add(new LogMessage(message, messageImportance));
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
            _actualEntries.Add(new LogWarning(
                message,
                subcategory,
                warningCode,
                helpKeyword,
                file,
                lineNumber,
                columnNumber,
                endLineNumber,
                endColumnNumber));
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
            _actualEntries.Add(new LogError(
                message,
                subcategory,
                errorCode,
                helpKeyword,
                file,
                lineNumber,
                columnNumber,
                endLineNumber,
                endColumnNumber));
        }
    }

    public abstract class LogEntry
    {
        protected LogEntry(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }

        public abstract void AssertIsEqual(LogEntry actualEntry);

        protected T CastWithAssert<T>(LogEntry actualEntry) where T : LogEntry
        {
            if (actualEntry == null)
            {
                throw new ArgumentNullException("actualEntry");
            }

            var actualCastedEntry = actualEntry as T;
            if (actualCastedEntry == null)
            {
                Assert.Fail(
                    "Expected a {0} with message \"{1}\", but got a {2} with message \"{3}\" instead.",
                    GetType().Name,
                    Message,
                    actualEntry.GetType().Name,
                    actualEntry.Message);
            }

            return actualCastedEntry;
        }
    }

    public abstract class LogEntry<T> : LogEntry where T : LogEntry
    {
        protected LogEntry(string message)
            : base(message)
        {
        }

        public sealed override void AssertIsEqual(LogEntry actualEntry)
        {
            var actualCastedEntry = CastWithAssert<T>(actualEntry);

            Assert.AreEqual(Message, actualCastedEntry.Message);

            AssertIsEqual(actualCastedEntry);
        }

        protected abstract void AssertIsEqual(T actualEntry);
    }

    public sealed class LogMessage : LogEntry<LogMessage>
    {
        public LogMessage(string message, MessageImportance messageImportance)
            : base(message)
        {
            MessageImportance = messageImportance;
        }

        public MessageImportance MessageImportance { get; private set; }


        protected override void AssertIsEqual(LogMessage actualEntry)
        {
            Assert.AreEqual(MessageImportance, actualEntry.MessageImportance);
        }
    }

    public sealed class LogWarning : LogEntry<LogWarning>
    {
        public LogWarning(
            string message,
            string subcategory = null,
            string errorCode = null,
            string helpKeyword = null,
            string file = null,
            int lineNumber = 0,
            int columnNumber = 0,
            int endLineNumber = 0,
            int endColumnNumber = 0)
            : base(message)
        {
            Subcategory = subcategory;
            ErrorCode = errorCode;
            HelpKeyword = helpKeyword;
            File = file;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
            EndLineNumber = endLineNumber;
            EndColumnNumber = endColumnNumber;
        }

        public string Subcategory { get; private set; }

        public string ErrorCode { get; private set; }

        public string HelpKeyword { get; private set; }

        public string File { get; private set; }

        public int LineNumber { get; private set; }

        public int ColumnNumber { get; private set; }

        public int EndLineNumber { get; private set; }

        public int EndColumnNumber { get; private set; }

        protected override void AssertIsEqual(LogWarning actualEntry)
        {
            Assert.AreEqual(Subcategory, actualEntry.Subcategory);
            Assert.AreEqual(ErrorCode, actualEntry.ErrorCode);
            Assert.AreEqual(HelpKeyword, actualEntry.HelpKeyword);
            Assert.AreEqual(File, actualEntry.File);
            Assert.AreEqual(LineNumber, actualEntry.LineNumber);
            Assert.AreEqual(ColumnNumber, actualEntry.ColumnNumber);
            Assert.AreEqual(EndLineNumber, actualEntry.EndLineNumber);
            Assert.AreEqual(EndColumnNumber, actualEntry.EndColumnNumber);
        }
    }

    public sealed class LogError : LogEntry<LogError>
    {
        public LogError(
            string message,
            string subcategory = null,
            string errorCode = null,
            string helpKeyword = null,
            string file = null,
            int lineNumber = 0,
            int columnNumber = 0,
            int endLineNumber = 0,
            int endColumnNumber = 0)
            : base(message)
        {
            Subcategory = subcategory;
            ErrorCode = errorCode;
            HelpKeyword = helpKeyword;
            File = file;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
            EndLineNumber = endLineNumber;
            EndColumnNumber = endColumnNumber;
        }

        public string Subcategory { get; private set; }

        public string ErrorCode { get; private set; }

        public string HelpKeyword { get; private set; }

        public string File { get; private set; }

        public int LineNumber { get; private set; }

        public int ColumnNumber { get; private set; }

        public int EndLineNumber { get; private set; }

        public int EndColumnNumber { get; private set; }

        protected override void AssertIsEqual(LogError actualEntry)
        {
            Assert.AreEqual(Subcategory, actualEntry.Subcategory);
            Assert.AreEqual(ErrorCode, actualEntry.ErrorCode);
            Assert.AreEqual(HelpKeyword, actualEntry.HelpKeyword);
            Assert.AreEqual(File, actualEntry.File);
            Assert.AreEqual(LineNumber, actualEntry.LineNumber);
            Assert.AreEqual(ColumnNumber, actualEntry.ColumnNumber);
            Assert.AreEqual(EndLineNumber, actualEntry.EndLineNumber);
            Assert.AreEqual(EndColumnNumber, actualEntry.EndColumnNumber);
        }
    }
}