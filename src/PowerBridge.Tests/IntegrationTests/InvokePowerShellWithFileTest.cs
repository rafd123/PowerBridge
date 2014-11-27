using System;
using Microsoft.Build.Framework;
using NUnit.Framework;
using PowerBridge.Internal;
using PowerBridge.Tasks;
using PowerBridge.Tests.Mocks;

namespace PowerBridge.Tests.IntegrationTests
{
    [TestFixture]
    public class InvokePowerShellWithFileTest : IntegrationTest
    {
        [Test]
        public void WhenInvokeWriteHost()
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
        public void WhenInvokingScriptFile()
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
        public void WhenInvokingScriptFileWithArguments()
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
        public void WhenInvokingNonExistentScriptFile()
        {
            var buildTaskLog = new MockBuildTaskLog();

            var commandFactory = new CommandFactory { File = "7d680d7c-3214-43c6-8eec-3b00a40ab91e.ps1" };
            var exception = Assert.Throws<ArgumentException>(() => InvokePowerShell.Execute(commandFactory, buildTaskLog));

            Assert.AreEqual(
                "The argument '7d680d7c-3214-43c6-8eec-3b00a40ab91e.ps1' to the File parameter " +
                "does not exist. Provide the path to an existing '.ps1' file as an argument to the File parameter.",
                exception.Message);
        }
    }
}
