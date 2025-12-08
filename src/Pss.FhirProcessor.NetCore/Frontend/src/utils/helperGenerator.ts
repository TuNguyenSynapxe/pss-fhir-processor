/**
 * Helper Message Generator
 * Generates comprehensive, contextual help messages for validation errors
 */

import {
  parseFieldPath,
  analyzePath,
  resolvePathSegments,
  findFirstMissingSegment,
  buildPathString,
  PathSegment,
  PathStatus,
  SegmentStatus,
} from './pathParser';
import {
  getErrorTemplate,
  generateFixInstructions,
  FixInstructions,
  ErrorCodeTemplate,
} from './helperTemplates';
import { buildJsonSnippet, buildCompleteExample, formatJsonSnippet } from './snippetBuilder';

export interface ValidationError {
  code: string;
  message: string;
  fieldPath: string;
  scope: string;
  ruleType?: string;
  rule?: {
    path?: string;
    expectedValue?: string;
    expectedType?: string;
    pattern?: string;
    allowedValues?: string[];
    targetTypes?: string[];
    system?: string;
  };
  resourcePointer?: {
    entryIndex?: number;
    fullUrl?: string;
    resourceType?: string;
    resourceId?: string;
  };
  pathAnalysis?: {
    parentPathExists?: boolean;
    pathMismatchSegment?: string;
    mismatchDepth?: number;
  };
  context?: {
    resourceType?: string;
    screeningType?: string;
    questionCode?: string;
    questionDisplay?: string;
    allowedAnswers?: string[];
  };
}

export interface HelperMessage {
  whatThisMeans: ErrorCodeTemplate;
  location: LocationInfo;
  expected: ExpectedInfo;
  howToFix: FixInstructions;
  exampleSnippet: string;
  pathBreakdown: SegmentStatus[]; // Changed from PathStatus[] to SegmentStatus[] for detailed analysis
  completeExample?: string;
}

export interface LocationInfo {
  entryIndex?: number;
  resourceType?: string;
  breadcrumb: string[];
  fullPath: string;
}

export interface ExpectedInfo {
  type: 'value' | 'pattern' | 'codes' | 'reference' | 'format';
  value?: string;
  pattern?: string;
  allowedValues?: Array<{ code: string; display?: string }>;
  targetTypes?: string[];
  description?: string;
}

/**
 * Main function to generate helper message from error and JSON tree
 */
export function generateHelper(error: ValidationError, jsonTree: any): HelperMessage {
  // Parse the field path
  const segments = parseFieldPath(error.fieldPath);

  // Resolve path segments with detailed status tracking
  const { statuses: segmentStatuses, firstNonExistingIndex } = resolvePathSegments(
    navigateToResource(error, jsonTree),
    segments
  );

  // Generate location info
  const location = generateLocationInfo(error, segments);

  // Generate expected info
  const expected = generateExpectedInfo(error);

  // Generate fix instructions with detailed segment analysis
  const howToFix = generateFixInstructions(
    error.code,
    segmentStatuses,
    error.rule?.expectedValue,
    error.rule?.allowedValues
  );

  // Build example snippet
  const snippet = buildJsonSnippet(segments, firstNonExistingIndex, {
    expectedValue: error.rule?.expectedValue,
    allowedValues: error.rule?.allowedValues,
    expectedType: error.rule?.expectedType,
    pattern: error.rule?.pattern,
  });

  // Build complete example for specific cases
  const completeExample = buildCompleteExample(error.code, segments, {
    expectedValue: error.rule?.expectedValue,
    allowedValues: error.rule?.allowedValues,
    expectedType: error.rule?.expectedType,
  });

  return {
    whatThisMeans: getErrorTemplate(error.code),
    location,
    expected,
    howToFix,
    exampleSnippet: formatJsonSnippet(snippet || {}),
    pathBreakdown: segmentStatuses,
    completeExample: completeExample ? formatJsonSnippet(completeExample) : undefined,
  };
}

/**
 * Navigate to the resource in JSON tree (handles Bundle structure)
 */
function navigateToResource(error: ValidationError, jsonTree: any): any {
  // If we have entry index, navigate to the resource first
  if (error.resourcePointer?.entryIndex !== undefined && jsonTree?.entry) {
    const entry = jsonTree.entry[error.resourcePointer.entryIndex];
    if (entry?.resource) {
      return entry.resource;
    }
  }
  // Return root if no navigation needed
  return jsonTree;
}

