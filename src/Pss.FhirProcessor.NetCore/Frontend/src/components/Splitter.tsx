import React from 'react';
import { useSplitter } from '../hooks/useSplitter';
import './Splitter.css';

interface SplitterProps {
  onWidthChange: (width: number) => void;
  minWidth?: number;
  maxWidth?: number;
}

/**
 * Draggable splitter component for resizing panels
 */
export const Splitter: React.FC<SplitterProps> = ({
  onWidthChange,
  minWidth = 20,
  maxWidth = 80,
}) => {
  const { startDragging } = useSplitter({
    onWidthChange,
    minWidth,
    maxWidth,
  });

  return (
    <div
      className="splitter"
      onMouseDown={startDragging}
      role="separator"
      aria-orientation="vertical"
      aria-label="Resize panels"
      title="Drag to resize"
    >
      <div className="splitter-handle" />
    </div>
  );
};
