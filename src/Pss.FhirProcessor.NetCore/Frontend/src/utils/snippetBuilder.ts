/**
 * JSON Snippet Builder
 * Generates minimal valid JSON examples with proper parent context
 */

import { PathSegment, formatSegment } from './pathParser';

export interface SnippetOptions {
  expectedValue?: string;
  allowedValues?: string[];
  expectedType?: string;
  pattern?: string;
  isArray?: boolean;
}

/**
 * Build a minimal JSON snippet showing the missing structure
 * Only includes necessary parent nodes and the target field
 */
export function buildJsonSnippet(
  segments: PathSegment[],
  missingIndex: number,
  options: SnippetOptions = {}
): any {
  if (missingIndex < 0 || segments.length === 0) {
    return null;
  }

  const missingSegments = segments.slice(missingIndex);
  let snippet: any = buildLeafValue(segments[segments.length - 1], options);

  // Build from leaf to root
  for (let i = missingSegments.length - 2; i >= 0; i--) {
    const segment = missingSegments[i];
    snippet = wrapInParent(segment, snippet);
  }

  return snippet;
}

/**
 * Build the leaf value based on field type and expected value
 */
function buildLeafValue(leafSegment: PathSegment, options: SnippetOptions): any {
  const { expectedValue, allowedValues, expectedType } = options;

  // If expected value is provided, use it
  if (expectedValue !== undefined) {
    // Try to parse as boolean
    if (expectedValue === 'true') return true;
    if (expectedValue === 'false') return false;

    // Try to parse as number
    if (/^\d+$/.test(expectedValue)) {
      return parseInt(expectedValue, 10);
    }

    return expectedValue;
  }

  // If allowed values provided, show the first one as example
  if (allowedValues && allowedValues.length > 0) {
    return allowedValues[0];
  }

  // Generate example based on expected type
  if (expectedType) {
    return generateExampleByType(expectedType, leafSegment.property);
  }

  // Generate based on field name
  return generateExampleByFieldName(leafSegment.property);
}

/**
 * Wrap a value in its parent structure
 */
function wrapInParent(segment: PathSegment, childValue: any): any {
  switch (segment.kind) {
    case 'property':
      return { [segment.property]: childValue };

    case 'arrayIndex':
      return { [segment.property]: [childValue] };

    case 'filteredArray':
      // For filtered arrays, create an object with both the filter property and child
      const filterObj: any = {
        [segment.filterKey!]: segment.filterValue,
      };

      // If child is an object, merge it; otherwise wrap it
      if (typeof childValue === 'object' && !Array.isArray(childValue)) {
        return { [segment.property]: [{ ...filterObj, ...childValue }] };
      }
      return { [segment.property]: [filterObj] };

    default:
      return { [segment.property]: childValue };
  }
}

/**
 * Generate example value based on expected type
 */
function generateExampleByType(type: string, fieldName: string): any {
  switch (type.toLowerCase()) {
    case 'guid':
    case 'uuid':
      return '12345678-1234-1234-1234-123456789abc';

    case 'boolean':
      return true;

    case 'date':
      return '2024-01-15';

    case 'datetime':
      return '2024-01-15T10:30:00+08:00';

    case 'integer':
    case 'int':
      return 0;

    case 'decimal':
    case 'float':
      return 0.0;

    case 'code':
      return 'CODE';

    case 'string':
    default:
      return generateExampleByFieldName(fieldName);
  }
}

/**
 * Generate example value based on field name
 */
function generateExampleByFieldName(fieldName: string): any {
  const lowerName = fieldName.toLowerCase();

  if (lowerName.includes('system')) {
    return 'https://fhir.synapxe.sg/CodeSystem/example';
  }

  if (lowerName.includes('code')) {
    return 'EXAMPLE_CODE';
  }

  if (lowerName.includes('display')) {
    return 'Example Display Text';
  }

  if (lowerName.includes('value')) {
    return 'example-value';
  }

  if (lowerName.includes('reference')) {
    return 'Patient/12345678-1234-1234-1234-123456789abc';
  }

  if (lowerName.includes('url')) {
    return 'https://example.com';
  }

  if (lowerName.includes('id')) {
    return '12345678-1234-1234-1234-123456789abc';
  }

  if (lowerName.includes('status')) {
    return 'active';
  }

  if (lowerName.includes('name')) {
    return 'Example Name';
  }

  return 'example-value';
}

/**
 * Build a complete example for specific error codes
 */
export function buildCompleteExample(
  errorCode: string,
  segments: PathSegment[],
  options: SnippetOptions
): any {
  // Handle special cases for specific error codes
  if (errorCode === 'ID_FULLURL_MISMATCH') {
    return {
      entry: [
        {
          fullUrl: 'urn:uuid:12345678-1234-1234-1234-123456789abc',
          resource: {
            resourceType: 'Patient',
            id: '12345678-1234-1234-1234-123456789abc',
            // ... other fields
          },
        },
      ],
    };
  }

  // Handle coding structure
  if (segments.some((s) => s.property === 'coding')) {
    return buildCodingExample(segments, options);
  }

  // Handle extension structure
  if (segments.some((s) => s.property === 'extension')) {
    return buildExtensionExample(segments, options);
  }

  // Handle identifier structure
  if (segments.some((s) => s.property === 'identifier')) {
    return buildIdentifierExample(segments, options);
  }

  // Default: use minimal snippet
  return null;
}

/**
 * Build example for coding structures
 */
function buildCodingExample(segments: PathSegment[], options: SnippetOptions): any {
  const { expectedValue } = options;

  return {
    coding: [
      {
        system: 'https://fhir.synapxe.sg/CodeSystem/example',
        code: expectedValue || 'EXAMPLE_CODE',
        display: 'Example Display',
      },
    ],
  };
}

/**
 * Build example for extension structures
 */
function buildExtensionExample(segments: PathSegment[], options: SnippetOptions): any {
  const extensionSegment = segments.find((s) => s.kind === 'filteredArray' && s.property === 'extension');
  const url = extensionSegment?.filterValue || 'https://fhir.synapxe.sg/StructureDefinition/example';

  return {
    extension: [
      {
        url: url,
        valueCodeableConcept: {
          coding: [
            {
              system: 'https://fhir.synapxe.sg/CodeSystem/example',
              code: 'EXAMPLE',
              display: 'Example',
            },
          ],
        },
      },
    ],
  };
}

/**
 * Build example for identifier structures
 */
function buildIdentifierExample(segments: PathSegment[], options: SnippetOptions): any {
  const identifierSegment = segments.find((s) => s.kind === 'filteredArray' && s.property === 'identifier');
  const system = identifierSegment?.filterValue || 'https://fhir.synapxe.sg/NamingSystem/example';

  return {
    identifier: [
      {
        system: system,
        value: 'EXAMPLE123',
      },
    ],
  };
}

/**
 * Format JSON with syntax highlighting (returns formatted string)
 */
export function formatJsonSnippet(snippet: any): string {
  return JSON.stringify(snippet, null, 2);
}
