using System;
using System.Collections;
using Microsoft.Build.Framework;

namespace PowerBridge.Tests.Mocks
{
    public class MockPropertyTaskItem : ITaskItem
    {
        private readonly string _value;

        public MockPropertyTaskItem(string name, string value)
        {
            ItemSpec = name;
            _value = value;
        }

        public string GetMetadata(string metadataName)
        {
            if (metadataName == "Value")
            {
                return _value;
            }

            throw new ArgumentException("metadataName");
        }

        public void SetMetadata(string metadataName, string metadataValue)
        {
            throw new NotImplementedException();
        }

        public void RemoveMetadata(string metadataName)
        {
            throw new NotImplementedException();
        }

        public void CopyMetadataTo(ITaskItem destinationItem)
        {
            throw new NotImplementedException();
        }

        public IDictionary CloneCustomMetadata()
        {
            throw new NotImplementedException();
        }

        public string ItemSpec { get; set; }

        public ICollection MetadataNames
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MetadataCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}