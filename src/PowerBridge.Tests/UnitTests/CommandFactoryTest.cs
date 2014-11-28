using System;
using System.IO;
using Microsoft.Build.Framework;
using Moq;
using NUnit.Framework;
using PowerBridge.Internal;

namespace PowerBridge.Tests.UnitTests
{
    [TestFixture]
    public class CommandFactoryTest
    {
        [Test]
        public void WhenExpressionIsSpecified()
        {
            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var command = new CommandFactory
            {
                Expression = "test"
            }
            .CreateCommand(commandProvider.Object);

            Assert.IsTrue(command.IsScript);
            Assert.AreEqual("test", command.CommandText);
        }

        [Test]
        public void WhenFileIsSpecified()
        {
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns((string path) => true);
            fileSystem.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => path);

            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var command = new CommandFactory(fileSystem: fileSystem.Object)
            {
                File = @"C:\test.ps1"
            }
            .CreateCommand(commandProvider.Object);

            Assert.IsTrue(command.IsScript);
            Assert.AreEqual(@"& 'C:\test.ps1'", command.CommandText);
        }

        [Test]
        public void WhenFileIsSpecifiedWithArguments()
        {
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns((string path) => true);
            fileSystem.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => path);

            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var command = new CommandFactory(fileSystem: fileSystem.Object)
            {
                File = @"C:\test.ps1",
                Arguments = "-Arg1 foo -Arg2 bar"
            }
            .CreateCommand(commandProvider.Object);

            Assert.IsTrue(command.IsScript);
            Assert.AreEqual(@"& 'C:\test.ps1' -Arg1 foo -Arg2 bar", command.CommandText);
        }

        [Test]
        public void WhenFileAndExpressionAreSpecified()
        {
            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var exception = Assert.Throws<ArgumentException>(() =>
                new CommandFactory
                {
                    File = @"C:\test.ps1",
                    Expression = "test"
                }
                .CreateCommand(commandProvider.Object));

            Assert.AreEqual(
                "You cannot specify both the Expression and File parameters simultaneously.",
                exception.Message);
        }

        [Test]
        public void WhenArgumentsAndAutoParametersAreSpecified()
        {
            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var exception = Assert.Throws<ArgumentException>(() =>
                new CommandFactory
                {
                    File = @"C:\test.ps1",
                    Arguments = "-Arg1 foo -Arg2 bar",
                    AutoParameters = new ITaskItem[0]
                }
                .CreateCommand(commandProvider.Object));

            Assert.AreEqual(
                "You cannot specify both the Arguments and AutoParameters parameters simultaneously.",
                exception.Message);
        }

        [Test]
        public void WhenExpressionAndArgumentsAreSpecified()
        {
            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var exception = Assert.Throws<ArgumentException>(() =>
                new CommandFactory
                {
                    Expression = "test",
                    Arguments = "-Arg1 foo -Arg2 bar"
                }
                .CreateCommand(commandProvider.Object));

            Assert.AreEqual(
                "The Arguments parameter can only be specified with the File parameter.",
                exception.Message);
        }

        [Test]
        public void WhenExpressionAndAutoParametersAreSpecified()
        {
            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var exception = Assert.Throws<ArgumentException>(() =>
                new CommandFactory
                {
                    Expression = "test",
                    AutoParameters = new ITaskItem[0]
                }
                .CreateCommand(commandProvider.Object));

            Assert.AreEqual(
                "The AutoParameters parameter can only be specified with the File parameter.",
                exception.Message);
        }

        [Test]
        public void WhenFileOrExpressionAreNotSpecified()
        {
            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var exception = Assert.Throws<ArgumentException>(() => new CommandFactory().CreateCommand(commandProvider.Object));

            Assert.AreEqual(
                "Either the Expression or File parameter must be specified.",
                exception.Message);
        }

        [Test]
        public void WhenNonPs1FileIsSpecified()
        {
            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var exception = Assert.Throws<ArgumentException>(() =>
                new CommandFactory
                {
                    File = @"C:\test.txt"
                }
                .CreateCommand(commandProvider.Object));

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

            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var exception = Assert.Throws<ArgumentException>(() =>
                new CommandFactory(fileSystem: fileSystem.Object)
                {
                    File = @"C:\test.ps1"
                }
                .CreateCommand(commandProvider.Object));

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

            var commandProvider = new Mock<IPowerShellCommandParameterProvider>();

            var command = new CommandFactory(fileSystem: fileSystem.Object)
            {
                File = @"test.ps1"
            }
            .CreateCommand(commandProvider.Object);
            Assert.IsTrue(command.IsScript);
            Assert.AreEqual(@"& 'C:\test.ps1'", command.CommandText);
        }
    }
}
