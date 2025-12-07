import React, { useState, useEffect, useRef } from 'react';
import { Modal, Button, Space, Alert, message } from 'antd';
import {
  FormatPainterOutlined,
  CheckCircleOutlined,
  ReloadOutlined,
  CloseOutlined,
} from '@ant-design/icons';
import Editor from '@monaco-editor/react';
import { useJSONFormatter } from '../hooks/useJSONFormatter';
import type { JSONEditorState } from '../types/fhir';
import './JsonEditorModal.css';

interface JsonEditorModalProps {
  isOpen: boolean;
  initialValue: string;
  onClose: () => void;
  onApply: (formattedJson: string, parsedObject: any) => void;
}

/**
 * Enhanced JSON editor modal with Monaco editor
 * Provides format, validate, reset, and apply functionality
 */
export const JsonEditorModal: React.FC<JsonEditorModalProps> = ({
  isOpen,
  initialValue,
  onClose,
  onApply,
}) => {
  const [editorState, setEditorState] = useState<JSONEditorState>({
    value: '',
    isValid: true,
  });
  const [initialSnapshot, setInitialSnapshot] = useState('');
  const editorRef = useRef<any>(null);
  const { formatJSON, validateJSON } = useJSONFormatter();

  // Initialize editor when modal opens
  useEffect(() => {
    if (isOpen && initialValue) {
      const formatted = formatJSON(initialValue);
      const value = formatted.isValid ? formatted.formatted : initialValue;
      setEditorState({ value, isValid: true });
      setInitialSnapshot(value);
    }
  }, [isOpen, initialValue, formatJSON]);

  const handleEditorDidMount = (editor: any) => {
    editorRef.current = editor;
  };

  const handleEditorChange = (value: string | undefined) => {
    setEditorState((prev) => ({
      ...prev,
      value: value || '',
      error: undefined,
      errorPosition: undefined,
    }));
  };

  const handleFormat = () => {
    const result = formatJSON(editorState.value);
    
    if (result.isValid) {
      setEditorState({
        value: result.formatted,
        isValid: true,
      });
      message.success('JSON formatted successfully');
    } else {
      setEditorState({
        value: editorState.value,
        isValid: false,
        error: result.error,
        errorPosition: result.errorPosition,
      });
      message.error('Cannot format invalid JSON');
      
      // Move cursor to error position
      if (result.errorPosition && editorRef.current) {
        moveCursorToError(result.errorPosition);
      }
    }
  };

  const handleValidate = () => {
    const result = validateJSON(editorState.value);
    
    if (result.isValid) {
      setEditorState({
        value: editorState.value,
        isValid: true,
      });
      message.success('✓ JSON is valid');
    } else {
      setEditorState({
        value: editorState.value,
        isValid: false,
        error: result.error,
        errorPosition: result.errorPosition,
      });
      
      // Move cursor to error position
      if (result.errorPosition && editorRef.current) {
        moveCursorToError(result.errorPosition);
      }
    }
  };

  const handleReset = () => {
    setEditorState({
      value: initialSnapshot,
      isValid: true,
    });
    message.info('Editor reset to initial state');
  };

  const handleApply = () => {
    const result = validateJSON(editorState.value);
    
    if (!result.isValid) {
      setEditorState({
        value: editorState.value,
        isValid: false,
        error: result.error,
        errorPosition: result.errorPosition,
      });
      message.error('Cannot apply invalid JSON');
      
      if (result.errorPosition && editorRef.current) {
        moveCursorToError(result.errorPosition);
      }
      return;
    }

    try {
      const parsed = JSON.parse(editorState.value);
      const formatted = formatJSON(editorState.value);
      onApply(formatted.formatted, parsed);
      message.success('JSON updated successfully');
      onClose();
    } catch (error) {
      message.error('Failed to apply changes');
    }
  };

  const handleCancel = () => {
    setEditorState({ value: '', isValid: true });
    onClose();
  };

  const moveCursorToError = (position: { line: number; column: number }) => {
    if (editorRef.current) {
      editorRef.current.setPosition({
        lineNumber: position.line,
        column: position.column,
      });
      editorRef.current.revealLineInCenter(position.line);
      editorRef.current.focus();
    }
  };

  return (
    <Modal
      title={
        <div className="json-editor-modal-header">
          <span className="modal-title">Edit FHIR JSON Input</span>
          <Space>
            <Button
              icon={<FormatPainterOutlined />}
              onClick={handleFormat}
              size="small"
            >
              Format JSON
            </Button>
            <Button
              icon={<CheckCircleOutlined />}
              onClick={handleValidate}
              size="small"
            >
              Validate JSON
            </Button>
            <Button icon={<ReloadOutlined />} onClick={handleReset} size="small">
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
      footer={
        <div className="json-editor-modal-footer">
          <Button size="large" onClick={handleCancel}>
            Cancel
          </Button>
          <Button type="primary" size="large" onClick={handleApply}>
            Apply Changes
          </Button>
        </div>
      }
      destroyOnClose
      className="json-editor-modal"
    >
      <div className="json-editor-modal-content">
        {editorState.error && (
          <Alert
            message="Validation Error"
            description={
              editorState.errorPosition
                ? `Line ${editorState.errorPosition.line}, Column ${editorState.errorPosition.column}: ${editorState.error}`
                : editorState.error
            }
            type="error"
            showIcon
            closable
            onClose={() =>
              setEditorState((prev) => ({
                ...prev,
                error: undefined,
                errorPosition: undefined,
              }))
            }
            className="validation-alert"
          />
        )}
        <div className="editor-container">
          <Editor
            height="100%"
            defaultLanguage="json"
            value={editorState.value}
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
                horizontal: 'auto',
              },
              quickSuggestions: {
                other: true,
                comments: false,
                strings: true,
              },
              suggestOnTriggerCharacters: true,
            }}
          />
        </div>
      </div>
    </Modal>
  );
};
