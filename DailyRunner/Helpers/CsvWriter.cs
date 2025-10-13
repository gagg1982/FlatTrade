using FlatTrade.Common.Helpers;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace DailyRunner.Helpers
{
    public class CsvWriter
    {
        private readonly ILogger<CsvWriter> _logger;
        private readonly Channel<CsvChannelObject> _csvFileChannel;
        private readonly Task _task;
        private readonly string _baseDir;
        private readonly string _writerFriendlyName;
        public CsvWriter(string baseDir, int fileChannelCapacity, string writerFriendlyName,  ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CsvWriter>();
            _baseDir = Path.GetFullPath(baseDir);
            CreateDirectoryPath(baseDir);
            _writerFriendlyName = writerFriendlyName;
            _csvFileChannel = Utility.CreateBoundedChannel<CsvChannelObject>(fileChannelCapacity);
            _task = WriteCsvAsync();
        }

        private void CreateDirectoryPath(string filePath)
        {
            if (!FileHelper.CreateDirectory(filePath))
            {
                var msg = $"Failed to create directory: {filePath}";
                _logger.LogCritical("{msg}", msg);
                throw new Exception($"{msg}");
            }
        }

        public async Task WriteCsvAsync(CsvChannelObject obj) => await _csvFileChannel.Writer.WriteAsync(obj);

        public Task WriteComplete()
        {
            _csvFileChannel.Writer.Complete();
            return _task;
        }

        private async Task WriteCsvAsync()
        {
            _logger.LogInformation("====== [{_writerFriendlyName}] Starting to process CsvRecords ======", _writerFriendlyName);
            int objectsProcessed = 0;
            int objectsfailed = 0;
            // Iterate through the channel as long as there are items and the writer hasn't completed

            await foreach (var reader in _csvFileChannel.Reader.ReadAllAsync())
            {
                var pureFileName = Path.GetFileName(reader.FileName);
                var isPureFile = pureFileName == reader.FileName;
                var isRootedFile = Path.IsPathRooted(reader.FileName);

                var file = Path.Combine(_baseDir, isRootedFile || isPureFile ? pureFileName : reader.FileName);
                CreateDirectoryPath(file);
                try
                {
                    using var writer = new StreamWriter(file, false);
                    await writer.WriteLineAsync(reader.Header);
                    foreach (var record in reader.Records)
                        await writer.WriteLineAsync(record);
                    objectsProcessed++;
                    _logger.LogDebug("[{_writerFriendlyName}] Success: File '{file}' Records: {recordCount}.", _writerFriendlyName, file, reader.Records.Count());
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "[_writerFriendlyName{}] Error: processing file '{file}': {ex.Message}", _writerFriendlyName, file, ex.Message);
                    objectsfailed++;
                }
                finally
                {
                    if((objectsProcessed + objectsfailed) % 500 == 0)
                        _logger.LogInformation("[{_writerFriendlyName}] Total: {objectsProcessed}, Success: {objectsProcessed}{objectsfailed}", _writerFriendlyName, objectsProcessed + objectsfailed, objectsProcessed, objectsfailed > 0? ", Failed: "+ objectsfailed:string.Empty );
                }
            }
            _logger.LogInformation("====== [{_writerFriendlyName}] Finished processing CsvRecords. Total: {total}, Success: {processed}, Failed: {failed} ======",
                _writerFriendlyName, objectsProcessed + objectsfailed, objectsProcessed, objectsfailed);
        }
    }
}
