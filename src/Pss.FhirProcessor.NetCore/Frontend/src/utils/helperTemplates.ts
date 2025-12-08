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

  REF_SUBJECT_INVALID: {
    whatThisMeans:
      'The reference must point to a valid Patient resource that exists in the Bundle.',
    commonCauses: [
      'Referenced Patient resource not in Bundle',
      'Wrong resource ID in reference',
      'Reference format incorrect (should be Patient/<id> or urn:uuid:<guid>)',
    ],
  },

  REF_OBS_SUBJECT_INVALID: {
    whatThisMeans:
      'Observation.subject.reference must point to a valid Patient resource in the Bundle.',
    commonCauses: [
      'Referenced Patient not in Bundle',
      'Wrong patient ID',
      'Reference format incorrect',
    ],
  },

  REF_OBS_ENCOUNTER_INVALID: {
    whatThisMeans:
      'Observation.encounter.reference must point to a valid Encounter resource in the Bundle.',
    commonCauses: [
      'Referenced Encounter not in Bundle',
      'Wrong encounter ID',
      'Reference format incorrect',
    ],
  },

  REF_OBS_PERFORMER_INVALID: {
    whatThisMeans:
      'Observation.performer.reference must point to a valid Organization resource in the Bundle.',
    commonCauses: [
      'Referenced Organization not in Bundle',
      'Wrong organization ID',
      'Reference format incorrect',
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

export interface FixInstructions {
  steps: string[];
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

  // Find first non-existing segment
  const firstNonExistingIndex = segmentStatuses.findIndex((s) => s.status !== 'EXISTS');

  // Case 1: All segments exist - value-based error
  if (firstNonExistingIndex === -1) {
    instructions.scenario = 'value-mismatch';
    
    if (errorCode.includes('MISMATCH') || errorCode.includes('INVALID')) {
      instructions.steps.push('Locate the field in your JSON at the path shown above.');
      if (expectedValue) {
        instructions.steps.push(`Change the value to: "${expectedValue}"`);
      } else if (allowedValues && allowedValues.length > 0) {
        instructions.steps.push(
          'Change the value to one of the allowed values listed in the "Expected" section.'
        );
      } else {
        instructions.steps.push('Correct the value to match the expected format.');
      }
    } else if (errorCode === 'ID_FULLURL_MISMATCH') {
      instructions.steps.push('Locate the resource entry in Bundle.entry array.');
      instructions.steps.push(
        'Ensure entry.fullUrl is in format: urn:uuid:XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX'
      );
      instructions.steps.push(
        'Ensure resource.id matches the GUID portion (everything after "urn:uuid:")'
      );
      instructions.steps.push('Both values must be identical.');
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
    
    instructions.steps.push(
      `The array "${arrayName}" exists, but no element has ${filterKey}="${filterValue}".`
    );
    
    // Build parent path (everything before the filtered segment)
    const parentPath = segmentStatuses
      .slice(0, firstNonExistingIndex)
      .map((s) => s.segment.raw)
      .join('.');
    
    if (parentPath) {
      instructions.steps.push(`Navigate to: ${parentPath}`);
    }
    instructions.steps.push(`Locate the "${arrayName}" array.`);
    instructions.steps.push(
      `Find or create an array element where "${filterKey}" = "${filterValue}".`
    );
    
    if (firstNonExistingIndex < segmentStatuses.length - 1) {
      const remainingPath = segmentStatuses
        .slice(firstNonExistingIndex + 1)
        .map((s) => s.segment.raw)
        .join('.');
      instructions.steps.push(
        `Once the filter matches, add the remaining path: ${remainingPath}`
      );
    }
    
    instructions.steps.push('See the "Expected" section for the correct filter value.');
    return instructions;
  }

  // Case 3: Array index out of range
  if (failingSegment.status === 'INDEX_OUT_OF_RANGE') {
    instructions.scenario = 'index-out-of-range';
    instructions.needsParentCreation = false;
    
    const arrayName = failingSegment.segment.property;
    const requestedIndex = failingSegment.segment.index;
    
    instructions.steps.push(
      `The array "${arrayName}" exists, but index [${requestedIndex}] is out of range.`
    );
    instructions.steps.push(`Option 1: Add a new element to the "${arrayName}" array.`);
    instructions.steps.push(
      `Option 2: Use an existing index (the array currently has fewer than ${(requestedIndex || 0) + 1} elements).`
    );
    instructions.steps.push('See the "Example JSON Snippet" for the structure to add.');
    return instructions;
  }

  // Case 4 & 5: Structural missing (MISSING_ARRAY or MISSING_PROPERTY)
  const isLeafOnly = firstNonExistingIndex === segmentStatuses.length - 1;
  
  if (isLeafOnly) {
    // Only the final field is missing
    instructions.scenario = 'leaf-missing';
    instructions.needsParentCreation = false;
    
    instructions.steps.push('The parent structure exists, but the final field is missing.');
    instructions.steps.push('Locate the parent object in your JSON (see path breakdown).');
    
    if (expectedValue) {
      instructions.steps.push(`Add the field "${failingSegment.segment.property}" with value: "${expectedValue}"`);
    } else if (allowedValues && allowedValues.length > 0) {
      instructions.steps.push(
        `Add the field "${failingSegment.segment.property}" with one of the allowed values from the "Expected" section.`
      );
    } else {
      instructions.steps.push(
        `Add the field "${failingSegment.segment.property}" with the appropriate value (see "Example JSON Snippet").`
      );
    }
  } else {
    // One or more parent levels are missing
    instructions.scenario = 'parent-missing';
    instructions.needsParentCreation = true;
    
    const missingCount = instructions.missingSegments.length;
    instructions.steps.push(
      `The parent structure is missing. You need to create ${missingCount} missing level(s).`
    );
    instructions.steps.push('Locate the last existing parent in your JSON (see path breakdown).');
    instructions.steps.push(
      'Add the missing parent structure as shown in the "Example JSON Snippet" section.'
    );
    instructions.steps.push('Copy the example and paste it into the correct location.');
  }

  return instructions;
}
