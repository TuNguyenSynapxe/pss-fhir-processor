/**
 * Path Parser & Analyzer
 * Parses FHIR path expressions and analyzes JSON trees to determine missing segments
 */

// NEW: Enums for better type safety and maintainability (Phase 1)
export enum SegmentStatusKind {
  EXISTS = 'EXISTS',
  MISSING_PROPERTY = 'MISSING_PROPERTY',
  MISSING_ARRAY = 'MISSING_ARRAY',
  FILTER_NO_MATCH = 'FILTER_NO_MATCH',
  INDEX_OUT_OF_RANGE = 'INDEX_OUT_OF_RANGE',
}

export enum SegmentKind {
  PROPERTY = 'property',
  ARRAY_INDEX = 'arrayIndex',
  FILTERED_ARRAY = 'filteredArray',
}

// LEGACY: Old types kept for backward compatibility
// @deprecated Use SegmentKind enum instead
export type SegmentKindLegacy = 'property' | 'arrayIndex' | 'filteredArray';

// @deprecated Use SegmentStatusKind enum instead
export type SegmentStatusKindLegacy =
  | 'EXISTS'
  | 'MISSING_ARRAY'
  | 'MISSING_PROPERTY'
  | 'FILTER_NO_MATCH'
  | 'INDEX_OUT_OF_RANGE';

// LEGACY: PathSegment interface kept for backward compatibility
// @deprecated Use EnhancedPathSegment instead
export interface PathSegment {
  raw: string;           // Original text: "extension[url:...]", "coding[0]", "system"
  kind: SegmentKindLegacy;
  property: string;      // Property name: "extension", "coding", "system"
  index?: number;        // For arrayIndex
  filterKey?: string;    // For filteredArray: "url"
  filterValue?: string;  // For filteredArray: "https://..."
}

// NEW: Enhanced path segment with rich metadata for navigation and UX (Phase 1)
export interface EnhancedPathSegment {
  // Display
  label: string;           // Human-readable: "Extension (ethnicity)"
  raw: string;             // Original: "extension[url:...]"
  description?: string;    // Tooltip/long description

  // Navigation
  jumpKey: string;         // Unique key for tree navigation: "entry[2].resource.extension[url:...]"
  parentKey: string | null; // Parent's jumpKey for hierarchy
  depth: number;           // Depth level (0-indexed)

  // Status
  exists: boolean;         // Does this segment exist in JSON?
  status: SegmentStatusKind; // Detailed status
  isTarget: boolean;       // Is this the segment causing the error?
  isAncestor: boolean;     // Is this an ancestor of the target?

  // Type info
  kind: SegmentKind;       // Segment kind (property/array/filter)
  property: string;        // Property name without filters/indexes
  index?: number;          // Array index if applicable
  isDiscriminator: boolean; // Is this a discriminated/filtered array?
  isLeaf: boolean;         // Is this the final segment in path?

  // Value (optional)
  node?: any;              // Actual JSON node if exists

  // Discriminator details (optional)
  discriminator?: {
    key: string;           // Filter key: "url", "system", "QuestionCode"
    value: string;         // Filter value: "https://...", "SQ-001"
    displayValue: string;  // Shortened display version
    matchFound: boolean;   // Was matching element found in array?
    arrayLength?: number;  // How many elements in the array?
    matchIndex?: number;   // Index of matching element if found
    isNested: boolean;     // Is this a nested discriminator?
  };
}

// LEGACY: SegmentStatus interface kept for backward compatibility
// @deprecated Use EnhancedPathSegment instead
export interface SegmentStatus {
  segment: PathSegment;
  status: SegmentStatusKind; // Now using enum
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

    // CRITICAL FIX 3: Use shared navigation logic
    const result = navigateSegment(currentNode, segment);
    
