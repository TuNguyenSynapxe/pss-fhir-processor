using System.Collections.Generic;

namespace MOH.HealthierSG.PSS.FhirProcessor.Utilities
{
    /// <summary>
    /// Simple logger that collects log messages during processing
    /// Log levels (from least to most verbose): error, warn, info, debug, verbose
    /// </summary>
    public class Logger
    {
        private readonly List<string> _logs;
        private readonly string _logLevel;
        private readonly int _logLevelValue;

        // Log level hierarchy: error=1, warn=2, info=3, debug=4, verbose=5
        private static readonly Dictionary<string, int> LogLevels = new Dictionary<string, int>
        {
            { "error", 1 },
            { "warn", 2 },
            { "info", 3 },
            { "debug", 4 },
            { "verbose", 5 }
        };

        public Logger(string logLevel = "info")
        {
            _logs = new List<string>();
            _logLevel = logLevel?.ToLower() ?? "info";
            _logLevelValue = LogLevels.ContainsKey(_logLevel) ? LogLevels[_logLevel] : 3; // default to info
        }

        private bool ShouldLog(int messageLevel)
        {
            return messageLevel <= _logLevelValue;
        }

        public void Error(string message)
        {
            if (ShouldLog(1))
            {
                _logs.Add($"[ERROR] {message}");
            }
        }

        public void Warn(string message)
        {
            if (ShouldLog(2))
            {
                _logs.Add($"[WARN] {message}");
            }
        }

        public void Info(string message)
        {
            if (ShouldLog(3))
            {
                _logs.Add($"[INFO] {message}");
            }
        }

        public void Debug(string message)
        {
            if (ShouldLog(4))
            {
                _logs.Add($"[DEBUG] {message}");
            }
        }

        public void Verbose(string message)
        {
            if (ShouldLog(5))
            {
                _logs.Add($"[VERBOSE] {message}");
            }
        }

        public List<string> GetLogs()
        {
            return new List<string>(_logs);
        }
    }
}
