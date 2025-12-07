import { useState, useEffect } from 'react';

interface UseLocalStorageWidthOptions {
  key: string;
  defaultValue: number;
  min?: number;
  max?: number;
}

/**
 * Custom hook to persist panel width in localStorage
 * @param options Configuration options
 * @returns [width, setWidth] tuple
 */
export function useLocalStorageWidth({
  key,
  defaultValue,
  min = 20,
  max = 80,
}: UseLocalStorageWidthOptions): [number, (value: number) => void] {
  const [width, setWidthState] = useState<number>(() => {
    try {
      const stored = localStorage.getItem(key);
      if (stored) {
        const parsed = parseFloat(stored);
        return Math.min(Math.max(parsed, min), max);
      }
    } catch (error) {
      console.error('Failed to read from localStorage:', error);
    }
    return defaultValue;
  });

  const setWidth = (value: number) => {
    const clamped = Math.min(Math.max(value, min), max);
    setWidthState(clamped);
    
    try {
      localStorage.setItem(key, clamped.toString());
    } catch (error) {
      console.error('Failed to write to localStorage:', error);
    }
  };

  return [width, setWidth];
}
