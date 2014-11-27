using System;
using System.Globalization;
using Microsoft.Build.Framework;
using NUnit.Framework;
using PowerBridge.Internal;
using PowerBridge.Tasks;
using PowerBridge.Tests.Mocks;

namespace PowerBridge.Tests.IntegrationTests
{
    [TestFixture]
    public class InvokePowerShellWithExpressionTest : IntegrationTest
    {
        [Test]
        public void WhenInvokeWriteHost()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Write-Host 'hello'" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High));
        }

        [Test]
        public void WhenInvokeWriteHostNoNewLine()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Write-Host 'hello' -NoNewLine" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High));
        }

        [Test]
        public void WhenInvokeWriteWarning()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Write-Warning 'hello'" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogWarning("hello"));
        }

        [Test]
        public void WhenInvokeWriteVerbose()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { Expression = "Write-Verbose 'hello' -Verbose" };
            InvokePowerShell.Execute(commandFactory, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.Low));
        }

        [Test]
        public void WhenInvokeWriteDebug()
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
        public void WhenInvokeReadHost()
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
        public void WhenInvokeReadHostAsSecureString()
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
        public void WhenInvokeGetCredential()
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
        public void WhenInvokeWriteError()
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
        public void WhenInvokeWriteWarningWithExplicitFilenameAndLine()
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
        public void WhenInvokeWriteWarningWithExplicitFilenameLineAndColumn()
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
        public void WhenInvokeWriteErrorWithExplicitFilenameAndLine()
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
        public void WhenInvokeWriteErrorWithExplicitFilenameLineAndColumn()
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
        public void WhenInvokeWriteErrorMultilineWithExplicitFilenameAndLine()
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
        public void WhenInvokingScriptFile()
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
    }
}
