/**
 * Path Parser & Analyzer
 * Parses FHIR path expressions and analyzes JSON trees to determine missing segments
 */

export type SegmentKind = 'property' | 'arrayIndex' | 'filteredArray';

export interface PathSegment {
  raw: string;           // Original text: "extension[url:...]", "coding[0]", "system"
  kind: SegmentKind;
  property: string;      // Property name: "extension", "coding", "system"
  index?: number;        // For arrayIndex
  filterKey?: string;    // For filteredArray: "url"
  filterValue?: string;  // For filteredArray: "https://..."
}

export type SegmentStatusKind =
  | 'EXISTS'
  | 'MISSING_ARRAY'
  | 'MISSING_PROPERTY'
  | 'FILTER_NO_MATCH'
  | 'INDEX_OUT_OF_RANGE';

export interface SegmentStatus {
  segment: PathSegment;
  status: SegmentStatusKind;
  node?: any;   // Actual node value if EXISTS
}

// Legacy interface for backward compatibility
export interface PathStatus {
  exists: boolean;
  segment: string;
  nodeValue: any;
  isMissingParent: boolean;
  depth: number;
}

/**
 * Parse a field path string into structured segments
 * Examples:
 * - "Patient.name" → [{type: 'property', name: 'Patient'}, {type: 'property', name: 'name'}]
 * - "coding[0]" → [{type: 'array', name: 'coding', index: 0}]
 * - "extension[url:https://...]" → [{type: 'filteredArray', name: 'extension', filter: {...}}]
 */
