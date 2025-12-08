import React, { useState } from 'react';
import { Alert, Empty, Button, Badge, Collapse, Space } from 'antd';
import { CheckCircleOutlined, DownloadOutlined, ExpandOutlined, CompressOutlined } from '@ant-design/icons';
import { ErrorHelperPanel } from './ErrorHelperPanel';

interface ValidationTabProps {
  validation: any;
  onGoToResource: (resourcePointer: any) => void;
  jsonTree?: any;
}

export default function ValidationTab({ validation, onGoToResource, jsonTree }: ValidationTabProps) {
  const [expandedKeys, setExpandedKeys] = useState<string[]>([]);

  if (!validation) {
    return (
      <div className="p-8">
        <Empty description="No validation results yet" />
      </div>
    );
  }

  if (validation.isValid && (!validation.errors || validation.errors.length === 0)) {
    return (
      <div className="p-8">
        <Alert
          message="Validation Successful"
          description="All validation rules passed successfully!"
          type="success"
          icon={<CheckCircleOutlined />}
          showIcon
        />
      </div>
    );
  }

  const errors = validation.errors || [];

  // Group errors by type (priority order - first match wins)
  const categorized = new Set<any>();
  
  const errorGroups = {
    missing: errors.filter((e: any) => {
      if (categorized.has(e)) return false;
      if (e.code?.includes('MISSING') || e.ruleType === 'Required') {
        categorized.add(e);
        return true;
      }
      return false;
    }),
    valueMismatch: errors.filter((e: any) => {
      if (categorized.has(e)) return false;
      if (e.code?.includes('MISMATCH') || e.code?.includes('INVALID') || 
          e.ruleType === 'FixedValue' || e.ruleType === 'FixedCoding' || e.ruleType === 'CodesMaster') {
        categorized.add(e);
        return true;
      }
      return false;
    }),
    format: errors.filter((e: any) => {
      if (categorized.has(e)) return false;
      if (e.code?.includes('REGEX') || e.code?.includes('TYPE') || 
          e.ruleType === 'Regex' || e.ruleType === 'Type') {
        categorized.add(e);
        return true;
      }
      return false;
    }),
    reference: errors.filter((e: any) => {
      if (categorized.has(e)) return false;
      if (e.code?.includes('REF_') || e.ruleType === 'Reference') {
        categorized.add(e);
        return true;
      }
      return false;
    }),
  };

  // Remaining errors not in above categories
  const otherErrors = errors.filter((e: any) => !categorized.has(e));

  const groupedErrors = [
    { key: 'missing', title: 'Missing Required Fields', errors: errorGroups.missing, color: 'red' },
    { key: 'valueMismatch', title: 'Value Mismatches', errors: errorGroups.valueMismatch, color: 'orange' },
    { key: 'format', title: 'Format Errors', errors: errorGroups.format, color: 'purple' },
    { key: 'reference', title: 'Reference Errors', errors: errorGroups.reference, color: 'blue' },
    { key: 'other', title: 'Other Errors', errors: otherErrors, color: 'gray' },
  ].filter(group => group.errors.length > 0);

  const handleExpandAll = () => {
    const allKeys = groupedErrors.flatMap((group, groupIdx) => 
      group.errors.map((_, errorIdx) => `${groupIdx}-${errorIdx}`)
    );
    setExpandedKeys(allKeys);
  };

  const handleCollapseAll = () => {
    setExpandedKeys([]);
  };

  const handleExportErrors = () => {
    const errorReport = {
      timestamp: new Date().toISOString(),
      totalErrors: errors.length,
      errors: errors.map((e: any) => ({
        code: e.code,
        message: e.message,
        fieldPath: e.fieldPath,
        scope: e.scope,
        ruleType: e.ruleType,
        resourcePointer: e.resourcePointer,
      })),
    };
    const blob = new Blob([JSON.stringify(errorReport, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `validation-errors-${Date.now()}.json`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="p-4 space-y-3 overflow-auto" style={{ maxHeight: 'calc(100vh - 200px)' }}>
      <div className="mb-4 space-y-3">
        <Alert
          message={`Found ${errors.length} validation error(s)`}
          type="warning"
          showIcon
        />
        
        <Space>
          <Button size="small" icon={<ExpandOutlined />} onClick={handleExpandAll}>
            Expand All
          </Button>
          <Button size="small" icon={<CompressOutlined />} onClick={handleCollapseAll}>
            Collapse All
          </Button>
          <Button size="small" icon={<DownloadOutlined />} onClick={handleExportErrors}>
            Export Errors
          </Button>
        </Space>
      </div>

      {groupedErrors.map((group, groupIdx) => (
        <div key={group.key} className="mb-4">
          <div className="flex items-center gap-2 mb-2">
            <h3 className="text-base font-semibold">{group.title}</h3>
            <Badge count={group.errors.length} style={{ backgroundColor: group.color }} />
          </div>
          
          <Collapse
            activeKey={expandedKeys}
            onChange={(keys) => {
              setExpandedKeys(keys as string[]);
            }}
            items={group.errors.map((error: any, errorIdx: number) => ({
              key: `${groupIdx}-${errorIdx}`,
              label: (
                <div className="flex items-start justify-between w-full">
                  <div className="flex-1">
                    <span className="font-mono text-xs text-red-600">[{error.code}]</span>
                    <span className="ml-2 text-sm">{error.message}</span>
                  </div>
                </div>
              ),
              children: (
                <ErrorHelperPanel
                  error={error}
                  json={jsonTree}
                  onJumpToLocation={(entryIndex, path) => {
                    onGoToResource({
                      entryIndex,
                      fieldPath: path,
                      resourceType: error.resourcePointer?.resourceType,
                      resourceId: error.resourcePointer?.resourceId,
                      fullUrl: error.resourcePointer?.fullUrl,
                    });
                  }}
                />
              ),
            }))}
          />
        </div>
      ))}
    </div>
  );
}
