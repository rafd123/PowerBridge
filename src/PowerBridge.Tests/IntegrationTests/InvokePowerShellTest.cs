using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Build.Framework;
using NUnit.Framework;
using PowerBridge.Internal;
using PowerBridge.Tasks;

namespace PowerBridge.Tests.IntegrationTests
{
    [TestFixture]
    public class InvokePowerShellTest
    {
        [Test]
        public void WhenInvokingInlineWriteHost()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute("Write-Host 'hello'", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High),
                new LogMessage("\n", MessageImportance.High));
        }

        [Test]
        public void WhenInvokingInlineWriteWarning()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute("Write-Warning 'hello'", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogWarning("hello"));
        }

        [Test]
        public void WhenInvokingInlineWriteVerbose()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute("Write-Verbose 'hello' -Verbose", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.Low));
        }

        [Test]
        public void WhenInvokingInlineWriteDebug()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute("Write-Debug 'hello' -Debug", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.Low),
                new LogError(
                    file: "<No file>",
                    lineNumber: 1,
                    message: "Windows PowerShell is in NonInteractive mode. Read and Prompt functionality is not available." + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokingInlineReadHost()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute("Read-Host", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: "<No file>",
                    lineNumber: 1,
                    message: "Windows PowerShell is in NonInteractive mode. Read and Prompt functionality is not available." + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokingInlineReadHostAsSecureString()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute("Read-Host -AsSecureString", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: "<No file>",
                    lineNumber: 1,
                    message: "Windows PowerShell is in NonInteractive mode. Read and Prompt functionality is not available." + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokingInlineGetCredential()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute("Get-Credential", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: "<No file>",
                    lineNumber: 1,
                    message: "Windows PowerShell is in NonInteractive mode. Read and Prompt functionality is not available." + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokingInlineWriteError()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute("Write-Error 'hello'", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: "<No file>",
                    lineNumber: 1,
                    message: "hello" + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }
        
        [Test]
        public void WhenInvokingInlineWriteErrorWithExplicitFilenameAndLine()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute(@"Write-Error 'c:\foo\bar.txt(123) : This is a test'", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: @"c:\foo\bar.txt",
                    lineNumber: 123,
                    message: @"This is a test" + Environment.NewLine +
                             @"at c:\foo\bar.txt: line 123" + Environment.NewLine +
                             @"at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokingInlineWriteErrorWithExplicitFilenameLineAndColumn()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute(@"Write-Error 'c:\foo\bar.txt(123,456) : This is a test'", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: @"c:\foo\bar.txt",
                    lineNumber: 123,
                    columnNumber: 456,
                    message: @"This is a test" + Environment.NewLine +
                             @"at c:\foo\bar.txt: line 123" + Environment.NewLine +
                             @"at <ScriptBlock>, <No file>: line 1"));
        }   

        [Test]
        public void WhenInvokingScriptFile()
        {
            var buildTaskLog = new MockBuildTaskLog();
            var scriptFilePath = GetTestResourceFilePath("WhenInvokingScriptFile.ps1");
            var script = string.Format(
                CultureInfo.InvariantCulture,
                "& \"{0}\"",
                scriptFilePath);

            InvokePowerShell.Execute(script, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("Alive", MessageImportance.High),
                new LogMessage("\n", MessageImportance.High),
                new LogWarning("Danger"),
                new LogError(
                    file: scriptFilePath,
                    lineNumber: 3,
                    message: "Dead" + Environment.NewLine +
                             "at <ScriptBlock>, " + scriptFilePath + ": line 3" + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokingComplexInlineScript()
        {
            var buildTaskLog = new MockBuildTaskLog();

            const string script = @"
function foo
{
    Write-Host 'in foo'
}

Write-Host 'hello'; foo

Write-Host 'goodbye'
";
            InvokePowerShell.Execute(script, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High),
                new LogMessage("\n", MessageImportance.High),
                new LogMessage("in foo", MessageImportance.High),
                new LogMessage("\n", MessageImportance.High),
                new LogMessage("goodbye", MessageImportance.High),
                new LogMessage("\n", MessageImportance.High));
        }

        private static string GetTestResourceFilePath(string testResource, [CallerFilePath] string sourceFilePath = "")
        {
            sourceFilePath = NormalizePath(sourceFilePath);

            return Path.Combine(
                Path.GetDirectoryName(sourceFilePath),
                Path.GetFileNameWithoutExtension(sourceFilePath) + "Resources",
                testResource);
        }

        private static string NormalizePath(string path)
        {
            // Capitalize the drive is necessary
            if (Path.IsPathRooted(path))
            {
                if (path[0] != char.ToUpperInvariant(path[0]))
                {
                    path = char.ToUpperInvariant(path[0]) + path.Substring(1);
                }
            }

            return path;
        }

        private sealed class MockBuildTaskLog : IBuildTaskLog
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

            public void LogWarning(string message)
            {
                _actualEntries.Add(new LogWarning(message));
            }

            public void LogError(string message)
            {
                _actualEntries.Add(new LogErrorMessageOnly(message));
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
                _actualEntries.Add(new LogError(
                    subcategory,
                    errorCode,
                    helpKeyword,
                    file,
                    lineNumber,
                    columnNumber,
                    endLineNumber,
                    endColumnNumber,
                    message));
            }
        }

        private abstract class LogEntry
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

        private abstract class LogEntry<T> : LogEntry where T : LogEntry
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

        private sealed class LogMessage : LogEntry<LogMessage>
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

        private sealed class LogWarning : LogEntry<LogWarning>
        {
            public LogWarning(string message)
                : base(message)
            {
            }

            protected override void AssertIsEqual(LogWarning actualEntry)
            {
                // no-op
            }
        }

        private sealed class LogErrorMessageOnly : LogEntry<LogErrorMessageOnly>
        {
            public LogErrorMessageOnly(string message)
                : base(message)
            {
            }

            protected override void AssertIsEqual(LogErrorMessageOnly actualEntry)
            {
                // no-op
            }
        }

        private sealed class LogError : LogEntry<LogError>
        {
            public LogError(
                string subcategory = null,
                string errorCode = null,
                string helpKeyword = null,
                string file = null,
                int lineNumber = 0,
                int columnNumber = 0,
                int endLineNumber = 0,
                int endColumnNumber = 0,
                string message = null)
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
}
