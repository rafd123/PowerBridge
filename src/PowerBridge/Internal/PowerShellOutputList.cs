using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerBridge.Internal
{
    internal class PowerShellOutputList : IList<PSObject>
    {
        private static readonly Type FormatInfoData =
            Type.GetType("Microsoft.PowerShell.Commands.Internal.Format.FormatInfoData, " + typeof(PowerShell).Assembly.FullName, throwOnError: true);

        private static readonly Type FormatEndDataType =
            Type.GetType("Microsoft.PowerShell.Commands.Internal.Format.FormatEndData, " + typeof(PowerShell).Assembly.FullName, throwOnError: true);

        private readonly IPowerShellHostOutput _powerShellHostOutput;
        private readonly IPowerShellStringProvider _powerShellStringProvider;
        private readonly List<PSObject> _formatBuffer = new List<PSObject>();

        public PowerShellOutputList(IPowerShellHostOutput powerShellHostOutput, IPowerShellStringProvider powerShellStringProvider)
        {
            _powerShellHostOutput = powerShellHostOutput;
            _powerShellStringProvider = powerShellStringProvider;
        }

        void ICollection<PSObject>.Add(PSObject item)
        {
            string s;
            if (TryGetString(item, out s))
            {
                _powerShellHostOutput.Write(s);    
            }
        }

        void IList<PSObject>.Insert(int index, PSObject item)
        {
            string s;
            if (TryGetString(item, out s))
            {
                _powerShellHostOutput.Write(s);
            }
        }

        private bool TryGetString(PSObject o, out string s)
        {
            if (o.BaseObject != null && FormatInfoData.IsInstanceOfType(o.BaseObject))
            {
                _formatBuffer.Add(o);

                if (!FormatEndDataType.IsInstanceOfType(o.BaseObject))
                {
                    s = null;
                    return false;                    
                }

                s = _powerShellStringProvider.ToString(_formatBuffer);
                _formatBuffer.Clear();
                return true;
            }

            s = _powerShellStringProvider.ToString(o);
            return true;
        }

        public IEnumerator<PSObject> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<PSObject>.Clear()
        {
        }

        bool ICollection<PSObject>.Contains(PSObject item)
        {
            return false;
        }

        void ICollection<PSObject>.CopyTo(PSObject[] array, int arrayIndex)
        {
        }

        bool ICollection<PSObject>.Remove(PSObject item)
        {
            return false;
        }

        int ICollection<PSObject>.Count
        {
            get { return 0; }
        }

        bool ICollection<PSObject>.IsReadOnly
        {
            get { return false; }
        }

        int IList<PSObject>.IndexOf(PSObject item)
        {
            return -1;
        }

        void IList<PSObject>.RemoveAt(int index)
        {
        }

        PSObject IList<PSObject>.this[int index]
        {
            get { throw new ArgumentOutOfRangeException(); }
            set { }
        }
    }
}