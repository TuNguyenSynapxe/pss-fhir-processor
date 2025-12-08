import React from 'react';
import { Empty } from 'antd';

interface BundleTabProps {
  bundle: any;
}

export default function BundleTab({ bundle }: BundleTabProps) {
  if (!bundle) {
    return (
      <div className="p-8">
        <Empty description="No bundle data available" />
      </div>
    );
  }

  return (
    <div className="p-4 overflow-auto" style={{ maxHeight: 'calc(100vh - 200px)' }}>
      <pre className="bg-white p-4 rounded border border-gray-300 text-sm">
        {JSON.stringify(bundle, null, 2)}
      </pre>
    </div>
  );
}
