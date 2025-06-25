using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace UploadData
{
    class Logger
    {
        private readonly string _logFolderPath;
        private readonly string _logFileName;
        private readonly object _lockObject = new object();

        public Logger(string logFolderPath, string logFileName)
        {
            _logFolderPath = logFolderPath;
            _logFileName = logFileName ?? $"log_{DateTime.Now:yyyyMMdd}.txt";

            // 确保日志文件夹存在
            Directory.CreateDirectory(_logFolderPath);
        }

        public void Log(string message)
        {
            try
            {
                var logFilePath = Path.Combine(_logFolderPath, _logFileName);
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]\n{message}\n";

                // 使用传统using语句替代using声明
                lock (_lockObject)
                {
                    using (var writer = File.AppendText(logFilePath))
                    {
                        writer.WriteLine(logEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"日志记录失败: {ex.Message}");
            }
        }
    }
}
