using System;
using System.Runtime.Serialization;

namespace BlmFileReader
{
    [Serializable]
    public class BlmFileParseException : Exception
    {
        public int LineNumber { get; }

        public BlmFileParseException(string message, int lineNumber) : base(message)
        {
            LineNumber = lineNumber;
        }

        public BlmFileParseException(string message, Exception inner, int lineNumber) : base(message, inner)
        {
            LineNumber = lineNumber;
        }

        protected BlmFileParseException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}