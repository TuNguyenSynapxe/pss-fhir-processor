import React, { useState, useEffect, useRef } from 'react';
import { Modal, Button, message, Space, Alert, Select } from 'antd';
import { 
  FormatPainterOutlined, 
  CheckCircleOutlined, 
  ReloadOutlined, 
  CloseOutlined,
  FolderOpenOutlined
} from '@ant-design/icons';

const { Option } = Select;
import Editor from '@monaco-editor/react';

interface JsonEditorModalProps {
  open: boolean;
  initialValue: string;
  onClose: () => void;
  onApply: (value: string) => void;
  onLoadSample?: (sampleName: string) => void;
}

export default function JsonEditorModal({ open, initialValue, onClose, onApply, onLoadSample }: JsonEditorModalProps) {
  const [editorValue, setEditorValue] = useState('');
  const [initialSnapshot, setInitialSnapshot] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);
  const [sampleFiles, setSampleFiles] = useState<string[]>([]);
  const editorRef = useRef<any>(null);

  // Load sample file names
  useEffect(() => {
    const loadSampleFiles = async () => {
      try {
        const modules = import.meta.glob('/src/seed/happy-*.json') as Record<string, any>;
        const files = Object.keys(modules).map(path => {
          const fileName = path.split('/').pop() || '';
          return fileName;
        });
        setSampleFiles(files);
      } catch (error) {
        console.error('Failed to load sample files:', error);
      }
    };
    loadSampleFiles();
  }, []);

  // Initialize editor value when modal opens
  useEffect(() => {
    if (open && initialValue) {
      const formatted = formatJson(initialValue);
      setEditorValue(formatted);
      setInitialSnapshot(formatted);
      setValidationError(null);
    }
  }, [open, initialValue]);

  const handleEditorDidMount = (editor: any) => {
    editorRef.current = editor;
  };

  const handleEditorChange = (value: string | undefined) => {
    setEditorValue(value || '');
    if (validationError) {
      setValidationError(null);
    }
  };

  // Format JSON
  const formatJson = (jsonString: string): string => {
    try {
      const parsed = JSON.parse(jsonString);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return jsonString;
    }
  };

  // Extract error position
  const extractErrorPosition = (errorMessage: string) => {
    const positionMatch = errorMessage.match(/position (\d+)/);
    if (positionMatch && editorValue) {
      const position = parseInt(positionMatch[1]);
      const lines = editorValue.substring(0, position).split('\n');
      const line = lines.length;
      const column = lines[lines.length - 1].length + 1;
      return { line, column };
    }
    return null;
  };

  // Format JSON button
  const handleFormatJson = () => {
    if (!editorValue.trim()) {
      message.warning('Editor is empty');
      return;
    }

    try {
      const parsed = JSON.parse(editorValue);
      const formatted = JSON.stringify(parsed, null, 2);
      setEditorValue(formatted);
      setValidationError(null);
      message.success('JSON formatted successfully');
    } catch (error: any) {
      const position = extractErrorPosition(error.message);
      const errorMsg = position 
        ? `Invalid JSON at line ${position.line}, column ${position.column}: ${error.message}`
        : `Invalid JSON: ${error.message}`;
      setValidationError(errorMsg);
      message.error('Cannot format invalid JSON');
    }
  };

  // Validate JSON button
  const handleValidateJson = () => {
    if (!editorValue.trim()) {
      message.warning('Editor is empty');
      return;
    }

    try {
      JSON.parse(editorValue);
      setValidationError(null);
      message.success('✓ JSON is valid');
    } catch (error: any) {
      const position = extractErrorPosition(error.message);
      const errorMsg = position 
        ? `Line ${position.line}, Column ${position.column}: ${error.message}`
        : error.message;
      setValidationError(errorMsg);
      
      if (position && editorRef.current) {
        editorRef.current.setPosition({ lineNumber: position.line, column: position.column });
        editorRef.current.revealLineInCenter(position.line);
        editorRef.current.focus();
      }
    }
  };

  // Reset button
  const handleReset = () => {
    setEditorValue(initialSnapshot);
    setValidationError(null);
    message.info('Editor reset to initial state');
  };

  // Apply Changes button
  const handleApply = () => {
    if (!editorValue.trim()) {
      message.warning('Editor is empty');
      return;
    }

    try {
      const parsed = JSON.parse(editorValue);
      const formatted = JSON.stringify(parsed, null, 2);
      onApply(formatted);
      message.success('JSON updated successfully');
      setValidationError(null);
      onClose();
    } catch (error: any) {
      const position = extractErrorPosition(error.message);
      const errorMsg = position 
        ? `Line ${position.line}, Column ${position.column}: ${error.message}`
        : error.message;
      setValidationError(errorMsg);
      message.error('Cannot apply invalid JSON');
      
      if (position && editorRef.current) {
        editorRef.current.setPosition({ lineNumber: position.line, column: position.column });
        editorRef.current.revealLineInCenter(position.line);
        editorRef.current.focus();
      }
    }
  };

  const handleCancel = () => {
    setValidationError(null);
    onClose();
  };

  return (
    <Modal
      title={
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', paddingRight: '20px', gap: '12px' }}>
          <span>Edit FHIR JSON Input</span>
          <Space>
            {onLoadSample && (
              <Select
                placeholder="Load Sample"
                style={{ width: 200 }}
                onChange={onLoadSample}
                allowClear
                size="small"
                suffixIcon={<FolderOpenOutlined />}
              >
                {sampleFiles.map(file => (
                  <Option key={file} value={file}>
                    {file.replace('happy-', '').replace('.json', '').replace(/-/g, ' ')}
                  </Option>
                ))}
              </Select>
            )}
            <Button 
              icon={<FormatPainterOutlined />} 
              onClick={handleFormatJson}
              size="small"
            >
              Format JSON
            </Button>
            <Button 
              icon={<CheckCircleOutlined />} 
              onClick={handleValidateJson}
              size="small"
            >
              Validate JSON
            </Button>
            <Button 
              icon={<ReloadOutlined />} 
              onClick={handleReset}
              size="small"
            >
              Reset
            </Button>
          </Space>
        </div>
      }
      open={open}
      onCancel={handleCancel}
      width="95vw"
      style={{ top: 0, maxWidth: '100%', paddingBottom: 0 }}
      closeIcon={<CloseOutlined />}
      footer={[
        <Button key="cancel" onClick={handleCancel} size="large">
          Cancel
        </Button>,
        <Button
          key="apply"
          type="primary"
          onClick={handleApply}
          size="large"
        >
          Apply Changes
        </Button>,
      ]}
      destroyOnClose
      styles={{
        body: { 
          height: 'calc(100vh - 140px)', 
          padding: 0,
          display: 'flex',
          flexDirection: 'column'
        }
      }}
    >
      {validationError && (
        <Alert
          message="Validation Error"
          description={validationError}
          type="error"
          showIcon
          closable
          onClose={() => setValidationError(null)}
          style={{ margin: '12px 12px 0 12px', borderRadius: '4px' }}
        />
      )}
      <div 
        style={{ 
          flex: 1,
          border: '1px solid #d9d9d9', 
          borderRadius: '4px',
          margin: '12px',
          overflow: 'hidden'
        }}
      >
        <Editor
          height="100%"
          defaultLanguage="json"
          value={editorValue}
          onChange={handleEditorChange}
          onMount={handleEditorDidMount}
          theme="vs-light"
          options={{
            minimap: { enabled: true },
            scrollBeyondLastLine: false,
            fontSize: 14,
            fontFamily: "'Fira Code', 'Consolas', 'Monaco', monospace",
            wordWrap: 'on',
            formatOnPaste: true,
            formatOnType: false,
            automaticLayout: true,
            tabSize: 2,
            lineNumbers: 'on',
            renderWhitespace: 'selection'
          }}
        />
      </div>
    </Modal>
  );
}
