import { useState, useMemo } from 'react';
import { Select, Badge, Space, Empty } from 'antd';
import { FilterOutlined } from '@ant-design/icons';
import { parseLogMessage, getLogLevelStyle, groupLogsByLevel, getLogLevelPriority } from '../utils/logHelper.jsx';

const { Option } = Select;

/**
 * LogsPanel Component
 * Displays categorized logs with color-coding and filtering
 */
function LogsPanel({ logs }) {
  const [filterLevel, setFilterLevel] = useState('all');

  // Debug: Log the first few log entries
  console.log('LogsPanel received logs:', logs?.slice(0, 5));

  // Parse and group logs
  const groupedLogs = useMemo(() => {
    if (!logs || logs.length === 0) return null;
    const grouped = groupLogsByLevel(logs);
    console.log('Grouped logs:', grouped);
    return grouped;
  }, [logs]);

  // Calculate summary counts
  const summary = useMemo(() => {
    if (!groupedLogs) return null;
    return {
      ERROR: groupedLogs.ERROR.length,
      WARN: groupedLogs.WARN.length,
      INFO: groupedLogs.INFO.length,
      DEBUG: groupedLogs.DEBUG.length,
      VERBOSE: groupedLogs.VERBOSE.length,
      total: logs.length
    };
  }, [groupedLogs, logs]);

  // Filter logs based on selected level
  const filteredLogs = useMemo(() => {
    if (!logs || logs.length === 0) return [];
    
    return logs
      .map((log, index) => ({
        ...parseLogMessage(log),
        index,
        original: log
      }))
      .filter(log => {
        if (filterLevel === 'all') return true;
        return log.level === filterLevel;
      });
  }, [logs, filterLevel]);

  if (!logs || logs.length === 0) {
    return (
      <div className="flex items-center justify-center h-64">
        <Empty description="No logs available" />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* DEBUG MARKER - Remove this after confirming component loads */}
      <div className="bg-green-500 text-white p-2 text-center font-bold">
        ✅ NEW LogsPanel Component Loaded! (Remove this debug marker)
      </div>
      
      {/* Summary Bar */}
      <div className="bg-gray-800 p-3 rounded-lg border border-gray-700">
        <div className="flex items-center justify-between flex-wrap gap-2">
          <Space size="middle">
            <span className="text-gray-400 text-sm">Total Logs: {summary.total}</span>
            {summary.ERROR > 0 && (
              <Badge 
                count={summary.ERROR} 
                style={{ backgroundColor: '#dc2626' }}
                overflowCount={999}
              >
                <span className="text-red-400 text-sm mr-2">Errors</span>
              </Badge>
            )}
            {summary.WARN > 0 && (
              <Badge 
                count={summary.WARN} 
                style={{ backgroundColor: '#ca8a04' }}
                overflowCount={999}
              >
                <span className="text-yellow-400 text-sm mr-2">Warnings</span>
              </Badge>
            )}
            {summary.INFO > 0 && (
              <Badge 
                count={summary.INFO} 
                style={{ backgroundColor: '#2563eb' }}
                overflowCount={999}
              >
                <span className="text-blue-400 text-sm mr-2">Info</span>
              </Badge>
            )}
            {summary.DEBUG > 0 && (
              <Badge 
                count={summary.DEBUG} 
                style={{ backgroundColor: '#6b7280' }}
                overflowCount={999}
              >
                <span className="text-gray-400 text-sm mr-2">Debug</span>
              </Badge>
            )}
            {summary.VERBOSE > 0 && (
              <Badge 
                count={summary.VERBOSE} 
                style={{ backgroundColor: '#9333ea' }}
                overflowCount={999}
              >
                <span className="text-purple-400 text-sm mr-2">Verbose</span>
              </Badge>
            )}
          </Space>
          
          {/* Filter Dropdown */}
          <Select
            value={filterLevel}
            onChange={setFilterLevel}
            style={{ width: 150 }}
            size="small"
            suffixIcon={<FilterOutlined />}
          >
            <Option value="all">All Levels</Option>
            <Option value="ERROR">Errors Only</Option>
            <Option value="WARN">Warnings Only</Option>
            <Option value="INFO">Info Only</Option>
            <Option value="DEBUG">Debug Only</Option>
            <Option value="VERBOSE">Verbose Only</Option>
          </Select>
        </div>
      </div>

      {/* Logs Display */}
      <div className="bg-gray-900 rounded-lg border border-gray-700 max-h-[500px] overflow-auto">
        <div className="divide-y divide-gray-800">
          {filteredLogs.length > 0 ? (
            filteredLogs.map((log, idx) => {
              const style = getLogLevelStyle(log.level);
              
              return (
                <div
                  key={log.index}
                  className={`p-3 hover:bg-gray-800/50 transition-colors ${style.bgColor} border-l-4 ${style.borderColor}`}
                >
                  <div className="flex items-start gap-3 font-mono text-sm">
                    {/* Icon */}
                    <span className="text-lg flex-shrink-0">{style.icon}</span>
                    
                    {/* Badge */}
                    <span 
                      className={`${style.badge} text-white text-xs font-bold px-2 py-0.5 rounded flex-shrink-0 mt-0.5`}
                    >
                      {style.name}
                    </span>
                    
                    {/* Message */}
                    <span className={`${style.color} flex-1 break-words`}>
                      {log.message}
                    </span>
                    
                    {/* Index */}
                    <span className="text-gray-600 text-xs flex-shrink-0 mt-0.5">
                      #{log.index + 1}
                    </span>
                  </div>
                </div>
              );
            })
          ) : (
            <div className="p-8 text-center text-gray-500">
              No logs match the selected filter
            </div>
          )}
        </div>
      </div>

      {/* Filter Info */}
      {filterLevel !== 'all' && (
        <div className="text-sm text-gray-500 text-center">
          Showing {filteredLogs.length} of {summary.total} logs
        </div>
      )}
    </div>
  );
}

export default LogsPanel;
