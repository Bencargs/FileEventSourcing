using FileEvents;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileEventTests
{
    public class SqliteRepositoryTests
    {
        [Test]
        public async Task CreateTest()
        {
            using var file = new TemporaryFile();
            var target = new SqliteRepository();
            
            await target.Create(file.Fullname);

            Assert.True(File.Exists(file.Fullname));
        }

        [Test]
        public async Task IsEmptyTest()
        {
            using var file = new TemporaryFile();
            var target = new SqliteRepository();
            await target.Create(file.Fullname);

            var initial = await target.IsEmpty(file.Fullname);
            await target.AddRecordAsync(file.Fullname, "Test");
            var subsequent = await target.IsEmpty(file.Fullname);

            Assert.IsTrue(initial);
            Assert.IsFalse(subsequent);
        }

        [Test]
        public async Task ReadRecordsTest()
        {
            using var file = new TemporaryFile();
            var target = new SqliteRepository();
            await target.Create(file.Fullname);

            await target.AddRecordAsync(file.Fullname, "Test");
            var records = await target.ReadRecordsAsync(file.Fullname).ToArrayAsync();

            Assert.AreEqual(records, new[] { "Test" });
        }
    }
}
