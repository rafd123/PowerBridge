using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Threading;

namespace PowerBridge.Internal
{
    internal sealed class PowerShellHost : PSHost
    {
        private readonly UserInterface _ui;
        private readonly Guid _instanceId = Guid.NewGuid();

        public PowerShellHost(IPowerShellHostOutput output)
        {
            _ui = new UserInterface(output);
        }

        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return Thread.CurrentThread.CurrentCulture; }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return Thread.CurrentThread.CurrentUICulture; }
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override Guid InstanceId
        {
            get { return _instanceId; }
        }

        public override string Name
        {
            get { return typeof(PowerShellHost).Name; }
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override void SetShouldExit(int exitCode)
        {
        }

        public override PSHostUserInterface UI
        {
            get { return _ui; }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        private class UserInterface : PSHostUserInterface
        {
            private readonly IPowerShellHostOutput _output;

            public UserInterface(IPowerShellHostOutput output)
            {
                _output = output;
            }

            public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
            {
                throw NonInteractiveException();
            }

            public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
            {
                throw NonInteractiveException();
            }

            public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
            {
                throw NonInteractiveException();
            }

            public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
            {
                throw NonInteractiveException();
            }

            public override PSHostRawUserInterface RawUI
            {
                get { return RawUserInterface.Instance; }
            }

            public override string ReadLine()
            {
                throw NonInteractiveException();
            }

            public override SecureString ReadLineAsSecureString()
            {
                throw NonInteractiveException();
            }

            public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
            {
                _output.Write(value);
            }

            public override void Write(string value)
            {
                _output.Write(value);
            }

            public override void WriteDebugLine(string message)
            {
                _output.WriteDebugLine(message);
            }

            public override void WriteErrorLine(string value)
            {
                _output.WriteErrorLine(value);
            }

            public override void WriteLine(string value)
            {
                _output.WriteLine(value);
            }

            public override void WriteProgress(long sourceId, ProgressRecord record)
            {
            }

            public override void WriteVerboseLine(string message)
            {
                _output.WriteVerboseLine(message);
            }

            public override void WriteWarningLine(string message)
            {
                _output.WriteWarningLine(message);
            }

            private static Exception NonInteractiveException()
            {
                return new InvalidOperationException("Windows PowerShell is in non-interactive mode. Read and Prompt functionality is not available.");
            }

            private class RawUserInterface : PSHostRawUserInterface
            {
                public static readonly RawUserInterface Instance = new RawUserInterface();

                private RawUserInterface()
                {
                }

                public override ConsoleColor BackgroundColor
                {
                    get
                    {
                        return ConsoleColor.Black;
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public override Size BufferSize
                {
                    get
                    {
                        return new Size(80, 20);
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public override Coordinates CursorPosition
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public override int CursorSize
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public override void FlushInputBuffer()
                {
                    throw new NotImplementedException();
                }

                public override ConsoleColor ForegroundColor
                {
                    get
                    {
                        return ConsoleColor.White;
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public override BufferCell[,] GetBufferContents(Rectangle rectangle)
                {
                    throw new NotImplementedException();
                }

                public override bool KeyAvailable
                {
                    get { throw new NotImplementedException(); }
                }

                public override Size MaxPhysicalWindowSize
                {
                    get { throw new NotImplementedException(); }
                }

                public override Size MaxWindowSize
                {
                    get { throw new NotImplementedException(); }
                }

                public override KeyInfo ReadKey(ReadKeyOptions options)
                {
                    throw new NotImplementedException();
                }

                public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
                {
                    throw new NotImplementedException();
                }

                public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
                {
                    throw new NotImplementedException();
                }

                public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
                {
                    throw new NotImplementedException();
                }

                public override Coordinates WindowPosition
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public override Size WindowSize
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public override string WindowTitle
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }
}