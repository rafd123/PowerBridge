using System.Management.Automation.Runspaces;
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
        public void WhenExpressionIsNotSpecified()
        {
            var taskLog = new MockBuildTaskLog();
            Command command;
            var result = new ExecuteParameters().TryGetCommand(taskLog, out command);

            Assert.IsFalse(result);
            taskLog.AssertLogEntriesAre(
                new LogErrorMessageOnly("The Expression parameter must be specified."));
            Assert.IsNull(command);
        }
    }
}