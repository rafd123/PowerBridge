using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Build.Framework;
using NUnit.Framework;
using PowerBridge.Internal;
using PowerBridge.Tasks;
using PowerBridge.Tests.Mocks;

namespace PowerBridge.Tests.IntegrationTests
{
    [TestFixture]
    public class InvokePowerShellTest
    {
        [Test]
        public void WhenInvokeWriteHostAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Write-Host 'hello'" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High));
        }

        [Test]
        public void WhenInvokeWriteHostAsFile()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { File = "Write-Host 'hello'" };
            var exception = Assert.Throws<ArgumentException>(() => InvokePowerShell.Execute(commandFactory, buildTaskLog));

            Assert.AreEqual(
                "Processing File 'Write-Host 'hello'' failed because the file does not have a '.ps1' " +
                "extension. Specify a valid Windows PowerShell script file name, and then try again.",
                exception.Message);
        }

        [Test]
        public void WhenInvokeWriteHostNoNewLineAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Write-Host 'hello' -NoNewLine" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High));
        }

        [Test]
        public void WhenInvokeWriteWarningAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Write-Warning 'hello'" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogWarning("hello"));
        }

        [Test]
        public void WhenInvokeWriteVerboseAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Write-Verbose 'hello' -Verbose" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.Low));
        }

        [Test]
        public void WhenInvokeWriteDebugAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Write-Debug 'hello' -Debug" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.Low),
                new LogError(
                    file: "<No file>",
                    lineNumber: 1,
                    message: "Windows PowerShell is in non-interactive mode. Read and Prompt functionality is not available." + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokeReadHostAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Read-Host" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: "<No file>",
                    lineNumber: 1,
                    message: "Windows PowerShell is in non-interactive mode. Read and Prompt functionality is not available." + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokeReadHostAsSecureStringAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Read-Host -AsSecureString" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: "<No file>",
                    lineNumber: 1,
                    message: "Windows PowerShell is in non-interactive mode. Read and Prompt functionality is not available." + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokeGetCredentialAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Get-Credential" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: "<No file>",
                    lineNumber: 1,
                    message: "Windows PowerShell is in non-interactive mode. Read and Prompt functionality is not available." + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokeWriteErrorAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Write-Error 'hello'" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: "<No file>",
                    lineNumber: 1,
                    message: "hello" + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }
        
        [Test]
        public void WhenInvokeWriteWarningWithExplicitFilenameAndLineAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = @"Write-Warning 'c:\foo\bar.txt(123) : This is a test'" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogWarning(
                    file: @"c:\foo\bar.txt",
                    lineNumber: 123,
                    message: @"This is a test"));
        }

        [Test]
        public void WhenInvokeWriteWarningWithExplicitFilenameLineAndColumnAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = @"Write-Warning 'c:\foo\bar.txt(123,456) : This is a test'" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogWarning(
                    file: @"c:\foo\bar.txt",
                    lineNumber: 123,
                    columnNumber: 456,
                    message: @"This is a test"));
        }

        [Test]
        public void WhenInvokeWriteErrorWithExplicitFilenameAndLineAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = @"Write-Error 'c:\foo\bar.txt(123) : This is a test'" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: @"c:\foo\bar.txt",
                    lineNumber: 123,
                    message: @"This is a test" + Environment.NewLine +
                             @"at c:\foo\bar.txt: line 123" + Environment.NewLine +
                             @"at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokeWriteErrorWithExplicitFilenameLineAndColumnAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = @"Write-Error 'c:\foo\bar.txt(123,456) : This is a test'" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

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
        public void WhenInvokeWriteErrorMultilineWithExplicitFilenameAndLineAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = @"Write-Error ""c:\foo\bar.txt(123) : This is a test`nThis is a test""" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(
                    file: @"c:\foo\bar.txt",
                    lineNumber: 123,
                    message: "This is a test\nThis is a test" + Environment.NewLine +
                             @"at c:\foo\bar.txt: line 123" + Environment.NewLine +
                             @"at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokingScriptFileAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();
            var scriptFilePath = GetTestResourceFilePath("WhenInvokingScriptFile.ps1");
            var script = string.Format(
                CultureInfo.InvariantCulture,
                "& \"{0}\"",
                scriptFilePath);

            var commandFactory = new CommandFactory { Expression = script };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("Alive", MessageImportance.High),
                new LogWarning(
                    file: scriptFilePath,
                    lineNumber: 2,
                    message: "Danger"),
                new LogError(
                    file: scriptFilePath,
                    lineNumber: 3,
                    message: "Dead" + Environment.NewLine +
                             "at <ScriptBlock>, " + scriptFilePath + ": line 3" + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokingScriptFileAsFile()
        {
            var buildTaskLog = new MockBuildTaskLog();
            var scriptFilePath = GetTestResourceFilePath("WhenInvokingScriptFile.ps1");

            var commandFactory = new CommandFactory { File = scriptFilePath };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("Alive", MessageImportance.High),
                new LogWarning(
                    file: scriptFilePath,
                    lineNumber: 2,
                    message: "Danger"),
                new LogError(
                    file: scriptFilePath,
                    lineNumber: 3,
                    message: "Dead" + Environment.NewLine +
                             "at <ScriptBlock>, " + scriptFilePath + ": line 3" + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokingScriptFileWithArgumentsAsFile()
        {
            var buildTaskLog = new MockBuildTaskLog();
            var scriptFilePath = GetTestResourceFilePath("WhenInvokingScriptFileWithArguments.ps1");

            var commandFactory = new CommandFactory { File = scriptFilePath, Arguments = "-Arg1 foo -Arg2 bar"};
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("Arg1 = foo", MessageImportance.High),
                new LogMessage("Arg2 = bar", MessageImportance.High),
                new LogMessage("Alive", MessageImportance.High),
                new LogWarning(
                    file: scriptFilePath,
                    lineNumber: 10,
                    message: "Danger"),
                new LogError(
                    file: scriptFilePath,
                    lineNumber: 11,
                    message: "Dead" + Environment.NewLine +
                             "at <ScriptBlock>, " + scriptFilePath + ": line 11" + Environment.NewLine +
                             "at <ScriptBlock>, <No file>: line 1"));
        }

        [Test]
        public void WhenInvokingNonExistentScriptFileAsFile()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { File = "7d680d7c-3214-43c6-8eec-3b00a40ab91e.ps1" };
            var exception = Assert.Throws<ArgumentException>(() => InvokePowerShell.Execute(commandFactory, buildTaskLog));

            Assert.AreEqual(
                "The argument '7d680d7c-3214-43c6-8eec-3b00a40ab91e.ps1' to the File parameter " +
                "does not exist. Provide the path to an existing '.ps1' file as an argument to the File parameter.",
                exception.Message);
        }

        [Test]
        public void WhenInvokingComplexInlineScriptAsExpression()
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
            var commandFactory = new CommandFactory { Expression = script };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High),
                new LogMessage("in foo", MessageImportance.High),
                new LogMessage("goodbye", MessageImportance.High));
        }

        [Test]
        public void WhenObjectsFallOffPipeline()
        {
            var buildTaskLog = new MockBuildTaskLog();

            const string script = @"
'hi'
'there'
1
Write-Error boom
@()
@{ foo = 'bar' } | Format-Table -AutoSize
";

            var commandFactory = new CommandFactory { Expression = script };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hi", MessageImportance.High),
                new LogMessage("there", MessageImportance.High),
                new LogMessage("1", MessageImportance.High),
                new LogError(
                    file: "<No file>",
                    lineNumber: 5,
                    message: "boom" + Environment.NewLine + 
                             "at <ScriptBlock>, <No file>: line 5"),
                new LogMessage(messageImportance: MessageImportance.High, message: @"
Name Value
---- -----
foo  bar  

"));
        }

        [Test]
        public void WhenInvokingCommandLineApp()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "& cmd /c echo hi" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hi", MessageImportance.High));
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
    }
}
