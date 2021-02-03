using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlmConvert
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: blmconvert.exe <filename>");
                return 0;
            }

            var filename = args[0];
            var csvFilename = Path.ChangeExtension(filename, ".csv");
            
            try
            {
                Console.WriteLine("Converting...");

                using var blmFileReader = new BlmFileReader.BlmFileReader(File.OpenRead(filename));

                await blmFileReader.ReadHeader();

                using var csvFile = new StreamWriter(File.Create(csvFilename), blmFileReader.Encoding);

                var headerRow = string.Join(",", blmFileReader.Definitions);
                await csvFile.WriteLineAsync(headerRow);

                var rowsWritten = 0;
                var record = await blmFileReader.ReadRecord();

                while (record != null)
                {
                    var dataRow = string.Join(",", record.Fields.Values.Select(EscapeCsvValue));
                    await csvFile.WriteLineAsync(dataRow);

                    rowsWritten++;
                    
                    record = await blmFileReader.ReadRecord();
                }
                
                Console.WriteLine($"Complete - {rowsWritten} written");
                return 0;
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.ToString());
                await Console.Error.WriteLineAsync($"Error processing '{filename}'");
                return 1;
            }
        }
        
        private static string EscapeCsvValue(string value)
        {
            const string doubleQuote = "\"";

            var escapedValue = value.Replace(doubleQuote, doubleQuote + doubleQuote);

            return $"{doubleQuote}{escapedValue}{doubleQuote}";
        }
    }
}