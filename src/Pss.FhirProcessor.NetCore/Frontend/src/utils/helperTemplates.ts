/**
 * Helper message templates and descriptions for different error codes
 */

export interface ErrorCodeTemplate {
  whatThisMeans: string;
  commonCauses: string[];
  documentationLink?: string;
}

export const ERROR_CODE_TEMPLATES: Record<string, ErrorCodeTemplate> = {
  MANDATORY_MISSING: {
    whatThisMeans:
      'A required field defined by the PSS FHIR specification is missing from your resource. This field must be present for the resource to be valid.',
    commonCauses: [
      'The field was not included in the JSON structure',
      'The field exists but has a null or empty value',
      'Parent object or array is missing',
      'Array index is out of bounds',
    ],
  },

  MISSING_PATIENT: {
    whatThisMeans:
      'The Bundle must contain at least one Patient resource. Patient information is mandatory for PSS screening records.',
    commonCauses: [
      'No Patient resource included in Bundle.entry array',
      'Patient resource exists but resourceType is incorrect',
    ],
  },

  MISSING_ENCOUNTER: {
    whatThisMeans:
      'The Bundle must contain at least one Encounter resource. Encounter represents the screening event.',
    commonCauses: [
      'No Encounter resource included in Bundle.entry array',
      'Encounter resource exists but resourceType is incorrect',
    ],
  },

  MISSING_SCREENING_TYPE: {
    whatThisMeans:
      'The Bundle must contain Observation resources for all required screening types (HS, OS, VS).',
    commonCauses: [
      'Missing Observation resource with the required screening-type code',
      'Observation exists but code.coding.code does not match expected type',
    ],
  },

  FIXED_VALUE_MISMATCH: {
    whatThisMeans:
      'The field value must exactly match a specific value defined by the PSS specification. This is typically used for system URLs, coding systems, or status codes.',
    commonCauses: [
      'Wrong value provided',
      'Typo in the value',
      'Wrong CodeSystem URL',
      'Case sensitivity mismatch',
    ],
  },

  INVALID_CODE: {
    whatThisMeans:
      'The code value is not in the allowed list defined by the PSS CodeSystem. Only specific codes are valid for this field.',
    commonCauses: [
      'Code does not exist in the CodeSystem',
      'Typo in the code value',
      'Using display value instead of code',
      'Wrong CodeSystem being referenced',
    ],
  },

  TYPE_MISMATCH: {
    whatThisMeans:
      'The datatype of the field does not match the expected FHIR type. Each field must conform to specific data formats.',
    commonCauses: [
      'String provided where boolean expected',
      'Invalid date format (must be YYYY-MM-DD)',
      'Invalid datetime format (must be ISO-8601)',
      'Invalid GUID format',
    ],
  },

  REGEX_INVALID_NRIC: {
    whatThisMeans:
      'NRIC format is invalid. Singapore NRIC must start with S/T/F/G, followed by 7 digits, and end with an alphabet checksum.',
    commonCauses: [
      'Incorrect NRIC format',
      'Missing checksum letter',
      'Wrong number of digits',
      'Invalid prefix letter',
    ],
  },

  REGEX_INVALID_POSTAL: {
    whatThisMeans: 'Singapore postal code must be exactly 6 digits.',
    commonCauses: ['Less than or more than 6 digits', 'Contains non-numeric characters'],
  },

  TYPE_INVALID_FULLURL: {
    whatThisMeans:
      'Bundle.entry.fullUrl must follow the format urn:uuid:<GUID>. This is required for all resources in the Bundle.',
    commonCauses: [
      'Missing urn:uuid: prefix',
      'Invalid GUID format',
      'Using relative URL instead of URN',
    ],
  },

  ID_FULLURL_MISMATCH: {
    whatThisMeans:
      'The resource.id must exactly match the GUID portion of entry.fullUrl. This ensures consistency within the Bundle.',
    commonCauses: [
      'resource.id has different GUID than fullUrl',
      'Typo in one of the GUIDs',
      'Copy-paste error when creating resources',
    ],
  },

  REFERENCE_INVALID: {
    whatThisMeans:
      'A reference field must point to a valid resource that exists in the Bundle and has the correct resource type.',
    commonCauses: [
      'Referenced resource not included in Bundle.entry',
      'Wrong resource ID in the reference',
      'Reference format incorrect (should be ResourceType/id or urn:uuid:guid)',
      'Referenced resource has wrong resourceType',
      'Typo in the resource ID or reference path',
    ],
  },

  INVALID_ANSWER_VALUE: {
    whatThisMeans:
      'The answer value in the Observation component does not match the allowed answers defined for this screening question.',
    commonCauses: [
      'Answer text does not exactly match allowed values',
      'Typo in answer text',
      'Using abbreviated answer instead of full text',
      'Wrong question code mapping',
    ],
  },

  REFERENCE_NOT_FOUND: {
    whatThisMeans: 'The referenced resource does not exist in the Bundle.',
    commonCauses: [
      'Resource was not included in Bundle.entry',
      'Wrong resource ID in reference',
      'Referenced resource has wrong resourceType',
    ],
  },

  REFERENCE_TYPE_MISMATCH: {
    whatThisMeans: 'The referenced resource has the wrong resource type.',
    commonCauses: [
      'Referencing Organization when Patient expected',
      'Wrong resource type in reference path',
    ],
  },
};

