import React, { useRef, useState, useEffect } from 'react';

interface SplitterProps {
  leftWidth: number;
  onWidthChange: (newWidth: number) => void;
  minWidth: number;
  maxWidth: number;
}

export default function Splitter({ leftWidth, onWidthChange, minWidth, maxWidth }: SplitterProps) {
  const [isDragging, setIsDragging] = useState(false);
  const startXRef = useRef(0);
  const startWidthRef = useRef(0);

  useEffect(() => {
    if (!isDragging) return;

    const handleMouseMove = (e: MouseEvent) => {
      const deltaX = e.clientX - startXRef.current;
      const containerWidth = window.innerWidth;
      const deltaPercent = (deltaX / containerWidth) * 100;
      const newWidth = startWidthRef.current + deltaPercent;
      
      onWidthChange(newWidth);
    };

    const handleMouseUp = () => {
      setIsDragging(false);
    };

    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);

    return () => {
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
    };
  }, [isDragging, onWidthChange]);

  const handleMouseDown = (e: React.MouseEvent) => {
    e.preventDefault();
    setIsDragging(true);
    startXRef.current = e.clientX;
    startWidthRef.current = leftWidth;
  };

  return (
    <div
      className={`w-1 bg-gray-300 hover:bg-blue-500 cursor-col-resize transition-colors ${
        isDragging ? 'bg-blue-500' : ''
      }`}
      onMouseDown={handleMouseDown}
      style={{ userSelect: 'none' }}
    />
  );
}
