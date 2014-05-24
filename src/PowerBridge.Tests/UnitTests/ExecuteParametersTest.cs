using System;
using System.IO;
using System.Management.Automation.Runspaces;
using Moq;
using NUnit.Framework;
using PowerBridge.Internal;
using PowerBridge.Tests.Mocks;

namespace PowerBridge.Tests.UnitTests
{
    [TestFixture]
    public class ExecuteParametersTest
    {
        [Test]
        public void WhenExpressionIsSpecified()
        {
            var taskLog = new MockBuildTaskLog();
            Command command;
            var result = new ExecuteParameters { Expression = "test" }.TryGetCommand(taskLog, out command);

            Assert.IsTrue(result);
            taskLog.AssertLogEntriesAre(new LogEntry[0]);
            Assert.IsTrue(command.IsScript);
            Assert.AreEqual("test", command.CommandText);
        }

        [Test]
        public void WhenFileIsSpecified()
        {
            var taskLog = new MockBuildTaskLog();
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns((string path) => true);
            fileSystem.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => path);

            Command command;
            var result = new ExecuteParameters(fileSystem: fileSystem.Object) { File = @"C:\test.ps1" }.TryGetCommand(taskLog, out command);

            Assert.IsTrue(result);
            taskLog.AssertLogEntriesAre(new LogEntry[0]);
            Assert.IsTrue(command.IsScript);
            Assert.AreEqual(@"& 'C:\test.ps1'", command.CommandText);
        }

        [Test]
        public void WhenFileIsSpecifiedWithArguments()
        {
            var taskLog = new MockBuildTaskLog();
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns((string path) => true);
            fileSystem.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => path);

            Command command;
            var result = new ExecuteParameters(fileSystem: fileSystem.Object) { File = @"C:\test.ps1", Arguments = "-Arg1 foo -Arg2 bar" }.TryGetCommand(taskLog, out command);

            Assert.IsTrue(result);
            taskLog.AssertLogEntriesAre(new LogEntry[0]);
            Assert.IsTrue(command.IsScript);
            Assert.AreEqual(@"& 'C:\test.ps1' -Arg1 foo -Arg2 bar", command.CommandText);
        }

        [Test]
        public void WhenFileAndExpressionAreSpecified()
        {
            var taskLog = new MockBuildTaskLog();
            Command command;
            var result = new ExecuteParameters { File = @"C:\test.ps1", Expression = "test" }.TryGetCommand(taskLog, out command);

            Assert.IsFalse(result);
            taskLog.AssertLogEntriesAre(
                new LogError("You cannot specify both the Expression and File parameters simultaneously."));
            Assert.IsNull(command);
        }

        [Test]
        public void WhenExpressionAndArgumentsAreSpecified()
        {
            var taskLog = new MockBuildTaskLog();
            Command command;
            var result = new ExecuteParameters { Expression = "test", Arguments = "-Arg1 foo -Arg2 bar" }.TryGetCommand(taskLog, out command);

            Assert.IsFalse(result);
            taskLog.AssertLogEntriesAre(
                new LogError("The Arguments parameter can only be specified with the File parameter."));
            Assert.IsNull(command);
        }

        [Test]
        public void WhenFileOrExpressionAreNotSpecified()
        {
            var taskLog = new MockBuildTaskLog();
            Command command;
            var result = new ExecuteParameters().TryGetCommand(taskLog, out command);

            Assert.IsFalse(result);
            taskLog.AssertLogEntriesAre(
                new LogError("Either the Expression or File parameter must be specified."));
            Assert.IsNull(command);
        }

        [Test]
        public void WhenNonPs1FileIsSpecified()
        {
            var taskLog = new MockBuildTaskLog();
            Command command;
            var result = new ExecuteParameters { File = @"C:\test.txt" }.TryGetCommand(taskLog, out command);

            Assert.IsFalse(result);
            taskLog.AssertLogEntriesAre(
                new LogError(@"Processing File 'C:\test.txt' failed because the file does not have a '.ps1' " +
                             @"extension. Specify a valid Windows PowerShell script file name, and then try again."));
            Assert.IsNull(command);
        }

        [Test]
        public void WhenNonExistentFileIsSpecified()
        {
            var taskLog = new MockBuildTaskLog();
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns((string path) => false);
            fileSystem.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => path);

            Command command;
            var result = new ExecuteParameters(fileSystem: fileSystem.Object) { File = @"C:\test.ps1" }.TryGetCommand(taskLog, out command);

            Assert.IsFalse(result);
            taskLog.AssertLogEntriesAre(
                new LogError(@"The argument 'C:\test.ps1' to the File parameter does not exist. Provide the path " + 
                             @"to an existing '.ps1' file as an argument to the File parameter."));
            Assert.IsNull(command);
        }

        [Test]
        public void WhenRelativeFilePathIsSpecified()
        {
            var taskLog = new MockBuildTaskLog();
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns((string path) => true);
            fileSystem.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => Path.Combine(@"C:\", path));

            Command command;
            var result = new ExecuteParameters(fileSystem: fileSystem.Object) { File = @"test.ps1" }.TryGetCommand(taskLog, out command);

            Assert.IsTrue(result);
            taskLog.AssertLogEntriesAre(new LogEntry[0]);
            Assert.IsTrue(command.IsScript);
            Assert.AreEqual(@"& 'C:\test.ps1'", command.CommandText);
        }
    }
}
