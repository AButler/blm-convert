using CommandLine;

namespace BlmConverter
{
    public class Arguments
    {
        [Option('i', "input", HelpText = "BLM input filename")]
        public string InputFilename { get; set; }

        [Option('o', "output", HelpText = "CSV output filename")]
        public string OutputFilename { get; set; }

        [Option('c', "convert", HelpText = "Convert without opening UI")]
        public bool AutoConvert { get; set; }

        public static Arguments Default => new Arguments
        {
            AutoConvert = false,
            InputFilename = null,
            OutputFilename = null
        };
    }
}