    if (segment.kind === 'property') {
      // Simple property access
      if (result.exists) {
        status = {
          segment,
          status: SegmentStatusKind.EXISTS,
          node: result.node,
        };
        currentNode = result.node;
      } else {
        status = {
          segment,
          status: SegmentStatusKind.MISSING_PROPERTY,
        };
      }
    } else if (segment.kind === 'arrayIndex') {
      // Array with index
      if (currentNode === null || currentNode === undefined || typeof currentNode !== 'object') {
        status = {
          segment,
          status: SegmentStatusKind.MISSING_ARRAY,
        };
      } else if (!result.array) {
        // Property is not an array
        status = {
          segment,
          status: SegmentStatusKind.MISSING_ARRAY,
        };
      } else if (!result.exists) {
        // CRITICAL FIX 2: Index out of range (includes negative)
        status = {
          segment,
          status: SegmentStatusKind.INDEX_OUT_OF_RANGE,
          node: result.array, // Store array for context
        };
      } else {
        status = {
          segment,
          status: SegmentStatusKind.EXISTS,
          node: result.node,
        };
        currentNode = result.node;
      }
    } else if (segment.kind === 'filteredArray') {
      // Filtered array
      if (currentNode === null || currentNode === undefined || typeof currentNode !== 'object') {
        status = {
          segment,
          status: SegmentStatusKind.MISSING_ARRAY,
        };
      } else if (!result.array) {
        // Property is not an array
        status = {
          segment,
          status: SegmentStatusKind.MISSING_ARRAY,
        };
      } else if (result.exists) {
        status = {
          segment,
          status: SegmentStatusKind.EXISTS,
          node: result.node,
        };
        currentNode = result.node;
      } else {
        // No matching element found
        status = {
          segment,
          status: SegmentStatusKind.FILTER_NO_MATCH,
          node: result.array, // Store array for context
        };
      }
    } else {
      // Unknown segment kind
      status = {
        segment,
        status: SegmentStatusKind.MISSING_PROPERTY,
      };
    }

    statuses.push(status);

    // Track first non-existing index
    if (firstNonExistingIndex === -1 && status.status !== SegmentStatusKind.EXISTS) {
      firstNonExistingIndex = i;
    }

