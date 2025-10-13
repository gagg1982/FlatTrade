using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Threading.Channels;

namespace DailyRunner.Helpers
{
    internal class CsvToSqlImporter(string connectionString, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<CsvToSqlImporter> _logger = loggerFactory.CreateLogger<CsvToSqlImporter>();
        private readonly string _connectionString = connectionString;

        /// <summary>
        /// Processes a CSV file, reading data in batches and inserting it into SQL Server via TVP.
        /// </summary>
        /// <param name="csvFilePath">The path to the CSV file.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task ImportCsvToSqlServerAsync<CsvRow>(string csvFilePath, ClassMap<CsvRow> csvRowMap, int batchSize, string storedProcedureName, string tvpTypeName)
        {
            if (!File.Exists(csvFilePath))
            {
                _logger.LogError("Error: CSV file not found at '{CsvFilePath}'", csvFilePath);
                return;
            }

            try
            {
                using var reader = new StreamReader(csvFilePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });

                _logger.LogInformation("[Reader] Starting CSV import from : {csvFilePath}, Batch Size: {_batchSize}", csvFilePath, batchSize);
                // Register the class map if you're using it
                csv.Context.RegisterClassMap(csvRowMap);

                // Read the header row
                await csv.ReadAsync();
                csv.ReadHeader();

                var batch = new List<CsvRow>();
                long totalRecordsProcessed = 0;
                int batchCount = 0;

                while (await csv.ReadAsync())
                {
                    try
                    {
                        var record = csv.GetRecord<CsvRow>();
                        if (record is not null)
                        {
                            batch.Add(record);
                        }
                    }
                    catch (CsvHelperException ex)
                    {
                        _logger.LogError(ex, "  Warning: Error reading CSV record at row {batch.Count}", batch.Count);
                        // Optionally, log the problematic row data or skip it
                        continue; // Skip to the next record
                    }

                    if (batch.Count >= batchSize)
                    {
                        batchCount++;
                        _logger.LogInformation("  Processing batch {batchCount} ({batch.Count} records)...", batchCount, batch.Count);
                        await InsertBatchToSqlServerAsync(batch, storedProcedureName, tvpTypeName);
                        totalRecordsProcessed += batch.Count;
                        batch.Clear(); // Clear the batch for the next set of records
                    }
                }

                // Process any remaining records in the last batch
                if (batch.Count > 0)
                {
                    batchCount++;
                    _logger.LogInformation("  Processing batch {batchCount} ({batch.Count} records)...", batchCount, batch.Count);
                    await InsertBatchToSqlServerAsync(batch, storedProcedureName, tvpTypeName);
                    totalRecordsProcessed += batch.Count;
                }

                _logger.LogInformation("CSV import and upload into SQL server completed successfully. Total records processed: {totalRecordsProcessed} in {batchCount} batches for file {csvFilePath}", totalRecordsProcessed, batchCount, csvFilePath);

            }
            catch (FileNotFoundException ex)
            {
                _logger.LogCritical(ex, "  Error: The file '{csvFilePath}' was not found.", csvFilePath);
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogCritical(ex, "  Error: The directory for '{csvFilePath}' was not found.", csvFilePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogCritical(ex, "  Error: Access to the file '{csvFilePath}' is denied. Check file permissions.", csvFilePath);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "  Error reading CSV file: {ex.Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "  An unexpected error occurred during CSV import: {ex.Message}", ex.Message);
            }
        }

        /// <summary>
        /// Inserts a batch of CsvRow records into SQL Server using a Table-Valued Parameter.
        /// </summary>
        /// <param name="batch">The list of CsvRow records to insert.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task InsertBatchToSqlServerAsync<CsvRow>(List<CsvRow> batch, string storedProcedureName, string tvpTypeName)
        {
            if (batch == null || batch.Count == 0)
            {
                return;
            }

            using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                using var command = new SqlCommand(storedProcedureName, connection);

                command.CommandType = CommandType.StoredProcedure;

                // Create a DataTable to hold the batch data, matching the TVP type schema
                var tvpDataTable = ToDataTable(batch);

                // Create the SQL Parameter for the TVP
                SqlParameter tvpParam = command.Parameters.AddWithValue("@tvpData", tvpDataTable);
                tvpParam.SqlDbType = SqlDbType.Structured;
                tvpParam.TypeName = tvpTypeName; // IMPORTANT: Must match the SQL Server TVP type name

                // Execute the stored procedure
                await command.ExecuteNonQueryAsync();
                _logger.LogInformation("    [Writer] Successfully inserted {BatchCount} records into SQL Server.", batch.Count);
            }
            catch (SqlException ex)
            {
                _logger.LogCritical(ex, "   SQL Error during batch insert: {ex.Message}", ex.Message);
                // Log more details from ex.Errors if needed for debugging SQL issues
                foreach (SqlError error in ex.Errors)
                {
                    _logger.LogCritical(ex, "  SQL Error Detail: {error.Message} (Line: {error.LineNumber}, Number: {error.Number})", error.Message, error.LineNumber, error.Number);
                }
                throw; // Re-throw to indicate failure
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "   An unexpected error occurred during SQL insert: {ex.Message}", ex.Message);
                throw; // Re-throw to indicate failure
            }

        }

        private static DataTable ToDataTable<T>(IEnumerable<T> list)
        {
            var dataTable = new DataTable(typeof(T).Name);

            // Get all the properties of the T type
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Create columns for each property
            foreach (PropertyInfo prop in properties)
            {
                // Handle Nullable types (e.g., int?, DateTime?)
                Type columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dataTable.Columns.Add(prop.Name, columnType);
            }

            // Populate the rows
            foreach (T item in list)
            {
                DataRow row = dataTable.NewRow();
                foreach (PropertyInfo prop in properties)
                {
                    // Get the value of the property for the current object
                    object? value = prop.GetValue(item, null);

                    // Handle DBNull for null values
                    row[prop.Name] = value ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public async Task WriteToDatabaseAsync<CsvRow>(ChannelReader<string> channel, ClassMap<CsvRow> csvRowMap, int batchSize, string storedProcedureName, string tvpTypeName)
        {
            _logger.LogInformation("====== Starting to process files and write to database ======");
            int filesProcessed = 0;

            // Iterate through the channel as long as there are items and the writer hasn't completed
            await foreach (var filePath in channel.ReadAllAsync())
            {
                filesProcessed++;
                try
                {
                    await ImportCsvToSqlServerAsync(Path.GetFullPath(filePath), csvRowMap, batchSize, storedProcedureName, tvpTypeName);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Error processing file '{filePath}': {ex.Message}", filePath, ex.Message);
                    // In a real app, you might move the file to a "failed" directory for manual review
                }
            }

            _logger.LogInformation("====== Finished processing files. Total files processed: {filesProcessed}======", filesProcessed);
        }
    }
}
