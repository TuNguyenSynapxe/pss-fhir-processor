/**
 * Log Helper Utilities
 * Parse and style log messages from backend
 */

/**
 * Parse a log message to extract level and content
 * Example: "[ERROR] Something went wrong" => { level: "ERROR", message: "Something went wrong" }
 */
export function parseLogMessage(logString) {
  const match = logString.match(/^\[(\w+)\]\s*(.*)$/);
  
  if (match) {
    return {
      level: match[1].toUpperCase(),
      message: match[2]
    };
  }
  
  // No level prefix found - treat as INFO
  return {
    level: 'INFO',
    message: logString
  };
}

/**
 * Get styling information for a log level
 */
export function getLogLevelStyle(level) {
  const styles = {
    ERROR: {
      color: 'text-red-400',
      bgColor: 'bg-red-900/20',
      borderColor: 'border-red-500/50',
      badge: 'bg-red-600',
      icon: '❌',
      name: 'ERROR'
    },
    WARN: {
      color: 'text-yellow-400',
      bgColor: 'bg-yellow-900/20',
      borderColor: 'border-yellow-500/50',
      badge: 'bg-yellow-600',
      icon: '⚠️',
      name: 'WARN'
    },
    INFO: {
      color: 'text-blue-400',
      bgColor: 'bg-blue-900/20',
      borderColor: 'border-blue-500/50',
      badge: 'bg-blue-600',
      icon: 'ℹ️',
      name: 'INFO'
    },
    DEBUG: {
      color: 'text-gray-400',
      bgColor: 'bg-gray-900/20',
      borderColor: 'border-gray-500/50',
      badge: 'bg-gray-600',
      icon: '🔍',
      name: 'DEBUG'
    },
    VERBOSE: {
      color: 'text-purple-400',
      bgColor: 'bg-purple-900/20',
      borderColor: 'border-purple-500/50',
      badge: 'bg-purple-600',
      icon: '📝',
      name: 'VERBOSE'
    }
  };
  
  return styles[level] || styles.INFO;
}

/**
 * Group logs by level for summary
 */
export function groupLogsByLevel(logs) {
  const grouped = {
    ERROR: [],
    WARN: [],
    INFO: [],
    DEBUG: [],
    VERBOSE: []
  };
  
  logs.forEach((log, index) => {
    const parsed = parseLogMessage(log);
    grouped[parsed.level].push({
      ...parsed,
      index,
      original: log
    });
  });
  
  return grouped;
}

/**
 * Get log level priority (for filtering)
 */
export function getLogLevelPriority(level) {
  const priorities = {
    ERROR: 1,
    WARN: 2,
    INFO: 3,
    DEBUG: 4,
    VERBOSE: 5
  };
  
  return priorities[level] || 999;
}
