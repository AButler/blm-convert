using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BlmFileReader.UnitTests
{
    [TestFixture]
    public class BlmFileReaderTests
    {
        [TestCase("HeaderNoData.blm")]
        [TestCase("HeaderNoDataSpaces.blm")]
        public async Task EmptyFile(string filename)
        {
            using var blmFileReader = new BlmFileReader(GetTestFile(filename));

            await blmFileReader.ReadHeader();

            var expectedDefinitions = new[]
            {
                "AGENT_REF", "ADDRESS_1", "ADDRESS_2", "ADDRESS_3", "TOWN", "POSTCODE1", "POSTCODE2"
            };

            Assert.That(blmFileReader.Definitions, Has.Count.EqualTo(expectedDefinitions.Length));

            var record = await blmFileReader.ReadRecord();

            Assert.That(record, Is.Null);
        }

        private static Stream GetTestFile(string filename)
        {
            return typeof(BlmFileReaderTests)
                .Assembly
                .GetManifestResourceStream(typeof(BlmFileReaderTests), $"TestFiles.{filename}");
        }
    }
}