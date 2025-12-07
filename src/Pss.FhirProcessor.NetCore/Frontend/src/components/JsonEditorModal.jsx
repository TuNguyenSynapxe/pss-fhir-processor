import { useState, useEffect, useRef } from 'react';
import { Modal, Button, message, Space, Alert } from 'antd';
import { 
  FormatPainterOutlined, 
  CheckCircleOutlined, 
  ReloadOutlined, 
  CloseOutlined 
} from '@ant-design/icons';
import Editor from '@monaco-editor/react';

function JsonEditorModal({ isOpen, onClose, onApply, initialValue }) {
  const [editorValue, setEditorValue] = useState('');
  const [initialSnapshot, setInitialSnapshot] = useState('');
  const [validationError, setValidationError] = useState(null);
  const editorRef = useRef(null);

  // Initialize editor value when modal opens
  useEffect(() => {
    if (isOpen && initialValue) {
      const formatted = formatJson(initialValue);
      setEditorValue(formatted);
      setInitialSnapshot(formatted); // Save initial state for reset
      setValidationError(null);
    }
  }, [isOpen, initialValue]);

  const handleEditorDidMount = (editor) => {
    editorRef.current = editor;
  };

  const handleEditorChange = (value) => {
    setEditorValue(value || '');
    // Clear validation error when user starts typing
    if (validationError) {
      setValidationError(null);
    }
  };

  // Utility: Format JSON with 2-space indentation
  const formatJson = (jsonString) => {
    try {
      const parsed = JSON.parse(jsonString);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return jsonString; // Return as-is if invalid
    }
  };

  // Utility: Extract line and column from JSON parse error
  const extractErrorPosition = (errorMessage) => {
    // Try to extract position from error message
    // Typical format: "Unexpected token } in JSON at position 123"
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

  // Handler: Format JSON button
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
    } catch (error) {
      const position = extractErrorPosition(error.message);
      const errorMsg = position 
        ? `Invalid JSON at line ${position.line}, column ${position.column}: ${error.message}`
        : `Invalid JSON: ${error.message}`;
      setValidationError(errorMsg);
      message.error('Cannot format invalid JSON');
    }
  };

  // Handler: Validate JSON button
  const handleValidateJson = () => {
    if (!editorValue.trim()) {
      message.warning('Editor is empty');
      return;
    }

    try {
      JSON.parse(editorValue);
      setValidationError(null);
      message.success('✓ JSON is valid');
    } catch (error) {
      const position = extractErrorPosition(error.message);
      const errorMsg = position 
        ? `Line ${position.line}, Column ${position.column}: ${error.message}`
        : error.message;
      setValidationError(errorMsg);
      
      // Move cursor to error position if available
      if (position && editorRef.current) {
        editorRef.current.setPosition({ lineNumber: position.line, column: position.column });
        editorRef.current.revealLineInCenter(position.line);
        editorRef.current.focus();
      }
    }
  };

  // Handler: Reset button
  const handleReset = () => {
    setEditorValue(initialSnapshot);
    setValidationError(null);
    message.info('Editor reset to initial state');
  };

  // Handler: Apply Changes button
  const handleApply = () => {
    if (!editorValue.trim()) {
      message.warning('Editor is empty');
      return;
    }

    try {
      const parsed = JSON.parse(editorValue);
      const formatted = JSON.stringify(parsed, null, 2);
      onApply(formatted, parsed);
      message.success('JSON updated successfully');
      setValidationError(null);
      onClose();
    } catch (error) {
      const position = extractErrorPosition(error.message);
      const errorMsg = position 
        ? `Line ${position.line}, Column ${position.column}: ${error.message}`
        : error.message;
      setValidationError(errorMsg);
      message.error('Cannot apply invalid JSON');
      
      // Move cursor to error position
      if (position && editorRef.current) {
        editorRef.current.setPosition({ lineNumber: position.line, column: position.column });
        editorRef.current.revealLineInCenter(position.line);
        editorRef.current.focus();
      }
    }
  };

  // Handler: Close/Cancel
  const handleCancel = () => {
    setValidationError(null);
    onClose();
  };

  return (
    <Modal
      title={
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', paddingRight: '20px' }}>
          <span>Edit FHIR JSON Input</span>
          <Space>
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
      open={isOpen}
      onCancel={handleCancel}
      width="90vw"
      style={{ top: 20 }}
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
          height: '90vh', 
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
          overflow: 'hidden',
          display: 'flex',
          flexDirection: 'column'
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
            renderWhitespace: 'selection',
            scrollbar: {
              vertical: 'auto',
              horizontal: 'auto'
            }
          }}
        />
      </div>
    </Modal>
  );
}

export default JsonEditorModal;

