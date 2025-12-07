import React from 'react';
import { Alert, Empty } from 'antd';
import { CheckCircleOutlined } from '@ant-design/icons';
import ValidationErrorCard from './ValidationErrorCard';

interface ValidationTabProps {
  validation: any;
  onGoToResource: (resourcePointer: any) => void;
}

export default function ValidationTab({ validation, onGoToResource }: ValidationTabProps) {
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

  return (
    <div className="p-4 space-y-3 overflow-auto" style={{ maxHeight: 'calc(100vh - 200px)' }}>
      <div className="mb-4">
        <Alert
          message={`Found ${validation.errors?.length || 0} validation error(s)`}
          type="warning"
          showIcon
        />
      </div>
      
      {validation.errors?.map((error: any, idx: number) => (
        <ValidationErrorCard
          key={idx}
          error={error}
          onGoToResource={onGoToResource}
        />
      ))}
    </div>
  );
}