    // Stop traversal if current segment doesn't exist
    if (status.status !== SegmentStatusKind.EXISTS) {
      // Mark all remaining segments as missing parents
      for (let j = i + 1; j < segments.length; j++) {
        statuses.push({
          segment: segments[j],
          status: SegmentStatusKind.MISSING_PROPERTY,
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
    exists: ss.status === SegmentStatusKind.EXISTS,
    segment: ss.segment.raw,
    nodeValue: ss.node,
    isMissingParent: idx > 0 && segmentStatuses[idx - 1].status !== SegmentStatusKind.EXISTS,
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

// ============================================================================
// NEW: Phase 1 - Enhanced Path Segment Builders
// ============================================================================

/**
 * Navigate a single segment with unified logic (CRITICAL FIX 3)
 * Shared by both resolvePathSegments and navigateToNode
 * 
 * @returns { node: any | null, exists: boolean, array?: any[] }
 */
function navigateSegment(
  currentNode: any,
  segment: PathSegment
): { node: any | null; exists: boolean; array?: any[] } {
  // Check for null/undefined/non-object early
  if (currentNode === null || currentNode === undefined || typeof currentNode !== 'object') {
    return { node: null, exists: false };
  }

  if (segment.kind === 'property') {
    // Simple property access
    if (segment.property in currentNode) {
      return { node: currentNode[segment.property], exists: true };
    } else {
      return { node: null, exists: false };
    }
  } else if (segment.kind === 'arrayIndex') {
    // Array with index
    const array = currentNode[segment.property];
    if (!Array.isArray(array)) {
      return { node: null, exists: false };
    }
    
    // CRITICAL FIX 2: Negative index guard
    if (segment.index === undefined || segment.index < 0 || segment.index >= array.length) {
      return { node: null, exists: false, array };
    }
    
    return { node: array[segment.index], exists: true, array };
  } else if (segment.kind === 'filteredArray') {
    // Filtered array: extension[url:...]
    const array = currentNode[segment.property];
    if (!Array.isArray(array)) {
      return { node: null, exists: false };
    }
    
    // Find matching element
    const matchedElement = array.find(
      (item) => item?.[segment.filterKey!] === segment.filterValue
    );
    
    if (matchedElement) {
      return { node: matchedElement, exists: true, array };
    } else {
      return { node: null, exists: false, array };
    }
  }
  
  // Unknown segment kind
  return { node: null, exists: false };
}

/**
 * Navigate safely to a node in JSON tree, with circular reference protection
 * @param root - Root JSON object
 * @param segments - Parsed path segments
 * @param upToDepth - Stop at this depth (0-indexed, inclusive)
 * @returns The node at the specified depth, or null if not found
 */
export function navigateToNode(
  root: any,
  segments: PathSegment[],
  upToDepth: number
): any | null {
  const visited = new Set<any>();
  let currentNode = root;

  for (let i = 0; i <= upToDepth && i < segments.length; i++) {
    // Early exit for null/undefined
    if (currentNode === null || currentNode === undefined) {
      return null;
    }

    // Circular reference detection (CRITICAL FIX 3: Check BEFORE navigation)
    if (typeof currentNode === 'object' && visited.has(currentNode)) {
      console.warn('Circular reference detected in navigateToNode');
      return null;
    }
    if (typeof currentNode === 'object') {
      visited.add(currentNode);
    }

    const segment = segments[i];
    
    // CRITICAL FIX 3: Use shared navigation logic
    const result = navigateSegment(currentNode, segment);
    if (!result.exists) {
      return null;
    }
    
    currentNode = result.node;
  }

  return currentNode;
}

/**
 * Build a human-readable label from a path segment
 * Examples:
 * - "extension[url:ethnicity]" → "Extension (ethnicity)"
 * - "identifier[system:urn:oid:2.16.840...]" → "Identifier (urn:oid:...)"
 * - "coding[0]" → "Coding [0]"
 * - "name" → "Name"
 */
function buildLabelFromSegment(segment: PathSegment): string {
  // Capitalize property name
  const capitalizedProperty = segment.property.charAt(0).toUpperCase() + segment.property.slice(1);

  if (segment.kind === 'filteredArray' && segment.filterKey && segment.filterValue) {
    // For discriminators, extract a short display value
    const displayValue = shortenDiscriminatorValue(segment.filterValue);
    return `${capitalizedProperty} (${displayValue})`;
  } else if (segment.kind === 'arrayIndex' && segment.index !== undefined) {
    return `${capitalizedProperty} [${segment.index}]`;
  } else {
    return capitalizedProperty;
  }
}

/**
 * Shorten a discriminator value for display
 * Examples:
 * - "https://fhir.synapxe.sg/StructureDefinition/ethnicity" → "ethnicity"
 * - "urn:oid:2.16.840.1.113883.2.1.4.1" → "urn:oid:..."
 * - "SQ-001" → "SQ-001"
 */
function shortenDiscriminatorValue(value: string): string {
  // If it's a URL, extract the last segment
  if (value.startsWith('http://') || value.startsWith('https://')) {
    const parts = value.split('/');
    return parts[parts.length - 1] || value;
  }

  // If it's a long URN, truncate
  if (value.startsWith('urn:') && value.length > 30) {
    return value.substring(0, 27) + '...';
  }

  // Otherwise return as-is
  return value;
}

/**
 * Build a unique jumpKey for tree navigation
 * @param basePath - Base path (e.g., "entry[2].resource")
 * @param segments - Segments up to current depth
 * @returns A unique key like "entry[2].resource.extension[url:ethnicity].valueCodeableConcept"
 */
function buildJumpKey(basePath: string, segments: PathSegment[]): string {
  const segmentPaths = segments.map((seg) => seg.raw);
  if (basePath) {
    return `${basePath}.${segmentPaths.join('.')}`;
  }
  return segmentPaths.join('.');
}

/**
 * Build discriminator info from a filtered array segment
 * CRITICAL FIX 1: Correctly handle parentArray and matchedNode
 */
function buildDiscriminatorInfo(
  segment: PathSegment,
  status: SegmentStatusKind,
  matchedNode: any | null,
  parentArray: any[] | null,
  ancestorSegments: PathSegment[]
): EnhancedPathSegment['discriminator'] | undefined {
  if (segment.kind !== 'filteredArray' || !segment.filterKey || !segment.filterValue) {
    return undefined;
  }

  const matchFound = status === SegmentStatusKind.EXISTS;
  
  // CRITICAL FIX 1: arrayLength from parentArray, not node
  const arrayLength = parentArray ? parentArray.length : undefined;
  
  // CRITICAL FIX 1: matchIndex from indexOf in parentArray
  let matchIndex: number | undefined;
  if (matchFound && parentArray && matchedNode) {
    matchIndex = parentArray.indexOf(matchedNode);
    if (matchIndex === -1) matchIndex = undefined; // Shouldn't happen, but safety
  }
  
  // Better isNested detection: check if any ancestor is also a filteredArray
  const isNested = ancestorSegments.some(ancestorSeg => ancestorSeg.kind === 'filteredArray');

  return {
    key: segment.filterKey,
    value: segment.filterValue,
    displayValue: shortenDiscriminatorValue(segment.filterValue),
    matchFound,
    arrayLength,
    matchIndex,
    isNested,
  };
}

/**
 * NEW: Build enhanced path segments with rich metadata
 * This is the main function to use for new code
 * 
 * @param root - Root JSON object (e.g., resource or full Bundle)
 * @param segments - Parsed path segments from parseFieldPath()
 * @param basePath - Optional base path for building jumpKeys (e.g., "Bundle.entry[2].resource")
 * @returns Array of EnhancedPathSegment with full metadata
 */
export function buildEnhancedPathSegments(
  root: any,
  segments: PathSegment[],
  basePath: string = ''
): EnhancedPathSegment[] {
  const { statuses } = resolvePathSegments(root, segments);
  const enhanced: EnhancedPathSegment[] = [];

  for (let i = 0; i < statuses.length; i++) {
    const segStatus = statuses[i];
    const segment = segStatus.segment;
    const isLastSegment = i === statuses.length - 1;
    const segmentsUpToCurrent = segments.slice(0, i + 1);
    
    // Build label with safe fallback
    const label = buildLabelFromSegment(segment) || segment.raw;
    
    // Build jumpKey with safe fallback
    const jumpKey = buildJumpKey(basePath, segmentsUpToCurrent) || segment.raw;
    
    // Build parentKey with safe fallback (null for depth=0)
    const parentKey = i > 0 ? (buildJumpKey(basePath, segments.slice(0, i)) || null) : null;

    // CRITICAL FIX 1: Determine parentArray and matchedNode for discriminator
    let parentArray: any[] | null = null;
    let matchedNode: any | null = null;
    
    if (segment.kind === 'filteredArray') {
      if (segStatus.status === SegmentStatusKind.EXISTS) {
        // node is the matched element, need to get parent array
        matchedNode = segStatus.node;
        // Navigate to parent to get the array
        if (i > 0) {
          const parentNode = navigateToNode(root, segments, i - 1);
          if (parentNode && typeof parentNode === 'object') {
            const array = parentNode[segment.property];
            if (Array.isArray(array)) {
              parentArray = array;
            }
          }
        } else {
          // At root level, get array directly
          if (root && typeof root === 'object') {
            const array = root[segment.property];
            if (Array.isArray(array)) {
              parentArray = array;
            }
          }
        }
      } else if (segStatus.status === SegmentStatusKind.FILTER_NO_MATCH) {
        // node is the parent array
        if (Array.isArray(segStatus.node)) {
          parentArray = segStatus.node;
        }
      }
    }

    const enhancedSeg: EnhancedPathSegment = {
      // Display
      label,
      raw: segment.raw,
      description: undefined, // TODO: Could add more detailed descriptions

      // Navigation
      jumpKey,
      parentKey,
      depth: i,

      // Status
      exists: segStatus.status === SegmentStatusKind.EXISTS,
      status: segStatus.status,
      isTarget: isLastSegment,
      isAncestor: !isLastSegment,

      // Type
      kind: segment.kind === 'property' ? SegmentKind.PROPERTY :
            segment.kind === 'arrayIndex' ? SegmentKind.ARRAY_INDEX :
            SegmentKind.FILTERED_ARRAY,
      property: segment.property,
      index: segment.index,
      isDiscriminator: segment.kind === 'filteredArray',
      isLeaf: isLastSegment,

      // Value
      node: segStatus.node,

      // CRITICAL FIX 1: Pass correct parameters to buildDiscriminatorInfo
      discriminator:
        segment.kind === 'filteredArray'
          ? buildDiscriminatorInfo(
              segment,
              segStatus.status,
              matchedNode,
              parentArray,
              segments.slice(0, i) // ancestor segments for isNested detection
            )
          : undefined,
    };

    enhanced.push(enhancedSeg);
  }

  return enhanced;
}