export function parseFieldPath(fieldPath: string): PathSegment[] {
  if (!fieldPath) return [];

  const segments: PathSegment[] = [];
  
  // Smart split: split on dots, but not dots inside brackets
  // This preserves URLs in filters like extension[url:https://fhir.synapxe.sg/...]
  const parts: string[] = [];
  let currentPart = '';
  let bracketDepth = 0;
  
  for (let i = 0; i < fieldPath.length; i++) {
    const char = fieldPath[i];
    
    if (char === '[') {
      bracketDepth++;
      currentPart += char;
    } else if (char === ']') {
      bracketDepth--;
      currentPart += char;
    } else if (char === '.' && bracketDepth === 0) {
      // Only split on dots outside of brackets
      if (currentPart) {
        parts.push(currentPart);
        currentPart = '';
      }
    } else {
      currentPart += char;
    }
  }
  
  // Add the last part
  if (currentPart) {
    parts.push(currentPart);
  }

  for (const part of parts) {
    // Check for array index: property[0]
    const arrayIndexMatch = part.match(/^([^[]+)\[(\d+)\]$/);
    if (arrayIndexMatch) {
      segments.push({
        raw: part,
        kind: 'arrayIndex',
        property: arrayIndexMatch[1],
        index: parseInt(arrayIndexMatch[2], 10),
      });
      continue;
    }

    // Check for filtered array: property[key:value]
    const filteredArrayMatch = part.match(/^([^[]+)\[([^:]+):([^\]]+)\]$/);
    if (filteredArrayMatch) {
      segments.push({
        raw: part,
        kind: 'filteredArray',
        property: filteredArrayMatch[1],
        filterKey: filteredArrayMatch[2],
        filterValue: filteredArrayMatch[3],
      });
      continue;
    }

    // Check for wildcard array: property[*] - treat as index 0
    const wildcardMatch = part.match(/^([^[]+)\[\*\]$/);
    if (wildcardMatch) {
      segments.push({
        raw: part,
        kind: 'arrayIndex',
        property: wildcardMatch[1],
        index: 0,
      });
      continue;
    }

    // Simple property
    segments.push({
      raw: part,
      kind: 'property',
      property: part,
    });
  }

  return segments;
}

/**
 * Resolve path segments against JSON tree with detailed status tracking
 * Returns detailed status for each segment and index of first non-existing segment
 */
export function resolvePathSegments(
  root: any,
  segments: PathSegment[]
): {
  statuses: SegmentStatus[];
  firstNonExistingIndex: number;
} {
  const statuses: SegmentStatus[] = [];
  let currentNode = root;
  let firstNonExistingIndex = -1;

  for (let i = 0; i < segments.length; i++) {
    const segment = segments[i];
    let status: SegmentStatus;

    if (segment.kind === 'property') {
      // Simple property access
      if (currentNode === null || currentNode === undefined || typeof currentNode !== 'object') {
        status = {
          segment,
          status: 'MISSING_PROPERTY',
        };
      } else if (segment.property in currentNode) {
        status = {
          segment,
          status: 'EXISTS',
          node: currentNode[segment.property],
        };
        currentNode = currentNode[segment.property];
      } else {
        status = {
          segment,
          status: 'MISSING_PROPERTY',
        };
      }
    } else if (segment.kind === 'arrayIndex') {
      // Array with index
      if (currentNode === null || currentNode === undefined || typeof currentNode !== 'object') {
        status = {
          segment,
          status: 'MISSING_ARRAY',
        };
      } else {
        const array = currentNode[segment.property];
        if (!Array.isArray(array)) {
          status = {
            segment,
            status: 'MISSING_ARRAY',
          };
        } else if (segment.index! >= array.length) {
          status = {
            segment,
            status: 'INDEX_OUT_OF_RANGE',
            node: array, // Store array for context
          };
        } else {
          status = {
            segment,
            status: 'EXISTS',
            node: array[segment.index!],
          };
          currentNode = array[segment.index!];
        }
      }
    } else if (segment.kind === 'filteredArray') {
      // Filtered array
      if (currentNode === null || currentNode === undefined || typeof currentNode !== 'object') {
        status = {
          segment,
          status: 'MISSING_ARRAY',
        };
      } else {
        const array = currentNode[segment.property];
        if (!Array.isArray(array)) {
          status = {
            segment,
            status: 'MISSING_ARRAY',
          };
        } else {
          // Find matching element
          const matchedElement = array.find(
            (item) => item?.[segment.filterKey!] === segment.filterValue
          );
          if (matchedElement) {
            status = {
              segment,
              status: 'EXISTS',
              node: matchedElement,
            };
            currentNode = matchedElement;
          } else {
            status = {
              segment,
              status: 'FILTER_NO_MATCH',
              node: array, // Store array for context
            };
          }
        }
      }
    } else {
      // Fallback for unknown segment kinds
      status = {
        segment,
        status: 'MISSING_PROPERTY',
      };
    }

    statuses.push(status);

    // Track first non-existing index
    if (firstNonExistingIndex === -1 && status.status !== 'EXISTS') {
      firstNonExistingIndex = i;
    }

    // Stop traversal if current segment doesn't exist
    if (status.status !== 'EXISTS') {
      // Mark all remaining segments as missing parents
      for (let j = i + 1; j < segments.length; j++) {
        statuses.push({
          segment: segments[j],
          status: 'MISSING_PROPERTY',
        });
      }
      break;
    }
  }

  return { statuses, firstNonExistingIndex };
}

/**
 * Analyze a JSON tree against parsed path segments (LEGACY - for backward compatibility)
 * Returns status for each segment showing existence and values
 * @deprecated Use resolvePathSegments for more detailed analysis
 */
export function analyzePath(json: any, segments: PathSegment[]): PathStatus[] {
  // Use new resolver and convert to legacy format
  const { statuses: segmentStatuses } = resolvePathSegments(json, segments);
  
  return segmentStatuses.map((ss, idx) => ({
    exists: ss.status === 'EXISTS',
    segment: ss.segment.raw,
    nodeValue: ss.node,
    isMissingParent: idx > 0 && segmentStatuses[idx - 1].status !== 'EXISTS',
    depth: idx,
  }));
}

/**
 * Format a segment for display (LEGACY)
 */
export function formatSegment(segment: PathSegment): string {
  return segment.raw;
}

/**
 * Find the first missing segment index (LEGACY)
 * Returns -1 if all segments exist
 * @deprecated Use resolvePathSegments instead
 */
export function findFirstMissingSegment(statuses: PathStatus[]): number {
  return statuses.findIndex((status) => !status.exists);
}

/**
 * Get parent segments up to a specific depth
 */
export function getParentSegments(segments: PathSegment[], depth: number): PathSegment[] {
  return segments.slice(0, depth);
}

/**
 * Build a path string from segments
 */
export function buildPathString(segments: PathSegment[]): string {
  return segments.map(formatSegment).join('.');
}
