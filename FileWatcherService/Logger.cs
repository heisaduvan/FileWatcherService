using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcherService
{
    public static class Logger
    {
        private const string LogFile = @"C:/Projects/Log.txt";
        private const string SUCCESS = "SUCCESS --- ";
        private const string ERROR = "ERROR --- ";

        public static void SuccessLog(string message)
        {
            File.AppendAllText(LogFile, Environment.NewLine + SUCCESS + message);
        }
        public static void ErrorLog(string message)
        {
            File.AppendAllText(LogFile, Environment.NewLine + ERROR + message);
        }
    }
}
