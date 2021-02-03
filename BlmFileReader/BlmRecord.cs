using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlmFileReader
{
    public class BlmRecord
    {
        public int LineNumber { get; }
        public IReadOnlyDictionary<string, string> Fields { get; }

        public BlmRecord(IReadOnlyList<string> definitions, IReadOnlyList<string> values, int lineNumber)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (definitions.Count != values.Count)
            {
                throw new ArgumentException("Definitions and values do not match");
            }

            var fields = new Dictionary<string,string>();
            for (var i = 0; i < definitions.Count; i++)
            {
                fields.Add(definitions[i], values[i]);
            }

            Fields = new ReadOnlyDictionary<string, string>(fields);
            LineNumber = lineNumber;
        }
    }
}