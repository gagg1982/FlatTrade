using FlatTrade.Common.Types;
using FlatTrade.SubscriptionManager.Helper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FlatTrade.Common.Helpers
{
    public class DbWriter
    {
        private readonly ILogger<DbWriter> _logger;
        private readonly Channel<DbChannelObject> _dbChannel;
        private readonly int _batchCount;
        private readonly Task _task;
        private readonly string _connectionString;
        private readonly string _dbWriterFriendlyName;
        public DbWriter(string connectionString, int batchCount, int dbChannelCapacity, string dbWriterFriendlyName, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DbWriter>();
            _connectionString = connectionString;
            _batchCount = batchCount;
            _dbWriterFriendlyName = dbWriterFriendlyName;
            _dbChannel = HelperUtility.CreateBoundedChannel<DbChannelObject>(dbChannelCapacity);
            _task = WriteDbAsync();
        }

        public async Task WriteComplete()
        {
            _dbChannel.Writer.Complete();
            await _task;
        }


        public async Task WriteDbAsync(DbChannelObject obj) => await _dbChannel.Writer.WriteAsync(obj!);

        internal async Task WriteDbAsync()
        {
            _logger.LogInformation("====== [{_dbWriterFriendlyName}] Starting to process DbRecords ======", _dbWriterFriendlyName);
            int objectsProcessed = 0;
            int objectsfailed = 0;

            await foreach (var reader in _dbChannel.Reader.ReadAllAsync())
            {
                ++objectsProcessed;
                int batchNumber = 0;
                foreach (var batch in reader.Records.AsEnumerable().Chunk(_batchCount))
                {
                    var table = batch.CopyToDataTable();
                    try
                    {
                        ++batchNumber;
                        await InsertBatchToSqlServerAsync(table, reader.StoredProcedureName, reader.TvpName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "[{_dbWriterFriendlyName}] Error: Batch/DbRecord: {batchNumber}/{recordCount}. Error {ex.Message}", _dbWriterFriendlyName, batchNumber, reader.Records.Rows.Count, ex.Message);
                        ++objectsfailed;
                    }
                    finally
                    {
                        _logger.LogDebug("[{_dbWriterFriendlyName}] Success: Batches/DbRecords: {batchNumber}/{recordCount}", _dbWriterFriendlyName, batchNumber, reader.Records.Rows.Count);
                    }
                }
                if ((objectsProcessed + objectsfailed) % 500 == 0)
                    _logger.LogInformation("[{_dbWriterFriendlyName}] Total: {objectsProcessed}, Success: {objectsProcessed}{objectsfailed}", _dbWriterFriendlyName, objectsProcessed + objectsfailed, objectsProcessed, objectsfailed > 0 ? ", Failed: " + objectsfailed : string.Empty);
            }
            _logger.LogInformation("====== [{_dbWriterFriendlyName}] Finished processing DbRecords. Total: {total}, Success: {processed}, Failed: {failed} ======",
                _dbWriterFriendlyName, objectsProcessed + objectsfailed, objectsProcessed, objectsfailed);
        }

        private async Task InsertBatchToSqlServerAsync(DataTable batch, string storedProcedureName, string tvpTypeName)
        {
            if (batch == null || batch.Rows.Count == 0)
            {
                return;
            }

            using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                using var command = new SqlCommand(storedProcedureName, connection);

                command.CommandType = CommandType.StoredProcedure;

                // Create the SQL Parameter for the TVP
                SqlParameter tvpParam = command.Parameters.AddWithValue("@tvpData", batch);
                tvpParam.SqlDbType = SqlDbType.Structured;
                tvpParam.TypeName = tvpTypeName; // IMPORTANT: Must match the SQL Server TVP type name

                // Execute the stored procedure
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogCritical(ex, "   Error: batch insert: {ex.Message}", ex.Message);
                // Log more details from ex.Errors if needed for debugging SQL issues
                foreach (SqlError error in ex.Errors)
                {
                    _logger.LogCritical(ex, "  Error Detail: {error.Message} (Line: {error.LineNumber}, Number: {error.Number})", error.Message, error.LineNumber, error.Number);
                }
                throw; // Re-throw to indicate failure
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "   An unexpected error occurred during SQL insert: {ex.Message}", ex.Message);
                throw; // Re-throw to indicate failure
            }
        }
    }
}
