import React, { useMemo, useState, useRef, useEffect } from 'react';
import { Tree, Button, Space } from 'antd';
import { ExpandOutlined, ShrinkOutlined, FolderOutlined, FileOutlined } from '@ant-design/icons';
import type { TreeNode } from '../types/fhir';
import './TreeView.css';

interface TreeViewProps {
  jsonData: string;
  onNodeSelect?: (key: string) => void;
}

/**
 * Enhanced tree view component with expand/collapse controls
 * Displays JSON structure in a hierarchical tree format
 */
export const TreeView: React.FC<TreeViewProps> = ({ jsonData, onNodeSelect }) => {
  const [expandedKeys, setExpandedKeys] = useState<React.Key[]>([]);
  const containerRef = useRef<HTMLDivElement>(null);

  // Convert JSON to tree data structure
  const treeData = useMemo(() => {
    if (!jsonData?.trim()) return [];
    try {
      return jsonToTreeData(jsonData);
    } catch {
      return [];
    }
  }, [jsonData]);

  // Collect all keys for expand all functionality
  const allTreeKeys = useMemo(() => {
    const keys: React.Key[] = [];
    const collectAllKeys = (nodes: TreeNode[]) => {
      nodes.forEach((node) => {
        if (!node.isLeaf && node.children) {
          keys.push(node.key);
          collectAllKeys(node.children);
        }
      });
    };
    collectAllKeys(treeData);
    return keys;
  }, [treeData]);

  // Default expanded keys (root level + entry nodes)
  const defaultExpandedKeys = useMemo(() => {
    const keys: React.Key[] = [];
    const collectKeys = (nodes: TreeNode[]) => {
      nodes.forEach((node) => {
        if (!node.isLeaf && node.children) {
          const isRootLevel = node.key.split('.').length === 2;
          const isUnderEntry = node.key.includes('.entry.');
          
          if (isRootLevel || isUnderEntry) {
            keys.push(node.key);
            collectKeys(node.children);
          }
        }
      });
    };
    collectKeys(treeData);
    return keys;
  }, [treeData]);

  // Initialize expanded keys
  useEffect(() => {
    setExpandedKeys(defaultExpandedKeys);
  }, [defaultExpandedKeys]);

  const handleExpandAll = () => {
    setExpandedKeys(allTreeKeys);
  };

  const handleCollapseAll = () => {
    setExpandedKeys([]);
  };

  const handleTreeExpand = (keys: React.Key[]) => {
    setExpandedKeys(keys);
  };

  const handleNodeSelect = (selectedKeys: React.Key[]) => {
    if (selectedKeys.length > 0 && onNodeSelect) {
      onNodeSelect(selectedKeys[0] as string);
    }
  };

  return (
    <div className="tree-view-container" ref={containerRef}>
      <div className="tree-view-header">
        <span className="tree-view-title">Tree View</span>
        <Space size="small">
          <Button
            size="small"
            icon={<ExpandOutlined />}
            onClick={handleExpandAll}
            disabled={treeData.length === 0}
          >
            Expand All
          </Button>
          <Button
            size="small"
            icon={<ShrinkOutlined />}
            onClick={handleCollapseAll}
            disabled={treeData.length === 0}
          >
            Collapse All
          </Button>
        </Space>
      </div>
      <div className="tree-view-content">
        {treeData.length > 0 ? (
          <Tree
            treeData={treeData}
            expandedKeys={expandedKeys}
            onExpand={handleTreeExpand}
            onSelect={handleNodeSelect}
            showIcon
            className="fhir-tree"
            virtual
            height={800}
          />
        ) : (
          <div className="tree-view-empty">
            <FileOutlined style={{ fontSize: 48, color: '#d9d9d9', marginBottom: 16 }} />
            <div>No JSON data to display</div>
            <div className="empty-hint">Process a FHIR bundle to see the structure</div>
          </div>
        )}
      </div>
    </div>
  );
};

// Helper function to convert JSON to tree data structure
function jsonToTreeData(jsonString: string, parentKey: string = 'root'): TreeNode[] {
  const buildNode = (value: any, key: string, path: string): TreeNode => {
    const nodeKey = `${path}.${key}`;

    if (Array.isArray(value)) {
      return {
        title: (
          <span>
            <strong>{key}</strong>:{' '}
            <span className="tree-value tree-array">[Array({value.length})]</span>
          </span>
        ),
        key: nodeKey,
        icon: <FolderOutlined />,
        children: value.map((item, idx) => buildNode(item, `[${idx}]`, nodeKey)),
      };
    } else if (typeof value === 'object' && value !== null) {
      const keys = Object.keys(value);
      return {
        title: (
          <span>
            <strong>{key}</strong>: <span className="tree-value tree-object">{`{Object}`}</span>
          </span>
        ),
        key: nodeKey,
        icon: <FolderOutlined />,
        children: keys.map((k) => buildNode(value[k], k, nodeKey)),
      };
    } else {
      const valueStr = String(value);
      const displayValue = valueStr.length > 100 ? valueStr.substring(0, 100) + '...' : valueStr;
      const valueClass =
        typeof value === 'string'
          ? 'tree-string'
          : typeof value === 'number'
          ? 'tree-number'
          : typeof value === 'boolean'
          ? 'tree-boolean'
          : 'tree-null';

      return {
        title: (
          <span>
            <strong>{key}</strong>: <span className={`tree-value ${valueClass}`}>"{displayValue}"</span>
          </span>
        ),
        key: nodeKey,
        icon: <FileOutlined />,
        isLeaf: true,
      };
    }
  };

  try {
    const parsed = typeof jsonString === 'string' ? JSON.parse(jsonString) : jsonString;
    const keys = Object.keys(parsed);
    return keys.map((key) => buildNode(parsed[key], key, parentKey));
  } catch (error) {
    return [
      {
        title: <span className="tree-error">Invalid JSON</span>,
        key: 'error',
        isLeaf: true,
        icon: <FileOutlined />,
      },
    ];
  }
}