/**
 * Analyze path in JSON tree, handling Bundle structure (LEGACY)
 * @deprecated Use resolvePathSegments with navigateToResource instead
 */
function analyzePathInJson(
  error: ValidationError,
  jsonTree: any,
  segments: PathSegment[]
): PathStatus[] {
  const resource = navigateToResource(error, jsonTree);
  return analyzePath(resource, segments);
}

/**
 * Generate location information with breadcrumb
 */
function generateLocationInfo(error: ValidationError, segments: PathSegment[]): LocationInfo {
  const breadcrumb: string[] = [];

  // Add entry index if available
  if (error.resourcePointer?.entryIndex !== undefined) {
    breadcrumb.push(`Entry #${error.resourcePointer.entryIndex}`);
  }

  // Add resource type
  if (error.resourcePointer?.resourceType || error.scope) {
    breadcrumb.push(error.resourcePointer?.resourceType || error.scope);
  }

  // Add path segments
  segments.forEach((segment) => {
    breadcrumb.push(formatSegmentForBreadcrumb(segment));
  });

  return {
    entryIndex: error.resourcePointer?.entryIndex,
    resourceType: error.resourcePointer?.resourceType || error.scope,
    breadcrumb,
    fullPath: error.fieldPath,
  };
}

/**
 * Format segment for breadcrumb display
 */
function formatSegmentForBreadcrumb(segment: PathSegment): string {
  // Just return the raw segment text which already has proper formatting
  return segment.raw;
}

/**
 * Generate expected information based on rule type
 */
function generateExpectedInfo(error: ValidationError): ExpectedInfo {
  const rule = error.rule;

  // Fixed value
  if (rule?.expectedValue) {
    return {
      type: 'value',
      value: rule.expectedValue,
      description: `The field must have the exact value: "${rule.expectedValue}"`,
    };
  }

  // Pattern (regex)
  if (rule?.pattern) {
    return {
      type: 'pattern',
      pattern: rule.pattern,
      description: `The field value must match the regular expression pattern: ${rule.pattern}`,
    };
  }

  // Allowed values (codes)
  if (rule?.allowedValues && rule.allowedValues.length > 0) {
    return {
      type: 'codes',
      allowedValues: rule.allowedValues.map((v) => ({ code: v })),
      description: 'The field must be one of the following allowed values:',
    };
  }

  // Allowed values from context (for CodesMaster)
  if (error.context?.allowedAnswers && error.context.allowedAnswers.length > 0) {
    return {
      type: 'codes',
      allowedValues: error.context.allowedAnswers.map((v) => ({ code: v })),
      description: `Allowed answers for question "${error.context.questionCode}":`,
    };
  }

  // Reference types
  if (rule?.targetTypes && rule.targetTypes.length > 0) {
    return {
      type: 'reference',
      targetTypes: rule.targetTypes,
      description: `The reference must point to one of these resource types: ${rule.targetTypes.join(', ')}`,
    };
  }

  // Expected type
  if (rule?.expectedType) {
    return {
      type: 'format',
      value: rule.expectedType,
      description: `The field must be of type: ${rule.expectedType}`,
    };
  }

  // Default
  return {
    type: 'format',
    description: 'See the validation message for specific requirements.',
  };
}

/**
 * Get resource from JSON tree by entry index
 */
export function getResourceByEntryIndex(jsonTree: any, entryIndex?: number): any {
  if (entryIndex === undefined || !jsonTree?.entry) {
    return null;
  }

  const entry = jsonTree.entry[entryIndex];
  return entry?.resource || null;
}

/**
 * Navigate to a specific path in JSON and return the parent node
 */
export function navigateToParent(json: any, segments: PathSegment[]): any {
  if (segments.length === 0) return json;

  const parentSegments = segments.slice(0, -1);
  let current = json;

  for (const segment of parentSegments) {
    if (!current || typeof current !== 'object') return null;

    switch (segment.kind) {
      case 'property':
        current = current[segment.property];
        break;
      case 'arrayIndex':
        if (Array.isArray(current[segment.property]) && segment.index !== undefined) {
          current = current[segment.property][segment.index];
        } else {
          return null;
        }
        break;
      case 'filteredArray':
        if (Array.isArray(current[segment.property])) {
          current = current[segment.property].find(
            (item: any) =>
              item &&
              typeof item === 'object' &&
              item[segment.filterKey!] === segment.filterValue
          );
        } else {
          return null;
        }
        break;
    }

    if (!current) return null;
  }

  return current;
}
