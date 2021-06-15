using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlmFileReader
{
    public class BlmFileReader : IDisposable
    {
        private const string HeaderTag = "#HEADER#";
        private const string DefinitionTag = "#DEFINITION#";
        private const string DataTag = "#DATA#";
        private const string EndTag = "#END#";

        private static readonly Regex HeaderAttributeRegex = new Regex("^(?<Attribute>.+) : (?<Value>.*)$");

        private readonly StreamReader _stream;

        private int _lineNumber;
        private char _endOfFieldCharacter;
        private char _endOfRecordCharacter;
        private IReadOnlyList<string> _definitions;
        private bool _headerRead;
        private bool _completed;
        private bool _hasEOFBeforeEOR;

        public IReadOnlyList<string> Definitions
        {
            get
            {
                if (!_headerRead)
                {
                    throw new InvalidOperationException("Header must be read first");
                }

                return _definitions.ToList();
            }
        }

        public Encoding Encoding => _stream.CurrentEncoding;

        public BlmFileReader(Stream stream)
        {
            _stream = new StreamReader(stream, Encoding.GetEncoding("iso-8859-1"), true);
            _stream.Peek();
            _lineNumber = 0;
            _definitions = new List<string>();
            _hasEOFBeforeEOR = true;
        }

        public async Task ReadHeader()
        {
            if (_headerRead)
            {
                throw new InvalidOperationException("Header has already been read");
            }

            var line = await ReadLine();
            
            while (string.IsNullOrWhiteSpace(line))
            {
                line = await ReadLine();
            }

            if (!string.Equals(line, HeaderTag))
            {
                throw new BlmFileParseException($"Missing {HeaderTag} header", _lineNumber);
            }

            var attributes = await ReadHeaderAttributes();

            ValidateAttributes(attributes);

            var definitions = await ReadDefinitions();

            _definitions = definitions;

            _headerRead = true;
        }

        public async Task<BlmRecord> ReadRecord()
        {
            if (!_headerRead)
            {
                throw new InvalidOperationException("Header must be read before records");
            }

            if (_completed)
            {
                return null;
            }

            var data = await ReadLine();

            while (string.IsNullOrWhiteSpace(data))
            {
                data = await ReadLine();
            }

            if (string.Equals(data, EndTag))
            {
                _stream.Close();
                _completed = true;
                return null;
            }

            while (!data.EndsWith(_endOfRecordCharacter.ToString()))
            {
                data += Environment.NewLine;
                data += await ReadLine();
            }

            var fields = SplitLine(data);

            return new BlmRecord(_definitions, fields, _lineNumber);
        }

        private IReadOnlyList<string> SplitLine(string data)
        {
            var charsToRemove = _hasEOFBeforeEOR ? 2 : 1;
            var dataWithoutTrailingChars = data.Substring(0, data.Length - charsToRemove);
            return dataWithoutTrailingChars.Split(_endOfFieldCharacter);
        }

        private async Task<IReadOnlyList<string>> ReadDefinitions()
        {
            var line = await ReadLine();
            var data = "";

            while (!string.Equals(line, DataTag))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    data += line;
                }

                line = await ReadLine();
            }

            if (!data.EndsWith($"{_endOfFieldCharacter}{_endOfRecordCharacter}"))
            {
                if (!data.EndsWith(_endOfRecordCharacter.ToString()))
                {
                    throw new BlmFileParseException("Invalid definitions", _lineNumber - 1);
                }

                _hasEOFBeforeEOR = false;
            }

            return SplitLine(data);
        }

        private void ValidateAttributes(IDictionary<string, (string Value, int LineNumber)> attributes)
        {
            if (!attributes.TryGetValue("Version", out var version))
            {
                throw new BlmFileParseException("Missing Version attribute", _lineNumber);
            }

            if (string.IsNullOrWhiteSpace(version.Value) || !string.Equals(version.Value, "3"))
            {
                throw new BlmFileParseException("Invalid version - only version 3 is supported", version.LineNumber);
            }

            if (!attributes.TryGetValue("EOF", out var endOfFieldChar))
            {
                throw new BlmFileParseException("Missing EOF attribute", _lineNumber);
            }

            if (!IsValidCharacter(endOfFieldChar.Value))
            {
                throw new BlmFileParseException("Invalid EOF character - only a single character is allowed",
                    endOfFieldChar.LineNumber);
            }

            _endOfFieldCharacter = endOfFieldChar.Value[1];

            if (!attributes.TryGetValue("EOR", out var endOfRecordChar))
            {
                throw new BlmFileParseException("Missing EOR attribute", _lineNumber);
            }

            if (!IsValidCharacter(endOfRecordChar.Value))
            {
                throw new BlmFileParseException("Invalid EOR character - only a single character is allowed",
                    endOfRecordChar.LineNumber);
            }

            _endOfRecordCharacter = endOfRecordChar.Value[1];
        }

        private static bool IsValidCharacter(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!value.StartsWith("'"))
            {
                return false;
            }

            if (!value.EndsWith("'"))
            {
                return false;
            }

            if (value.Length != 3)
            {
                return false;
            }

            return true;
        }

        private async Task<IDictionary<string, (string Value, int LineNumber)>> ReadHeaderAttributes()
        {
            var attributes = new Dictionary<string, (string Value, int LineNumber)>();

            var line = await ReadLine();

            do
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var match = HeaderAttributeRegex.Match(line);
                    if (!match.Success)
                    {
                        throw new BlmFileParseException("Invalid attribute", _lineNumber);
                    }

                    attributes.Add(match.Groups["Attribute"].Value, (match.Groups["Value"].Value, _lineNumber));
                }

                line = await ReadLine();
            } while (!string.Equals(line, DefinitionTag));

            return attributes;
        }

        private async Task<string> ReadLine()
        {
            if (_stream.EndOfStream)
            {
                throw new BlmFileParseException($"Missing {EndTag}", _lineNumber);
            }

            var line = await _stream.ReadLineAsync();
            _lineNumber++;
            return line;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}