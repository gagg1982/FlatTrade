using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace FlatTrade.Common.Helpers
{
    public class DbReader
    {
        private readonly ILogger<DbReader> _logger;
        private readonly string _connectionString;

        public DbReader(string connectionString, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DbReader>();
            _connectionString = connectionString;
        }

        public async Task<List<T>> ExecuteStoredProcedure<T>(
           string storedProcName,
           Dictionary<string, object>? parameters,
           Func<IDataReader, T> mapFunction)
        {
            var result = new List<T>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(storedProcName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add parameters dynamically
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    command.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                }
            }

            connection.Open();
            using var reader = await command.ExecuteReaderAsync();

            // Custom mapping
            while (reader.Read())
            {
                result.Add(mapFunction(reader));
            }

            return result;
        }
    }
}
