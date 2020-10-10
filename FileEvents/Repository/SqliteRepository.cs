using Dapper;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace FileEvents
{
    public class SqliteRepository : IRepository
    {
        public async Task Create(string path)
        {
            SQLiteConnection.CreateFile(path);
            using var connection = await GetConnection(path);

            const string sql = @"create table events ( bookmark INTEGER PRIMARY KEY, content text )";
            await connection.ExecuteAsync(sql);
        }

        public async Task<bool> IsEmpty(string path)
        {
            using var connection = await GetConnection(path);

            const string sql = @"select count(bookmark) from events";
            var count = await connection.ExecuteScalarAsync<int>(sql);
            return count == 0;
        }

        public async Task AddRecordAsync(string path, string content)
        {
            using var connection = await GetConnection(path);

            const string sql = @"insert into events ( content ) values (@content)";
            await connection.ExecuteAsync(sql, new { content });
        }

        public async IAsyncEnumerable<string> ReadRecordsAsync(string path)
        {
            using var connection = await GetConnection(path);

            const string sql = @"select content from events order by bookmark asc";
            var records = await connection.QueryAsync<string>(sql);

            foreach (var r in records)
            {
                yield return r;
            }
        }

        private async Task<SQLiteConnection> GetConnection(string path)
        {
            var connection = new SQLiteConnection($"Data Source={path};Version=3;");
            await connection.OpenAsync();
            return connection;
        }
    }
}
