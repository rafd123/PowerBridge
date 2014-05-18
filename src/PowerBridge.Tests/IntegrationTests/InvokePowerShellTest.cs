using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Build.Framework;
using NUnit.Framework;
using PowerBridge.Tasks;
using PowerBridge.Tests.Mocks;

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
                new LogMessage("hello", MessageImportance.High));
        }

        [Test]
        public void WhenInvokingInlineWriteHostNoNewLine()
        {
            var buildTaskLog = new MockBuildTaskLog();

            InvokePowerShell.Execute("Write-Host 'hello' -NoNewLine", buildTaskLog);

            buildTaskLog.AssertLogEntriesAre(
                new LogMessage("hello", MessageImportance.High));
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
                    message: "Windows PowerShell is in non-interactive mode. Read and Prompt functionality is not available." + Environment.NewLine +
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
                    message: "Windows PowerShell is in non-interactive mode. Read and Prompt functionality is not available." + Environment.NewLine +
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
                    message: "Windows PowerShell is in non-interactive mode. Read and Prompt functionality is not available." + Environment.NewLine +
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
                    message: "Windows PowerShell is in non-interactive mode. Read and Prompt functionality is not available." + Environment.NewLine +
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
