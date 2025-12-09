/**
 * SmartPathNavigator Component (Phase 2 - Initial Skeleton)
 * 
 * A unified path navigation component that combines breadcrumb and path breakdown
 * into a single, progressive disclosure interface.
 * 
 * This is the initial skeleton implementation. Full progressive disclosure
 * and advanced UX features will be added in later phases.
 */

import React, { useState } from 'react';
import { Space, Tag, Typography, Collapse, Button } from 'antd';
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  EnvironmentOutlined,
  DownOutlined,
  RightOutlined,
} from '@ant-design/icons';
import { EnhancedPathSegment, SegmentStatusKind } from '../utils/pathParser';

const { Text } = Typography;
const { Panel } = Collapse;

export interface SmartPathNavigatorProps {
  /** Enhanced path segments with rich metadata */
  segments: EnhancedPathSegment[];
  
  /** Callback when user clicks on a segment to navigate */
  onSegmentClick?: (jumpKey: string) => void;
  
  /** Whether detailed view is expanded by default */
  defaultExpanded?: boolean;
  
  /** Optional title for the navigator */
  title?: string;
  
  /** JumpKey of segment to highlight (synchronized with external hover/selection) */
  highlightSegment?: string | null;
  
  /** Callback when user hovers over a segment */
  onSegmentHover?: (jumpKey: string | null) => void;
}

/**
 * SmartPathNavigator - Unified path navigation with progressive disclosure
 */
