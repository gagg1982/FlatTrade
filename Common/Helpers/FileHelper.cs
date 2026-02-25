namespace Common.Helpers
{
    public class FileHelper
    {
        public static bool CreateDirectory(string filePath)
        {
            try
            {
                string? directoryPath = Path.GetDirectoryName(filePath);
                if (directoryPath != null && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static string GetConfigFile(string filePath, string defaultFile)
        {
            string configFileName = filePath.Length > 0 && !string.IsNullOrWhiteSpace(filePath)
            ? filePath
            : string.IsNullOrEmpty(defaultFile) ? "AppConfig.json": defaultFile;

            // Check if the specified file exists before trying to load it.
            // We combine the file name with the application's base directory.
            string configFilePath = Path.Combine(AppContext.BaseDirectory, configFileName);

            if (!File.Exists(configFilePath))
            {
                throw new ArgumentException("Error: The configuration file '{0}' was not found.", configFilePath);
            }
            return configFileName;
        }
    }
}
