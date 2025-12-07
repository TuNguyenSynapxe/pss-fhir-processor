import React, { useState, useMemo } from 'react';
import { Empty, Tag, Select, Badge, Space } from 'antd';
import { FilterOutlined } from '@ant-design/icons';

interface LogsTabProps {
  logs: any[];
}

// Parse log message to extract level from format: "[LEVEL] message"
function parseLogMessage(logString: string) {
  if (typeof logString !== 'string') {
    return { level: 'INFO', message: JSON.stringify(logString) };
  }
  
  const match = logString.match(/^\[(\w+)\]\s*(.*)$/);
  if (match) {
    return {
      level: match[1].toUpperCase(),
      message: match[2]
    };
  }
  
  return { level: 'INFO', message: logString };
}

// Get styling for log level
function getLogLevelStyle(level: string) {
  const styles: Record<string, any> = {
    ERROR: {
      color: 'text-red-600',
      bgColor: 'bg-red-50',
      borderColor: 'border-l-red-500',
      tagColor: 'red',
      icon: '❌'
    },
    WARN: {
      color: 'text-yellow-600',
      bgColor: 'bg-yellow-50',
      borderColor: 'border-l-yellow-500',
      tagColor: 'orange',
      icon: '⚠️'
    },
    INFO: {
      color: 'text-blue-600',
      bgColor: 'bg-blue-50',
      borderColor: 'border-l-blue-500',
      tagColor: 'blue',
      icon: 'ℹ️'
    },
    DEBUG: {
      color: 'text-gray-600',
      bgColor: 'bg-gray-50',
      borderColor: 'border-l-gray-500',
      tagColor: 'default',
      icon: '🔍'
    },
    VERBOSE: {
      color: 'text-purple-600',
      bgColor: 'bg-purple-50',
      borderColor: 'border-l-purple-500',
      tagColor: 'purple',
      icon: '📝'
    }
  };
  
  return styles[level] || styles.INFO;
}

export default function LogsTab({ logs }: LogsTabProps) {
  const [filterLevel, setFilterLevel] = useState<string>('all');

  // Parse all logs
  const parsedLogs = useMemo(() => {
    if (!logs || logs.length === 0) return [];
    return logs.map((log, index) => ({
      ...parseLogMessage(log),
      index,
      original: log
    }));
  }, [logs]);

  // Calculate summary counts
  const summary = useMemo(() => {
    const counts = { ERROR: 0, WARN: 0, INFO: 0, DEBUG: 0, VERBOSE: 0 };
    parsedLogs.forEach(log => {
      if (counts.hasOwnProperty(log.level)) {
        counts[log.level as keyof typeof counts]++;
      }
    });
    return counts;
  }, [parsedLogs]);

  // Filter logs
  const filteredLogs = useMemo(() => {
    if (filterLevel === 'all') return parsedLogs;
    return parsedLogs.filter(log => log.level === filterLevel);
  }, [parsedLogs, filterLevel]);

  if (!logs || logs.length === 0) {
    return (
      <div className="p-8">
        <Empty description="No logs available" />
      </div>
    );
  }

  return (
    <div className="p-4 space-y-4">
      {/* Summary Bar */}
      <div className="bg-gray-100 p-3 rounded-lg border border-gray-300">
        <div className="flex items-center justify-between flex-wrap gap-2">
          <Space size="middle">
            <span className="text-sm text-gray-600 font-medium">Total: {parsedLogs.length}</span>
            {summary.ERROR > 0 && (
              <Badge count={summary.ERROR} style={{ backgroundColor: '#dc2626' }}>
                <span className="text-sm mr-2">Errors</span>
              </Badge>
            )}
            {summary.WARN > 0 && (
              <Badge count={summary.WARN} style={{ backgroundColor: '#ea580c' }}>
                <span className="text-sm mr-2">Warnings</span>
              </Badge>
            )}
            {summary.INFO > 0 && (
              <Badge count={summary.INFO} style={{ backgroundColor: '#2563eb' }}>
                <span className="text-sm mr-2">Info</span>
              </Badge>
            )}
            {summary.DEBUG > 0 && (
              <Badge count={summary.DEBUG} style={{ backgroundColor: '#6b7280' }}>
                <span className="text-sm mr-2">Debug</span>
              </Badge>
            )}
            {summary.VERBOSE > 0 && (
              <Badge count={summary.VERBOSE} style={{ backgroundColor: '#9333ea' }}>
                <span className="text-sm mr-2">Verbose</span>
              </Badge>
            )}
          </Space>

          <Select
            value={filterLevel}
            onChange={setFilterLevel}
            style={{ width: 150 }}
            size="small"
            suffixIcon={<FilterOutlined />}
          >
            <Select.Option value="all">All Levels</Select.Option>
            <Select.Option value="ERROR">Errors Only</Select.Option>
            <Select.Option value="WARN">Warnings Only</Select.Option>
            <Select.Option value="INFO">Info Only</Select.Option>
            <Select.Option value="DEBUG">Debug Only</Select.Option>
            <Select.Option value="VERBOSE">Verbose Only</Select.Option>
          </Select>
        </div>
      </div>

      {/* Logs Display */}
      <div className="space-y-2 overflow-auto" style={{ maxHeight: 'calc(100vh - 300px)' }}>
        {filteredLogs.length > 0 ? (
          filteredLogs.map((log) => {
            const style = getLogLevelStyle(log.level);
            return (
              <div
                key={log.index}
                className={`p-3 rounded border-l-4 ${style.bgColor} ${style.borderColor} border border-gray-200 hover:shadow-sm transition-shadow`}
              >
                <div className="flex items-start gap-3">
                  <span className="text-lg flex-shrink-0">{style.icon}</span>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <Tag color={style.tagColor} className="font-semibold">
                        {log.level}
                      </Tag>
                      <span className="text-xs text-gray-500">#{log.index + 1}</span>
                    </div>
                    <div className={`font-mono text-xs ${style.color} whitespace-pre-wrap break-words`}>
                      {log.message}
                    </div>
                  </div>
                </div>
              </div>
            );
          })
        ) : (
          <div className="text-center text-gray-500 py-8">
            No logs match the selected filter
          </div>
        )}
      </div>

      {/* Filter Info */}
      {filterLevel !== 'all' && (
        <div className="text-sm text-gray-500 text-center">
          Showing {filteredLogs.length} of {parsedLogs.length} logs
        </div>
      )}
    </div>
  );
}
