using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Utilities
{
    /// <summary>
    /// Simple logger that collects log messages during processing
    /// </summary>
    public class Logger
    {
        private readonly List<string> _logs;
        private readonly string _logLevel;

        public Logger(string logLevel = "info")
        {
            _logs = new List<string>();
            _logLevel = logLevel?.ToLower() ?? "info";
        }

        public void Info(string message)
        {
            _logs.Add($"[INFO] {message}");
        }

        public void Debug(string message)
        {
            if (_logLevel == "debug" || _logLevel == "verbose")
            {
                _logs.Add($"[DEBUG] {message}");
            }
        }

        public void Verbose(string message)
        {
            if (_logLevel == "verbose")
            {
                _logs.Add($"[VERBOSE] {message}");
            }
        }

        public void Error(string message)
        {
            _logs.Add($"[ERROR] {message}");
        }

        public List<string> GetLogs()
        {
            return new List<string>(_logs);
        }
    }
}
