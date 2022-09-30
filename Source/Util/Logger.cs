using System;
using System.Text;
using System.IO;

namespace GateFailoverService.Source.Util
{
    class Logger
    {
        private string logFileName;
        public Logger(string logFileName = "C:\\GFS\\GateFailoverService.log") // Hardcode!
        {
            this.logFileName = logFileName;
            if (!File.Exists(logFileName))
                File.CreateText(logFileName);
        }
        public void Write(string LogEntry)
        {
            string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] {1}\r\n", DateTime.Now, LogEntry);
            File.AppendAllText(this.logFileName, fullText, Encoding.UTF8);
        }
    }
}