export const SmartPathNavigator: React.FC<SmartPathNavigatorProps> = ({
  segments,
  onSegmentClick,
  defaultExpanded = false,
  title = 'Path Navigator',
  highlightSegment,
  onSegmentHover,
}) => {
  const [isExpanded, setIsExpanded] = useState(defaultExpanded);

  if (!segments || segments.length === 0) {
    return (
      <div style={{ padding: 16, background: '#f5f5f5', borderRadius: 4 }}>
        <Text type="secondary">No path information available</Text>
      </div>
    );
  }

  return (
    <div style={{ marginBottom: 16 }}>
      {/* Compact Breadcrumb View */}
      <div
        style={{
          padding: 12,
          background: '#fafafa',
          borderRadius: 4,
          border: '1px solid #d9d9d9',
        }}
      >
        <Space direction="vertical" style={{ width: '100%' }} size="small">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Text strong>{title}</Text>
            <Button
              type="text"
              size="small"
              icon={isExpanded ? <DownOutlined /> : <RightOutlined />}
              onClick={() => setIsExpanded(!isExpanded)}
            >
              {isExpanded ? 'Hide Details' : 'Show Details'}
            </Button>
          </div>

          {/* Horizontal breadcrumb with status dots */}
          <div style={{ display: 'flex', alignItems: 'center', flexWrap: 'wrap', gap: 4 }}>
            {segments.map((seg, idx) => (
              <React.Fragment key={seg.jumpKey}>
                <div
                  className={seg.jumpKey === highlightSegment ? 'smart-path-highlight' : ''}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 6,
                    padding: '4px 8px',
                    background: seg.jumpKey === highlightSegment ? '#e6f7ff' : seg.isTarget ? '#fff7e6' : 'transparent',
                    borderRadius: 4,
                    cursor: onSegmentClick && seg.exists ? 'pointer' : 'default',
                    transition: 'background 0.2s ease',
                  }}
                  onClick={() => {
                    if (onSegmentClick && seg.exists) {
                      onSegmentClick(seg.jumpKey);
                    }
                  }}
                  onMouseEnter={() => onSegmentHover?.(seg.jumpKey)}
                  onMouseLeave={() => onSegmentHover?.(null)}
                >
                  {/* Status dot */}
                  <span
                    style={{
                      width: 8,
                      height: 8,
                      borderRadius: '50%',
                      background: getStatusColor(seg.status),
                      display: 'inline-block',
                    }}
                    title={seg.status}
                  />
                  
                  {/* Label */}
                  <Text
                    code
                    style={{
                      fontSize: 12,
                      fontWeight: seg.isTarget ? 'bold' : 'normal',
                      color: seg.exists ? '#000' : '#ff4d4f',
                    }}
                  >
                    {seg.label}
                  </Text>

                  {/* Discriminator badge */}
                  {seg.discriminator && (
                    <Tag
                      color={seg.discriminator.matchFound ? 'blue' : 'orange'}
                      style={{ fontSize: 10, padding: '0 4px', marginLeft: 4 }}
                    >
                      {seg.discriminator.displayValue}
                    </Tag>
                  )}
                </div>

                {/* Arrow separator */}
                {idx < segments.length - 1 && (
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    →
                  </Text>
                )}
              </React.Fragment>
            ))}
          </div>
        </Space>
      </div>

      {/* Detailed Breakdown View (expandable) */}
      {isExpanded && (
        <div
          style={{
            marginTop: 8,
            padding: 12,
            background: '#fff',
            border: '1px solid #d9d9d9',
            borderRadius: 4,
          }}
        >
          <div style={{ fontFamily: 'monospace', fontSize: 12 }}>
            {segments.map((seg, idx) => (
              <DetailedSegmentView
                key={seg.jumpKey}
                segment={seg}
                onSegmentClick={onSegmentClick}
                highlightSegment={highlightSegment}
                onSegmentHover={onSegmentHover}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

/**
 * Detailed view of a single segment in the breakdown
 */
interface DetailedSegmentViewProps {
  segment: EnhancedPathSegment;
  onSegmentClick?: (jumpKey: string) => void;
  highlightSegment?: string | null;
  onSegmentHover?: (jumpKey: string | null) => void;
}

const DetailedSegmentView: React.FC<DetailedSegmentViewProps> = ({ 
  segment, 
  onSegmentClick,
  highlightSegment,
  onSegmentHover,
}) => {
  const icon = segment.exists ? (
    <CheckCircleOutlined style={{ color: '#52c41a', marginRight: 8 }} />
  ) : (
    <CloseCircleOutlined style={{ color: '#ff4d4f', marginRight: 8 }} />
  );

  const statusTag = getStatusTag(segment.status);
  const isHighlighted = segment.jumpKey === highlightSegment;

  return (
    <div
      className={isHighlighted ? 'smart-path-highlight' : ''}
      style={{
        paddingLeft: segment.depth * 20,
        marginBottom: 6,
        display: 'flex',
        alignItems: 'center',
        gap: 8,
        background: isHighlighted ? '#e6f7ff' : 'transparent',
        padding: '4px 8px',
        borderRadius: 4,
        marginLeft: -8,
        transition: 'background 0.2s ease',
      }}
      onMouseEnter={() => onSegmentHover?.(segment.jumpKey)}
      onMouseLeave={() => onSegmentHover?.(null)}
    >
      {/* Status icon */}
      {icon}

      {/* Segment raw path */}
      <Text
        code
        style={{
          color: segment.exists ? '#000' : '#ff4d4f',
          fontWeight: segment.isTarget ? 'bold' : 'normal',
        }}
      >
        {segment.raw}
      </Text>

      {/* Status tag */}
      {statusTag}

      {/* Discriminator info */}
      {segment.discriminator && !segment.discriminator.matchFound && (
        <Tag color="warning" style={{ fontSize: 10 }}>
          No match in {segment.discriminator.arrayLength || 0} elements
        </Tag>
      )}

      {/* Node value preview (if exists) */}
      {segment.exists && segment.node !== undefined && (
        <Text type="secondary" style={{ fontSize: 11 }}>
          {typeof segment.node === 'object'
            ? Array.isArray(segment.node)
              ? `[array: ${segment.node.length} items]`
              : '[object]'
            : `= ${JSON.stringify(segment.node)}`}
        </Text>
      )}

      {/* Navigate button (if clickable and exists) */}
      {onSegmentClick && segment.exists && (
        <Button
          type="link"
          size="small"
          icon={<EnvironmentOutlined />}
          onClick={() => onSegmentClick(segment.jumpKey)}
          style={{ fontSize: 11, padding: 0 }}
        >
          Jump
        </Button>
      )}
    </div>
  );
};

/**
 * Get color for status dot based on segment status
 */
function getStatusColor(status: SegmentStatusKind): string {
  switch (status) {
    case SegmentStatusKind.EXISTS:
      return '#52c41a'; // Green
    case SegmentStatusKind.MISSING_PROPERTY:
    case SegmentStatusKind.MISSING_ARRAY:
      return '#ff4d4f'; // Red
    case SegmentStatusKind.FILTER_NO_MATCH:
    case SegmentStatusKind.INDEX_OUT_OF_RANGE:
      return '#faad14'; // Orange
    default:
      return '#d9d9d9'; // Gray
  }
}

/**
 * Get status tag for detailed view
 */
function getStatusTag(status: SegmentStatusKind): React.ReactNode {
  switch (status) {
    case SegmentStatusKind.EXISTS:
      return null; // No tag needed for successful status
    case SegmentStatusKind.MISSING_PROPERTY:
      return (
        <Tag color="error" style={{ fontSize: 10 }}>
          property missing
        </Tag>
      );
    case SegmentStatusKind.MISSING_ARRAY:
      return (
        <Tag color="error" style={{ fontSize: 10 }}>
          array missing
        </Tag>
      );
    case SegmentStatusKind.FILTER_NO_MATCH:
      return (
        <Tag color="warning" style={{ fontSize: 10 }}>
          filter mismatch
        </Tag>
      );
    case SegmentStatusKind.INDEX_OUT_OF_RANGE:
      return (
        <Tag color="warning" style={{ fontSize: 10 }}>
          index out of range
        </Tag>
      );
    default:
      return null;
  }
}

export default SmartPathNavigator;
