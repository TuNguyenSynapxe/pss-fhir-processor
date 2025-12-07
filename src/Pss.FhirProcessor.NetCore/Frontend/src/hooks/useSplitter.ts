import { useRef, useCallback, useEffect } from 'react';

interface UseSplitterOptions {
  onWidthChange: (width: number) => void;
  minWidth?: number;
  maxWidth?: number;
}

/**
 * Custom hook for draggable splitter functionality
 * @param options Configuration options
 * @returns Object with splitter handlers
 */
export function useSplitter({
  onWidthChange,
  minWidth = 20,
  maxWidth = 80,
}: UseSplitterOptions) {
  const dragging = useRef(false);

  const startDragging = useCallback((e: React.MouseEvent) => {
    e.preventDefault();
    dragging.current = true;
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
  }, []);

  const stopDragging = useCallback(() => {
    if (dragging.current) {
      dragging.current = false;
      document.body.style.cursor = '';
      document.body.style.userSelect = '';
    }
  }, []);

  const handleMouseMove = useCallback(
    (e: MouseEvent) => {
      if (!dragging.current) return;

      const percentage = (e.clientX / window.innerWidth) * 100;
      const clamped = Math.min(Math.max(percentage, minWidth), maxWidth);
      onWidthChange(clamped);
    },
    [onWidthChange, minWidth, maxWidth]
  );

  useEffect(() => {
    const handleMove = (e: MouseEvent) => handleMouseMove(e);
    const handleUp = () => stopDragging();

    window.addEventListener('mousemove', handleMove);
    window.addEventListener('mouseup', handleUp);

    return () => {
      window.removeEventListener('mousemove', handleMove);
      window.removeEventListener('mouseup', handleUp);
    };
  }, [handleMouseMove, stopDragging]);

  return {
    startDragging,
    isDragging: dragging.current,
  };
}
