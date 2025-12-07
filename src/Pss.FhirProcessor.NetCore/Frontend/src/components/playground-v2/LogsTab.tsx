import React from 'react';
import { Empty, Tag } from 'antd';

interface LogsTabProps {
  logs: any[];
}

export default function LogsTab({ logs }: LogsTabProps) {
  if (!logs || logs.length === 0) {
    return (
      <div className="p-8">
        <Empty description="No logs available" />
      </div>
    );
  }

  const getLogColor = (level: string) => {
    switch (level?.toLowerCase()) {
      case 'error':
        return 'red';
      case 'warning':
        return 'orange';
      case 'info':
        return 'blue';
      default:
        return 'default';
    }
  };

  return (
    <div className="p-4 space-y-2 overflow-auto" style={{ maxHeight: 'calc(100vh - 200px)' }}>
      {logs.map((log, idx) => (
        <div key={idx} className="bg-white p-3 rounded border border-gray-200 text-sm">
          <div className="flex items-center gap-2 mb-1">
            <Tag color={getLogColor(log.level)}>{log.level || 'INFO'}</Tag>
            {log.timestamp && (
              <span className="text-xs text-gray-500">{log.timestamp}</span>
            )}
          </div>
          <div className="font-mono text-xs">{log.message || JSON.stringify(log)}</div>
        </div>
      ))}
    </div>
  );
}
