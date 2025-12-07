import React from 'react';
import { Empty } from 'antd';

interface ExtractionTabProps {
  extraction: any;
}

export default function ExtractionTab({ extraction }: ExtractionTabProps) {
  if (!extraction) {
    return (
      <div className="p-8">
        <Empty description="No extraction results yet" />
      </div>
    );
  }

  return (
    <div className="p-4 overflow-auto" style={{ maxHeight: 'calc(100vh - 200px)' }}>
      <pre className="bg-white p-4 rounded border border-gray-300 text-sm">
        {JSON.stringify(extraction, null, 2)}
      </pre>
    </div>
  );
}