/**
 * Get template for error code, with fallback
 */
export function getErrorTemplate(errorCode: string): ErrorCodeTemplate {
  return (
    ERROR_CODE_TEMPLATES[errorCode] || {
      whatThisMeans: 'A validation error occurred in your FHIR resource.',
      commonCauses: ['The field does not meet the validation requirements'],
    }
  );
}

/**
 * Generate "How to Fix" instructions based on error type and path analysis
 */
import { SegmentStatus } from './pathParser';

// NEW: FixStep with optional segmentKey for hover synchronization
export interface FixStep {
  text: string;           // The instruction text
  segmentKey?: string;    // Optional jumpKey for highlighting corresponding path segment
  targetPath?: string;    // Optional raw path segment (for mapping to segmentKey)
}

export interface FixInstructions {
  steps: FixStep[];       // Changed from string[] to FixStep[] for hover sync
  needsParentCreation: boolean;
  missingSegments: string[];
  scenario?: 'value-mismatch' | 'filter-no-match' | 'index-out-of-range' | 'parent-missing' | 'leaf-missing';
}

export function generateFixInstructions(
  errorCode: string,
  segmentStatuses: SegmentStatus[],
  expectedValue?: string,
  allowedValues?: string[]
): FixInstructions {
  const instructions: FixInstructions = {
    steps: [],
    needsParentCreation: false,
    missingSegments: [],
  };

  // Helper function to create FixStep from text
  const createStep = (text: string, targetPath?: string): FixStep => ({
    text,
    targetPath, // Will be mapped to segmentKey later in helperGenerator
  });

  // Find first non-existing segment
  const firstNonExistingIndex = segmentStatuses.findIndex((s) => s.status !== 'EXISTS');

  // Case 1: All segments exist - value-based error
  if (firstNonExistingIndex === -1) {
    instructions.scenario = 'value-mismatch';
    
    // Get target path (last segment)
    const targetPath = segmentStatuses.length > 0 
      ? segmentStatuses[segmentStatuses.length - 1].segment.raw 
      : undefined;
    
    // Special handling for REFERENCE errors
    if (errorCode.includes('REFERENCE')) {
      instructions.steps.push(createStep('This is a reference validation error. The reference value exists but is invalid.', targetPath));
      instructions.steps.push(createStep('Step 1: Locate the reference field in your JSON (e.g., subject.reference, encounter.reference).', targetPath));
      instructions.steps.push(createStep('Step 2: Check the reference value format (should be "ResourceType/id" or "urn:uuid:guid").'));
      
      if (errorCode === 'REFERENCE_NOT_FOUND') {
        instructions.steps.push(createStep('Step 3: Navigate to Bundle.entry array and verify the referenced resource exists.'));
        instructions.steps.push(createStep('Step 4: Check that entry.resource.id matches the ID in your reference.'));
        instructions.steps.push(createStep('Option A: Add the missing resource to Bundle.entry array.'));
        instructions.steps.push(createStep('Option B: Correct the reference to point to an existing resource.'));
      } else if (errorCode === 'REFERENCE_TYPE_MISMATCH') {
        instructions.steps.push(createStep('Step 3: Navigate to the referenced resource in Bundle.entry array.'));
        instructions.steps.push(createStep('Step 4: Check the resourceType of the referenced resource.'));
        instructions.steps.push(createStep('Step 5: See "Expected" section for allowed resource types.'));
        instructions.steps.push(createStep('Option A: Change the reference to point to a resource with the correct type.'));
        instructions.steps.push(createStep('Option B: Change the resourceType of the target resource (if appropriate).'));
      } else if (errorCode === 'INVALID_REFERENCE_FORMAT') {
        instructions.steps.push(createStep('Step 3: Ensure format is either "ResourceType/id" (e.g., "Patient/p123") or "urn:uuid:guid".'));
        instructions.steps.push(createStep('Step 4: Remove any extra characters, spaces, or incorrect separators.'));
      } else {
        instructions.steps.push(createStep('Step 3: Verify the reference points to an existing resource with the correct type.'));
      }
      return instructions;
    }
    
    if (errorCode.includes('MISMATCH') || errorCode.includes('INVALID')) {
      instructions.steps.push(createStep('Locate the field in your JSON at the path shown above.', targetPath));
      if (expectedValue) {
        instructions.steps.push(createStep(`Change the value to: "${expectedValue}"`, targetPath));
      } else if (allowedValues && allowedValues.length > 0) {
        instructions.steps.push(createStep(
          'Change the value to one of the allowed values listed in the "Expected" section.',
          targetPath
        ));
      } else {
        instructions.steps.push(createStep('Correct the value to match the expected format.', targetPath));
      }
    } else if (errorCode === 'ID_FULLURL_MISMATCH') {
      instructions.steps.push(createStep('Locate the resource entry in Bundle.entry array.'));
      instructions.steps.push(createStep(
        'Ensure entry.fullUrl is in format: urn:uuid:XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX'
      ));
      instructions.steps.push(createStep(
        'Ensure resource.id matches the GUID portion (everything after "urn:uuid:")'
      ));
      instructions.steps.push(createStep('Both values must be identical.'));
    }
    return instructions;
  }

  // Case 2+: At least one segment doesn't exist
  const failingSegment = segmentStatuses[firstNonExistingIndex];
  instructions.missingSegments = segmentStatuses
    .slice(firstNonExistingIndex)
    .map((s) => s.segment.raw);

  // Case 2: Filter didn't match any array element
  if (failingSegment.status === 'FILTER_NO_MATCH') {
    instructions.scenario = 'filter-no-match';
    instructions.needsParentCreation = false;
    
    const filterKey = failingSegment.segment.filterKey || 'key';
    const filterValue = failingSegment.segment.filterValue || 'value';
    const arrayName = failingSegment.segment.property;
    const targetFilterPath = failingSegment.segment.raw;
    
    instructions.steps.push(createStep(
      `The array "${arrayName}" exists, but no element has ${filterKey}="${filterValue}".`,
      targetFilterPath
    ));
    
    // Build parent path (everything before the filtered segment)
    const parentPathSegments = segmentStatuses.slice(0, firstNonExistingIndex);
    const parentPathStr = parentPathSegments.map((s) => s.segment.raw).join('.');
    const parentTargetPath = parentPathSegments.length > 0 
      ? parentPathSegments[parentPathSegments.length - 1].segment.raw 
      : undefined;
    
    if (parentPathStr) {
      instructions.steps.push(createStep(`Navigate to: ${parentPathStr}`, parentTargetPath));
    }
    instructions.steps.push(createStep(`Locate the "${arrayName}" array.`, targetFilterPath));
    instructions.steps.push(createStep(
      `Find or create an array element where "${filterKey}" = "${filterValue}".`,
      targetFilterPath
    ));
    
    if (firstNonExistingIndex < segmentStatuses.length - 1) {
      const remainingPathSegments = segmentStatuses.slice(firstNonExistingIndex + 1);
      const remainingPath = remainingPathSegments.map((s) => s.segment.raw).join('.');
      const remainingTargetPath = remainingPathSegments.length > 0
        ? remainingPathSegments[remainingPathSegments.length - 1].segment.raw
        : undefined;
      instructions.steps.push(createStep(
        `Once the filter matches, add the remaining path: ${remainingPath}`,
        remainingTargetPath
      ));
    }
    
    instructions.steps.push(createStep('See the "Expected" section for the correct filter value.'));
    return instructions;
  }

  // Case 3: Array index out of range
  if (failingSegment.status === 'INDEX_OUT_OF_RANGE') {
    instructions.scenario = 'index-out-of-range';
    instructions.needsParentCreation = false;
    
    const arrayName = failingSegment.segment.property;
    const requestedIndex = failingSegment.segment.index;
    const targetPath = failingSegment.segment.raw;
    
    instructions.steps.push(createStep(
      `The array "${arrayName}" exists, but index [${requestedIndex}] is out of range.`,
      targetPath
    ));
    instructions.steps.push(createStep(`Option 1: Add a new element to the "${arrayName}" array.`, targetPath));
    instructions.steps.push(createStep(
      `Option 2: Use an existing index (the array currently has fewer than ${(requestedIndex || 0) + 1} elements).`,
      targetPath
    ));
    instructions.steps.push(createStep('See the "Example JSON Snippet" for the structure to add.'));
    return instructions;
  }

  // Case 4 & 5: Structural missing (MISSING_ARRAY or MISSING_PROPERTY)
  const isLeafOnly = firstNonExistingIndex === segmentStatuses.length - 1;
  const targetPath = failingSegment.segment.raw;
  
  if (isLeafOnly) {
    // Only the final field is missing
    instructions.scenario = 'leaf-missing';
    instructions.needsParentCreation = false;
    
    const parentPath = firstNonExistingIndex > 0
      ? segmentStatuses[firstNonExistingIndex - 1].segment.raw
      : undefined;
    
    instructions.steps.push(createStep('The parent structure exists, but the final field is missing.'));
    instructions.steps.push(createStep('Locate the parent object in your JSON (see path breakdown).', parentPath));
    
    if (expectedValue) {
      instructions.steps.push(createStep(`Add the field "${failingSegment.segment.property}" with value: "${expectedValue}"`, targetPath));
    } else if (allowedValues && allowedValues.length > 0) {
      instructions.steps.push(createStep(
        `Add the field "${failingSegment.segment.property}" with one of the allowed values from the "Expected" section.`,
        targetPath
      ));
    } else {
      instructions.steps.push(createStep(
        `Add the field "${failingSegment.segment.property}" with the appropriate value (see "Example JSON Snippet").`,
        targetPath
      ));
    }
  } else {
    // One or more parent levels are missing
    instructions.scenario = 'parent-missing';
    instructions.needsParentCreation = true;
    
    const missingCount = instructions.missingSegments.length;
    const firstMissingPath = targetPath;
    const lastExistingPath = firstNonExistingIndex > 0
      ? segmentStatuses[firstNonExistingIndex - 1].segment.raw
      : undefined;
    
    instructions.steps.push(createStep(
      `The parent structure is missing. You need to create ${missingCount} missing level(s).`,
      firstMissingPath
    ));
    instructions.steps.push(createStep('Locate the last existing parent in your JSON (see path breakdown).', lastExistingPath));
    instructions.steps.push(createStep(
      'Add the missing parent structure as shown in the "Example JSON Snippet" section.',
      firstMissingPath
    ));
    instructions.steps.push(createStep('Copy the example and paste it into the correct location.'));
  }

  return instructions;
}
