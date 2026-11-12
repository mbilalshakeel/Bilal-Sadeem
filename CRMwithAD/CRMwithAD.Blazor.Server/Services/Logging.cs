namespace CRMwithAD.Blazor.Server.Services
{
    public class Logging
    {
        private static readonly string LogDirectory = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Logs")["ErrorLogDirectory"]!;

        public static void LogError(Exception ex, string source)
        {
            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);

                // Create a log file with a timestamp
                string logFileName = $"CRMwithADErrorLogFile-{DateTime.Today:yyyy-MM-dd}.txt";
                string logFilePath = Path.Combine(LogDirectory, logFileName);

                // Prepare the error message
                string message = $"---------------------------------------{Environment.NewLine}";
                message += $"Timestamp  : {DateTime.Now}{Environment.NewLine}";
                message += $"Source     : {source}{Environment.NewLine}";
                message += $"Message    : {ex.Message}{Environment.NewLine}";
                message += $"StackTrace : {ex.StackTrace}{Environment.NewLine}";

                if (ex.InnerException != null)
                {
                    message += $"Inner Exception: {ex.InnerException.Message}{Environment.NewLine}";
                    message += $"Inner StackTrace: {ex.InnerException.StackTrace}{Environment.NewLine}";
                }

                message += "---------------------------------------" + Environment.NewLine;

                // Write the log to a text file (Append if file already exists)
                File.AppendAllText(logFilePath, message);
                //LogExceptionToEventViewer(ex, source);
            }

            catch (Exception logEx)
            {
                // If logging fails, write to a fallback file in the Temp directory
                string fallbackFilePath = Path.Combine(Path.GetTempPath(), "Fallback-ErrorLog.txt");
                File.AppendAllText(fallbackFilePath, $"Logging failed: {logEx.Message}{Environment.NewLine}");
            }
        }
    }
}