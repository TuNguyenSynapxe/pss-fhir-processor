import { useCallback } from 'react';

interface JSONFormatResult {
  formatted: string;
  isValid: boolean;
  error?: string;
  errorPosition?: { line: number; column: number };
}

/**
 * Custom hook for JSON formatting and validation utilities
 * @returns Object with formatting functions
 */
export function useJSONFormatter() {
  const formatJSON = useCallback((jsonString: string, indent: number = 2): JSONFormatResult => {
    if (!jsonString?.trim()) {
      return {
        formatted: '',
        isValid: false,
        error: 'Empty input',
      };
    }

    try {
      const parsed = JSON.parse(jsonString);
      const formatted = JSON.stringify(parsed, null, indent);
      return {
        formatted,
        isValid: true,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      const position = extractErrorPosition(errorMessage, jsonString);
      
      return {
        formatted: jsonString,
        isValid: false,
        error: errorMessage,
        errorPosition: position,
      };
    }
  }, []);

  const validateJSON = useCallback((jsonString: string): JSONFormatResult => {
    if (!jsonString?.trim()) {
      return {
        formatted: '',
        isValid: false,
        error: 'Empty input',
      };
    }

    try {
      JSON.parse(jsonString);
      return {
        formatted: jsonString,
        isValid: true,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      const position = extractErrorPosition(errorMessage, jsonString);
      
      return {
        formatted: jsonString,
        isValid: false,
        error: errorMessage,
        errorPosition: position,
      };
    }
  }, []);

  const minifyJSON = useCallback((jsonString: string): JSONFormatResult => {
    try {
      const parsed = JSON.parse(jsonString);
      const minified = JSON.stringify(parsed);
      return {
        formatted: minified,
        isValid: true,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        formatted: jsonString,
        isValid: false,
        error: errorMessage,
      };
    }
  }, []);

  return {
    formatJSON,
    validateJSON,
    minifyJSON,
  };
}

/**
 * Extract line and column number from JSON parse error
 */
function extractErrorPosition(
  errorMessage: string,
  jsonString: string
): { line: number; column: number } | undefined {
  const positionMatch = errorMessage.match(/position (\d+)/);
  
  if (positionMatch && jsonString) {
    const position = parseInt(positionMatch[1], 10);
    const lines = jsonString.substring(0, position).split('\n');
    const line = lines.length;
    const column = lines[lines.length - 1].length + 1;
    return { line, column };
  }
  
  return undefined;
}
