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
            var command = new ExecuteParameters
            {
                Expression = "test"
            }
            .GetCommand();

            Assert.IsTrue(command.IsScript);
            Assert.AreEqual("test", command.CommandText);
        }

        [Test]
        public void WhenFileIsSpecified()
        {
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns((string path) => true);
            fileSystem.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => path);

            var command = new ExecuteParameters(fileSystem: fileSystem.Object)
            {
                File = @"C:\test.ps1"
            }
            .GetCommand();

            Assert.IsTrue(command.IsScript);
            Assert.AreEqual(@"& 'C:\test.ps1'", command.CommandText);
        }

        [Test]
        public void WhenFileIsSpecifiedWithArguments()
        {
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns((string path) => true);
            fileSystem.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => path);

            var command = new ExecuteParameters(fileSystem: fileSystem.Object)
            {
                File = @"C:\test.ps1",
                Arguments = "-Arg1 foo -Arg2 bar"
            }
            .GetCommand();

            Assert.IsTrue(command.IsScript);
            Assert.AreEqual(@"& 'C:\test.ps1' -Arg1 foo -Arg2 bar", command.CommandText);
        }

        [Test]
        public void WhenFileAndExpressionAreSpecified()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new ExecuteParameters
                {
                    File = @"C:\test.ps1",
                    Expression = "test"
                }
                .GetCommand());

            Assert.AreEqual(
                "You cannot specify both the Expression and File parameters simultaneously.",
                exception.Message);
        }

        [Test]
        public void WhenExpressionAndArgumentsAreSpecified()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new ExecuteParameters
                {
                    Expression = "test",
                    Arguments = "-Arg1 foo -Arg2 bar"
                }
                .GetCommand());

            Assert.AreEqual(
                "The Arguments parameter can only be specified with the File parameter.",
                exception.Message);
        }

        [Test]
        public void WhenFileOrExpressionAreNotSpecified()
        {
            var exception = Assert.Throws<ArgumentException>(() => new ExecuteParameters().GetCommand());

            Assert.AreEqual(
                "Either the Expression or File parameter must be specified.",
                exception.Message);
        }

        [Test]
        public void WhenNonPs1FileIsSpecified()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new ExecuteParameters
                {
                    File = @"C:\test.txt"
                }
                .GetCommand());

            Assert.AreEqual(
                @"Processing File 'C:\test.txt' failed because the file does not have a '.ps1' " +
                @"extension. Specify a valid Windows PowerShell script file name, and then try again.",
                exception.Message);
        }

        [Test]
        public void WhenNonExistentFileIsSpecified()
        {
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns((string path) => false);
            fileSystem.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => path);

            var exception = Assert.Throws<ArgumentException>(() =>
                new ExecuteParameters(fileSystem: fileSystem.Object)
                {
                    File = @"C:\test.ps1"
                }
                .GetCommand());

            Assert.AreEqual(
                @"The argument 'C:\test.ps1' to the File parameter does not exist. Provide the path " +
                @"to an existing '.ps1' file as an argument to the File parameter.",
                exception.Message);
        }

        [Test]
        public void WhenRelativeFilePathIsSpecified()
        {
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns((string path) => true);
            fileSystem.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => Path.Combine(@"C:\", path));

            var command = new ExecuteParameters(fileSystem: fileSystem.Object)
            {
                File = @"test.ps1"
            }
            .GetCommand();
            Assert.IsTrue(command.IsScript);
            Assert.AreEqual(@"& 'C:\test.ps1'", command.CommandText);
        }
    }
}
