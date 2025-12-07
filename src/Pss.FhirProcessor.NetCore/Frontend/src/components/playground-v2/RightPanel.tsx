import React, { useState } from 'react';
import { Button, Select, Switch, Tabs, Spin } from 'antd';
import { 
  PlayCircleOutlined
} from '@ant-design/icons';
import ValidationTab from './ValidationTab';
import ExtractionTab from './ExtractionTab';
import BundleTab from './BundleTab';
import LogsTab from './LogsTab';

const { Option } = Select;

interface RightPanelProps {
  loading: boolean;
  result: any;
  logLevel: string;
  strictDisplay: boolean;
  onProcess: () => void;
  onClear: () => void;
  onLoadSample: (sampleName: string) => void;
  onLogLevelChange: (level: string) => void;
  onStrictDisplayChange: (checked: boolean) => void;
  onGoToResource: (resourcePointer: any) => void;
  hasJson: boolean;
}

export default function RightPanel({
  loading,
  result,
  logLevel,
  strictDisplay,
  onProcess,
  onClear,
  onLoadSample,
  onLogLevelChange,
  onStrictDisplayChange,
  onGoToResource,
  hasJson
}: RightPanelProps) {
  const [activeTab, setActiveTab] = useState('validation');

  // Debug logging
  console.log('RightPanel render:', {
    loading,
    hasResult: !!result,
    resultKeys: result ? Object.keys(result) : [],
    validationErrors: result?.validation?.errors?.length || 0
  });

  const tabItems = [
    {
      key: 'validation',
      label: `Validation ${result?.validation?.errors?.length ? `(${result.validation.errors.length})` : ''}`,
      children: (
        <ValidationTab 
          validation={result?.validation}
          onGoToResource={onGoToResource}
        />
      )
    },
    {
      key: 'extraction',
      label: 'Extraction',
      children: <ExtractionTab extraction={result?.extraction} />
    },
    {
      key: 'bundle',
      label: 'Original Bundle',
      children: <BundleTab bundle={result?.originalBundle} />
    },
    {
      key: 'logs',
      label: `Logs ${result?.logs?.length ? `(${result.logs.length})` : ''}`,
      children: <LogsTab logs={result?.logs} />
    }
  ];

  return (
    <div className="flex flex-col h-full">
      {/* Controls Bar */}
      <div className="bg-white border-b border-gray-200 p-3">
        <div className="flex items-center gap-3 flex-wrap">
          {/* Process Button */}
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={onProcess}
            loading={loading}
            disabled={!hasJson}
          >
            Process
          </Button>

          {/* Log Level */}
          <Select
            value={logLevel}
            onChange={onLogLevelChange}
            style={{ width: 120 }}
            disabled={loading}
          >
            <Option value="verbose">Verbose</Option>
            <Option value="info">Info</Option>
            <Option value="warning">Warning</Option>
            <Option value="error">Error</Option>
          </Select>

          {/* Strict Display Match */}
          <div className="flex items-center gap-2">
            <span className="text-sm text-gray-600">Strict Display:</span>
            <Switch
              checked={strictDisplay}
              onChange={onStrictDisplayChange}
              disabled={loading}
            />
          </div>
        </div>
      </div>

      {/* Results Tabs */}
      <div className="flex-1 overflow-hidden bg-gray-50">
        {loading ? (
          <div className="h-full flex items-center justify-center">
            <Spin size="large" tip="Processing FHIR data..." />
          </div>
        ) : result ? (
          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            items={tabItems}
            className="h-full"
            style={{ padding: '0 16px' }}
          />
        ) : (
          <div className="h-full flex items-center justify-center">
            <div className="text-center text-gray-400">
              <PlayCircleOutlined style={{ fontSize: 48 }} />
              <p className="mt-4">Click "Process" to validate and extract FHIR data</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
