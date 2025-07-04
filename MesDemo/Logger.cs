using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using QC.QTMDotNetKernelInterface;

namespace MesDemo
{
    public static class Logger
    {
        private static string _logFolderPath;
        private static string _logFileName;
        private static readonly object _lockObject = new object();
        private static bool _isInitialized = false;
        private static string _logWindowName;

        // 初始化方法，程序启动时调用一次
        public static void Initialize(string logFolderPath, string logFileName, string logWindowName)
        {
            _logFolderPath = logFolderPath;
            _logFileName = logFileName ?? $"log_{DateTime.Now:yyyyMMdd}.txt";
            _logWindowName = logWindowName;
            _isInitialized = true;
            Directory.CreateDirectory(_logFolderPath);
        }

        public static void Log(string message, bool isLogToWindow = false)
        {
            // 检查是否已初始化
            if (!_isInitialized)
            {
                _logFolderPath = @"C:\Qualcomm\MesDemo";
                _logFileName =  $"log_{DateTime.Now:yyyyMMdd}.txt";
                _logWindowName = "Log";
                Initialize(_logFolderPath, _logFileName, _logWindowName);
            }
            try
            {
                Directory.CreateDirectory(_logFolderPath);
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

                if(isLogToWindow)//同时记录到QSPR窗口
                {
                    DebugMessage.Write(_logWindowName, logEntry, System.Diagnostics.TraceLevel.Verbose);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"日志记录失败: {ex.Message}");
            }
        }
    }
}
