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

            var parameters = new ExecuteParameters { Expression = "Write-Host 'hello'" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High));
        }

        [Test]
        public void WhenInvokeWriteHostAsFile()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var parameters = new ExecuteParameters { File = "Write-Host 'hello'" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(message: "Processing File 'Write-Host 'hello'' failed because the file does not have a '.ps1' " +
                                      "extension. Specify a valid Windows PowerShell script file name, and then try again."));
        }

        [Test]
        public void WhenInvokeWriteHostNoNewLineAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var parameters = new ExecuteParameters { Expression = "Write-Host 'hello' -NoNewLine" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High));
        }

        [Test]
        public void WhenInvokeWriteWarningAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var parameters = new ExecuteParameters { Expression = "Write-Warning 'hello'" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogWarning("hello"));
        }

        [Test]
        public void WhenInvokeWriteVerboseAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var parameters = new ExecuteParameters { Expression = "Write-Verbose 'hello' -Verbose" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.Low));
        }

        [Test]
        public void WhenInvokeWriteDebugAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var parameters = new ExecuteParameters { Expression = "Write-Debug 'hello' -Debug" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

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

            var parameters = new ExecuteParameters { Expression = "Read-Host" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

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

            var parameters = new ExecuteParameters { Expression = "Read-Host -AsSecureString" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

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

            var parameters = new ExecuteParameters { Expression = "Get-Credential" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

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

            var parameters = new ExecuteParameters { Expression = "Write-Error 'hello'" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

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

            var parameters = new ExecuteParameters { Expression = @"Write-Warning 'c:\foo\bar.txt(123) : This is a test'" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

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

            var parameters = new ExecuteParameters { Expression = @"Write-Warning 'c:\foo\bar.txt(123,456) : This is a test'" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogWarning(
                    file: @"c:\foo\bar.txt",
                    lineNumber: 123,
                    columnNumber: 456,
                    message: @"This is a test"));
        }

        public void WhenInvokeWriteErrorWithExplicitFilenameAndLineAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var parameters = new ExecuteParameters { Expression = @"Write-Error 'c:\foo\bar.txt(123) : This is a test'" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

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

            var parameters = new ExecuteParameters { Expression = @"Write-Error 'c:\foo\bar.txt(123,456) : This is a test'" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

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
        public void WhenInvokingScriptFileAsExpression()
        {
            var buildTaskLog = new MockBuildTaskLog();
            var scriptFilePath = GetTestResourceFilePath("WhenInvokingScriptFile.ps1");
            var script = string.Format(
                CultureInfo.InvariantCulture,
                "& \"{0}\"",
                scriptFilePath);

            var parameters = new ExecuteParameters { Expression = script };
            InvokePowerShell.Execute(parameters, buildTaskLog);

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

            var parameters = new ExecuteParameters { File = scriptFilePath };
            InvokePowerShell.Execute(parameters, buildTaskLog);

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
                             "at <ScriptBlock>, " + scriptFilePath + ": line 3"));
        }

        [Test]
        public void WhenInvokingNonExistentScriptFileAsFile()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var parameters = new ExecuteParameters { File = "7d680d7c-3214-43c6-8eec-3b00a40ab91e.ps1" };
            InvokePowerShell.Execute(parameters, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogError(message: "The argument '7d680d7c-3214-43c6-8eec-3b00a40ab91e.ps1' to the File parameter " +
                                      "does not exist. Provide the path to an existing '.ps1' file as an argument to the File parameter."));
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
            var parameters = new ExecuteParameters { Expression = script };
            InvokePowerShell.Execute(parameters, buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High),
                new LogMessage("in foo", MessageImportance.High),
                new LogMessage("goodbye", MessageImportance.High));
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
