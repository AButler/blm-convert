using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BlmFileReader.UnitTests
{
    [TestFixture]
    public class BlmFileReaderTests
    {
        private static readonly string[] ExpectedDefinitions = {
            "AGENT_REF", "ADDRESS_1", "ADDRESS_2", "ADDRESS_3", "TOWN", "POSTCODE1", "POSTCODE2"
        };

        [TestCase("HeaderNoData.blm")]
        [TestCase("HeaderNoDataSpaces.blm")]
        [TestCase("HeaderNoDataAltSeparator.blm")]
        [TestCase("HeaderNoDataSpacesAltSeparator.blm")]
        public async Task EmptyFile(string filename)
        {
            using var blmFileReader = new BlmFileReader(GetTestFile(filename));

            await blmFileReader.ReadHeader();

            Assert.That(blmFileReader.Definitions, Has.Count.EqualTo(ExpectedDefinitions.Length));

            var record = await blmFileReader.ReadRecord();

            Assert.That(record, Is.Null);
        }
        
        [TestCase("SimpleData.blm")]
        [TestCase("SimpleDataSpaces.blm")]
        [TestCase("SimpleDataAltSeparator.blm")]
        [TestCase("SimpleDataSpacesAltSeparator.blm")]
        [TestCase("SimpleDataMissingEOF.blm")]
        [TestCase("SimpleDataSpacesMissingEOF.blm")]
        [TestCase("SimpleDataAltSeparatorMissingEOF.blm")]
        [TestCase("SimpleDataSpacesAltSeparatorMissingEOF.blm")]
        public async Task SimpleData(string filename)
        {
            using var blmFileReader = new BlmFileReader(GetTestFile(filename));

            await blmFileReader.ReadHeader();

            Assert.That(blmFileReader.Definitions, Has.Count.EqualTo(ExpectedDefinitions.Length));

            var record = await blmFileReader.ReadRecord();

            Assert.That(record, Is.Not.Null);
            
            Assert.That(record.Fields, Has.Count.EqualTo(ExpectedDefinitions.Length));
            
            Assert.That(record.Fields["AGENT_REF"], Is.EqualTo("REF_123")); 
            Assert.That(record.Fields["ADDRESS_1"], Is.EqualTo("1"));
            Assert.That(record.Fields["ADDRESS_2"], Is.EqualTo("Bobbinton Road"));
            Assert.That(record.Fields["ADDRESS_3"], Is.EqualTo(""));
            Assert.That(record.Fields["TOWN"], Is.EqualTo("Bobsville"));
            Assert.That(record.Fields["POSTCODE1"], Is.EqualTo("BO6"));
            Assert.That(record.Fields["POSTCODE2"], Is.EqualTo("3RT"));
        }

        private static Stream GetTestFile(string filename)
        {
            return typeof(BlmFileReaderTests)
                .Assembly
                .GetManifestResourceStream(typeof(BlmFileReaderTests), $"TestFiles.{filename}");
        }
    }
}