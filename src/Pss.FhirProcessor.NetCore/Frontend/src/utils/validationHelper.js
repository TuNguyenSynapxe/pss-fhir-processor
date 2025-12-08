/**
 * Validation Helper System
 * Generic, metadata-driven error helper for FHIR validation
 */

/**
 * Rule type to template mapping
 */
const helperTemplates = {
  Required: renderRequired,
  Regex: renderRegex,
  Type: renderType,
  FixedValue: renderFixedValue,
  FullUrlIdMatch: renderFullUrlMatch,
  Reference: renderReference,
  CodeSystem: renderCodeSystem,
  CodesMaster: renderCodesMaster,
  AllowedValues: renderAllowedValues,
};

/**
 * Parse discriminator from fieldPath segment like identifier[system:https://...]
 * @param {string} segment - Path segment potentially containing discriminator
 * @returns {Object|null} - { field, value, arrayName } or null
 */
function parseDiscriminator(segment) {
  // Match patterns like: identifier[system:value] or extension[url:value]
  const match = segment.match(/^([^\[]+)\[([^:]+):(.+)\]$/);
  if (match) {
    return {
      arrayName: match[1],
      field: match[2],
      value: match[3]
    };
  }
  return null;
}

/**
 * Detect discriminator parent mismatch scenario
 * @param {string} fieldPath - Full field path
 * @returns {Object|null} - Discriminator info and scenario type
 */
function detectDiscriminatorScenario(fieldPath) {
  if (!fieldPath) return null;
  
  // Split path and look for discriminator patterns
  const segments = fieldPath.split('.');
  
  for (let i = 0; i < segments.length; i++) {
    const segment = segments[i];
    const discriminator = parseDiscriminator(segment);
    
    if (discriminator) {
      // Found discriminator - check if there are child segments after it
      const hasChildSegments = i < segments.length - 1;
      
      return {
        discriminator,
        segmentIndex: i,
        hasChildSegments,
        childSegments: hasChildSegments ? segments.slice(i + 1) : []
      };
    }
  }
  
  return null;
}

/**
 * Main helper generator - called from UI components
 * @param {Object} error - Validation error with metadata
 * @returns {Object} Helper message with structured content
 */
export function generateHelper(error) {
  if (!error || !error.ruleType) {
    return createBasicHelper(error);
  }

  const template = helperTemplates[error.ruleType];
  if (!template) {
    return createBasicHelper(error);
  }

  return template(error, error.rule || {}, error.context || {}, error.resourcePointer || {});
}

/**
 * Fallback for errors without ruleType
 */
function createBasicHelper(error) {
  return {
    title: `Error: ${error?.code || 'Unknown'}`,
    description: error?.message || 'Validation failed',
    location: humanizePath(error?.fieldPath, error?.context),
    howToFix: ['Review and correct the field value'],
  };
}

/**
 * Template: Required field missing
 */
function renderRequired(error, rule, context, resourcePointer) {
  const fieldName = extractFieldName(error.fieldPath);
  const humanPath = humanizePath(error.scope, error.fieldPath, context);
  const breadcrumb = generateBreadcrumb(error.fieldPath, context);
  
  // Detect discriminator scenario first
  const discriminatorInfo = detectDiscriminatorScenario(error.fieldPath);
  
  // Determine fix scenario
  const pathAnalysis = error.pathAnalysis || {};
  let fixScenario;
  let discriminator = null;
  
  if (discriminatorInfo && discriminatorInfo.hasChildSegments) {
    // This is a missing child field inside a discriminated array element
    fixScenario = 'discriminatorParentMissing';
    discriminator = {
      field: discriminatorInfo.discriminator.field,
      value: discriminatorInfo.discriminator.value,
      arrayName: discriminatorInfo.discriminator.arrayName,
      childPath: discriminatorInfo.childSegments.join('.')
    };
  } else if (pathAnalysis.parentPathExists === false) {
    fixScenario = 'parentMissing';
  } else {
    fixScenario = 'childMissing';
  }

  return {
    title: `Missing required field: ${humanPath}`,
    whatThisMeans: generateWhatThisMeans('Required', humanPath, context, resourcePointer, discriminator),
    description: rule.message || error.message || `${humanPath} is mandatory.`,
    location: error.fieldPath,
    breadcrumb: breadcrumb,
    resourceType: resourcePointer.resourceType || context.resourceType,
    expected: 'A value must be provided',
    howToFix: generateHowToFix('Required', humanPath, context, resourcePointer, rule, pathAnalysis, fixScenario, discriminator),
    resourcePointer: resourcePointer,
    fixScenario: fixScenario,
    pathAnalysis: pathAnalysis,
    discriminator: discriminator,
  };
}

/**
 * Template: Regex pattern validation
 */
function renderRegex(error, rule, context, resourcePointer) {
  const humanPath = humanizePath(error.scope, error.fieldPath, context);
  const breadcrumb = generateBreadcrumb(error.fieldPath, context);
  const pattern = rule.pattern || '';
  const example = generateRegexExample(pattern, error.fieldPath);

  return {
    title: `Invalid format: ${humanPath}`,
    whatThisMeans: generateWhatThisMeans('Regex', humanPath, context, resourcePointer),
    description: error.message || `Value does not match the required pattern.`,
    location: error.fieldPath,
    breadcrumb: breadcrumb,
    resourceType: resourcePointer.resourceType || context.resourceType,
    expected: `Pattern: ${pattern}`,
    example: example,
    howToFix: generateHowToFix('Regex', humanPath, context, resourcePointer, rule),
    resourcePointer: resourcePointer,
  };
}

/**
 * Template: Type mismatch
 */
function renderType(error, rule, context, resourcePointer) {
  const humanPath = humanizePath(error.scope, error.fieldPath, context);
  const breadcrumb = generateBreadcrumb(error.fieldPath, context);
  const expectedType = rule.expectedType || 'correct type';

  return {
    title: `Wrong data type: ${humanPath}`,
    whatThisMeans: generateWhatThisMeans('Type', humanPath, context, resourcePointer),
    description: error.message || `Expected type: ${expectedType}`,
    location: error.fieldPath,
    breadcrumb: breadcrumb,
    resourceType: resourcePointer.resourceType || context.resourceType,
    expected: `Type: ${expectedType}`,
    howToFix: generateHowToFix('Type', humanPath, context, resourcePointer, rule),
    resourcePointer: resourcePointer,
  };
}

/**
 * Template: Fixed value mismatch
 */
function renderFixedValue(error, rule, context, resourcePointer) {
  const humanPath = humanizePath(error.scope, error.fieldPath, context);
  const breadcrumb = generateBreadcrumb(error.fieldPath, context);
  const expectedValue = rule.expectedValue;

  return {
    title: `Incorrect value: ${humanPath}`,
    whatThisMeans: generateWhatThisMeans('FixedValue', humanPath, context, resourcePointer),
    description: error.message || 'Value must match the specified fixed value.',
    location: error.fieldPath,
    breadcrumb: breadcrumb,
    resourceType: resourcePointer.resourceType || context.resourceType,
    expected: expectedValue,
    actual: extractActualValue(error.message),
    howToFix: generateHowToFix('FixedValue', humanPath, context, resourcePointer, rule),
    resourcePointer: resourcePointer,
  };
}

/**
 * Template: FullUrl/ID mismatch
 */
function renderFullUrlMatch(error, rule, context, resourcePointer) {
  return {
    title: 'ID and fullUrl mismatch',
    whatThisMeans: generateWhatThisMeans('FullUrlIdMatch', 'ID', context, resourcePointer),
    description: error.message || 'Resource.id must match the GUID portion of entry.fullUrl',
    location: error.fieldPath,
    breadcrumb: ['entry', 'fullUrl vs resource.id'],
    resourceType: resourcePointer.resourceType || context.resourceType,
    expected: 'entry.fullUrl = "urn:uuid:<GUID>" and resource.id = "<GUID>"',
    howToFix: generateHowToFix('FullUrlIdMatch', 'ID', context, resourcePointer, rule),
    resourcePointer: resourcePointer,
  };
}

/**
 * Template: Reference validation
 */
function renderReference(error, rule, context, resourcePointer) {
  const humanPath = humanizePath(error.scope, error.fieldPath, context);
  const breadcrumb = generateBreadcrumb(error.fieldPath, context);
  const targetTypes = rule.targetTypes || [];

  return {
    title: `Invalid reference: ${humanPath}`,
    whatThisMeans: generateWhatThisMeans('Reference', humanPath, context, resourcePointer),
    description: error.message || 'Reference must point to an allowed resource type.',
    location: error.fieldPath,
    breadcrumb: breadcrumb,
    resourceType: resourcePointer.resourceType || context.resourceType,
    allowedTypes: targetTypes,
    howToFix: generateHowToFix('Reference', humanPath, context, resourcePointer, rule),
    resourcePointer: resourcePointer,
  };
}

/**
 * Template: CodeSystem validation
 */
function renderCodeSystem(error, rule, context, resourcePointer) {
  const humanPath = humanizePath(error.scope, error.fieldPath, context);
  const breadcrumb = generateBreadcrumb(error.fieldPath, context);
  const concepts = context.codeSystemConcepts || [];

  // Generate expected value from concepts list
  let expectedValue = rule.system;
  if (concepts && concepts.length > 0) {
    const codeList = concepts.map(c => `'${c.code}'`).slice(0, 5).join(', ');
    const remaining = concepts.length > 5 ? ` (and ${concepts.length - 5} more)` : '';
    expectedValue = `One of: ${codeList}${remaining}`;
  }

  return {
    title: `Invalid code: ${humanPath}`,
    whatThisMeans: generateWhatThisMeans('CodeSystem', humanPath, context, resourcePointer),
    description: error.message || 'Code must be from the specified CodeSystem.',
    location: error.fieldPath,
    breadcrumb: breadcrumb,
    resourceType: resourcePointer.resourceType || context.resourceType,
    expected: expectedValue,
    allowedCodes: concepts.map((c) => ({
      code: c.code,
      display: c.display,
    })),
    howToFix: generateHowToFix('CodeSystem', humanPath, context, resourcePointer, rule),
    resourcePointer: resourcePointer,
  };
}

/**
 * Template: CodesMaster validation (PSS question answers)
 */
function renderCodesMaster(error, rule, context, resourcePointer) {
  const questionDisplay = context.questionDisplay || 'Question';
  const breadcrumb = generateBreadcrumb(error.fieldPath, context);
  const allowedAnswers = context.allowedAnswers || [];
  const actual = extractActualValue(error.message);

  const isMultiValue = allowedAnswers.some((a) => a.includes('|'));

  return {
    title: `Invalid answer: ${questionDisplay}`,
    whatThisMeans: generateWhatThisMeans('CodesMaster', questionDisplay, context, resourcePointer),
    description: error.message || 'Answer is not in the allowed list.',
    location: humanizePath(error.scope, error.fieldPath, context),
    breadcrumb: breadcrumb,
    resourceType: resourcePointer.resourceType || context.resourceType || 'Observation',
    questionCode: context.questionCode,
    questionDisplay: questionDisplay,
    expected: allowedAnswers.length > 0 ? 'One of the allowed answers' : null,
    actual: actual,
    allowedAnswers: allowedAnswers,
    isMultiValue: isMultiValue,
    howToFix: generateHowToFix('CodesMaster', questionDisplay, context, resourcePointer, rule),
    resourcePointer: resourcePointer,
  };
}

/**
 * Template: AllowedValues validation
 */
function renderAllowedValues(error, rule, context, resourcePointer) {
  const humanPath = humanizePath(error.scope, error.fieldPath, context);
  const breadcrumb = generateBreadcrumb(error.fieldPath, context);
  const allowedValues = rule.allowedValues || [];

  return {
    title: `Invalid value: ${humanPath}`,
    whatThisMeans: `The ${humanPath} must be one of the approved values. The value provided is not in the allowed list.`,
    description: error.message || 'Value must be one of the allowed values.',
    location: error.fieldPath,
    breadcrumb: breadcrumb,
    resourceType: resourcePointer.resourceType || context.resourceType,
    allowedAnswers: allowedValues,
    howToFix: [
      `Choose one of the allowed values from the list below.`,
      `Check for exact spelling and case sensitivity.`,
      `Verify your source data matches one of these approved values.`,
    ],
    resourcePointer: resourcePointer,
  };
}

/**
 * Humanize CPS1 path to user-friendly display
 */
export function humanizePath(scope, fieldPath, context) {
  if (!fieldPath) return 'Unknown field';

  // If we have a question display, use it
  if (context?.questionDisplay) {
    return context.questionDisplay;
  }

  // Handle extension-based coded fields
  if (fieldPath.includes('extension[url:')) {
    if (fieldPath.includes('/ext-grc]')) return 'GRC Code';
    if (fieldPath.includes('/ext-constituency]')) return 'Constituency Code';
    if (fieldPath.includes('/ext-ethnicity]')) return 'Ethnicity Code';
    if (fieldPath.includes('/ext-residential-status]')) return 'Residential Status Code';
    if (fieldPath.includes('/ext-subsidy]')) return 'Subsidy Code';
  }

  // Handle NRIC identifier specifically
  if (fieldPath.includes('identifier[system:') && fieldPath.includes('/nric')) {
    return 'NRIC';
  }
  
  // Handle other common identifiers
  if (fieldPath.includes('identifier') && fieldPath.includes('value')) {
    if (fieldPath.includes('/fin')) return 'FIN';
    if (fieldPath.includes('/passport')) return 'Passport Number';
  }

  // Handle Organization type
  if (fieldPath.includes('Organization.type.coding')) {
    return 'Organization Type Code';
  }

  // Handle Observation screening type
  if (fieldPath.includes('Observation.code.coding') && fieldPath.includes('/screening-type')) {
    return 'Screening Type Code';
  }

  // Extract last meaningful segment
  let path = fieldPath;

  // Remove array indices and system filters
  path = path.replace(/\[system:[^\]]+\]/g, '');
  path = path.replace(/\[code:[^\]]+\]/g, '');
  path = path.replace(/\[url:[^\]]+\]/g, '');
  path = path.replace(/\[\d+\]/g, '');
  path = path.replace(/^entry\.resource\./, '');

  // Get the last segment
  const segments = path.split('.');
  let lastSegment = segments[segments.length - 1];

  // Prettify camelCase to Title Case
  lastSegment = lastSegment
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, (str) => str.toUpperCase())
    .trim();

  return lastSegment;
}

/**
 * Generate breadcrumb path for navigation
 */
function generateBreadcrumb(fieldPath, context) {
  if (!fieldPath) return [];

  // If we have a question display, use simplified breadcrumb
  if (context?.questionDisplay) {
    return [
      context.resourceType || 'Observation',
      'component',
      context.questionDisplay,
      'value'
    ];
  }

  let path = fieldPath;
  
  // Remove entry prefix
  path = path.replace(/^entry\[\d+\]\.resource\./, '');
  
  // For paths with filters, we need to NOT split by dots inside the filter brackets
  // Example: identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value
  // Should become: ["identifier[system:https://fhir.synapxe.sg/NamingSystem/nric]", "value"]
  
  const segments = [];
  let currentSegment = '';
  let insideBracket = false;
  
  for (let i = 0; i < path.length; i++) {
    const char = path[i];
    
    if (char === '[') {
      insideBracket = true;
      currentSegment += char;
    } else if (char === ']') {
      insideBracket = false;
      currentSegment += char;
    } else if (char === '.' && !insideBracket) {
      if (currentSegment) {
        segments.push(currentSegment);
        currentSegment = '';
      }
    } else {
      currentSegment += char;
    }
  }
  
  if (currentSegment) {
    segments.push(currentSegment);
  }
  
  const breadcrumb = [];

  for (const segment of segments) {
    let clean = segment;
    
    // Handle filters with system/code/url - keep them as-is
    if (clean.includes('[system:') || clean.includes('[code:') || clean.includes('[url:')) {
      breadcrumb.push(clean);
      continue;
    }
    
    // Handle numeric array indices
    if (clean.includes('[') && /\[\d+\]/.test(clean)) {
      breadcrumb.push(clean); // Keep as-is with index
      continue;
    }
    
    // Remove only numeric indices that are standalone
    clean = clean.replace(/\[\d+\]/g, '');
    
    if (clean) {
      // Capitalize first letter for simple field names
      clean = clean.charAt(0).toUpperCase() + clean.slice(1);
      breadcrumb.push(clean);
    }
  }

  return breadcrumb;
}

/**
 * Generate "What this means" explanation
 */
function generateWhatThisMeans(ruleType, humanPath, context, resourcePointer, discriminator) {
  const resourceName = resourcePointer.resourceType || context.resourceType || 'record';
  
  switch (ruleType) {
    case 'Required':
      if (discriminator) {
        return `This ${resourceName.toLowerCase()} is missing the ${humanPath} field within a specific ${discriminator.arrayName} entry. The system could not find an ${discriminator.arrayName} element with ${discriminator.field}="${discriminator.value}", or found it but the ${discriminator.childPath} is missing.`;
      }
      if (humanPath === 'NRIC') {
        return `This patient record is missing the NRIC value. We could not find the NRIC field in the Patient details.`;
      }
      return `This ${resourceName.toLowerCase()} is missing the ${humanPath} field. This information is required but was not provided.`;
      
    case 'Regex':
      return `The ${humanPath} value doesn't match the required format. The system expects a specific pattern for this field.`;
      
    case 'Type':
      return `The ${humanPath} has the wrong data type. The system expects a different type of value (e.g., number instead of text).`;
      
    case 'FixedValue':
      return `The ${humanPath} must have a specific fixed value, but a different value was provided.`;
      
    case 'FullUrlIdMatch':
      return `The resource ID doesn't match the identifier in the fullUrl. These two values must be identical for the system to process the record correctly.`;
      
    case 'Reference':
      return `The ${humanPath} reference points to an invalid or missing resource. References must link to resources that exist in this bundle.`;
      
    case 'CodeSystem':
      return `The ${humanPath} contains an invalid code. The code must be from the approved list of codes for this field.`;
      
    case 'CodesMaster':
      const question = context.questionDisplay || humanPath;
      return `The answer provided for "${question}" is not one of the allowed values. Please select from the approved answer options.`;
      
    default:
      return `The ${humanPath} field has a validation error. Please review and correct the value.`;
  }
}

/**
 * Generate "How to fix" steps
 */
function generateHowToFix(ruleType, humanPath, context, resourcePointer, rule, pathAnalysis = {}, fixScenario = 'childMissing') {
  const resourceName = resourcePointer.resourceType || context.resourceType || 'resource';
  const entryIndex = resourcePointer.entryIndex;
  const steps = [];

  // Step 1: Navigate to resource
  if (entryIndex !== null && entryIndex !== undefined) {
    steps.push(`Open the ${resourceName} resource (entry #${entryIndex}) in your bundle.`);
  } else {
    steps.push(`Open the ${resourceName} resource in your bundle.`);
  }

  // Handle parent-missing scenario for Required rule
  if (ruleType === 'Required' && fixScenario === 'parentMissing') {
    steps.push(`⚠️ WARNING: One or more parent segments in the path could not be found.`);
    
    if (pathAnalysis.pathMismatchSegment) {
      steps.push(`The segment "${pathAnalysis.pathMismatchSegment}" appears to be missing or misspelled.`);
    }
    
    steps.push(`Before adding the ${humanPath} field, you must first:`);
    steps.push(`  • Check that all parent objects in the path exist`);
    steps.push(`  • Verify correct spelling of all path segments`);
    steps.push(`  • Ensure the parent structure matches FHIR specification`);
    steps.push(`  • Fix any structural issues in the JSON`);
    steps.push(`After correcting the parent structure, add the ${humanPath} field with an appropriate value.`);
    steps.push(`Save your changes and validate again to confirm the error is resolved.`);
    return steps;
  }

  // Step 2: Specific fix based on rule type (child-missing scenario)
  switch (ruleType) {
    case 'Required':
      if (humanPath === 'NRIC') {
        steps.push(`In the "identifier" section, add an entry with system: https://fhir.synapxe.sg/NamingSystem/nric`);
        steps.push(`Add the NRIC value in the format: S1234567A (or T/F/G followed by 7 digits and a letter)`);
      } else {
        steps.push(`Navigate through the structure: ${context.resourceType || resourceName} → ${humanPath}`);
        steps.push(`Add the ${humanPath} field with an appropriate value.`);
        steps.push(`Ensure the field is not empty or null.`);
      }
      break;
      
    case 'Regex':
      if (rule.pattern) {
        const example = generateRegexExample(rule.pattern, humanPath);
        steps.push(`Update the ${humanPath} to match the required format.`);
        if (example) {
          steps.push(`Example format: ${example}`);
        } else {
          steps.push(`Pattern: ${rule.pattern}`);
        }
      }
      break;
      
    case 'Type':
      if (rule.expectedType) {
        steps.push(`Change the ${humanPath} to the correct type: ${rule.expectedType}`);
        const example = getTypeExample(rule.expectedType);
        if (example) steps.push(example);
      }
      break;
      
    case 'FixedValue':
      if (rule.expectedValue) {
        steps.push(`Set the ${humanPath} to exactly: "${rule.expectedValue}"`);
        steps.push(`This field must have this exact value - no variations allowed.`);
      }
      break;
      
    case 'CodesMaster':
      if (context.allowedAnswers && context.allowedAnswers.length > 0) {
        steps.push(`Change the answer to one of the allowed values (see list below).`);
        if (context.allowedAnswers.some(a => a.includes('|'))) {
          steps.push(`For multiple answers, separate them with a pipe character (|), e.g., "500Hz – R|1000Hz – R"`);
        }
      }
      break;
      
    case 'CodeSystem':
      steps.push(`Select a valid code from the approved code list (see below).`);
      if (rule.system) {
        steps.push(`The code must be from: ${rule.system.split('/').pop()}`);
      }
      break;
  }

  // Step 3: Verify
  steps.push(`Save your changes and validate again to confirm the error is resolved.`);

  return steps;
}

/**
 * Extract field name from path
 */
function extractFieldName(fieldPath) {
  if (!fieldPath) return 'field';
  const segments = fieldPath.split('.');
  return segments[segments.length - 1].replace(/\[.*?\]/g, '');
}

/**
 * Extract actual value from error message
 */
function extractActualValue(message) {
  if (!message) return null;

  // Try to extract from patterns like "found 'value'" or "got value"
  const patterns = [
    /found ['"](.+?)['"]/i,
    /got ['"](.+?)['"]/i,
    /actual[:\s]+['"](.+?)['"]/i,
    /is ['"](.+?)['"]/i,
  ];

  for (const pattern of patterns) {
    const match = message.match(pattern);
    if (match) return match[1];
  }

  return null;
}

/**
 * Generate example for regex pattern
 */
function generateRegexExample(pattern, fieldPath) {
  // NRIC pattern
  if (pattern.includes('[STFG]') && pattern.includes('\\d{7}')) {
    return 'S1234567A';
  }

  // UUID pattern
  if (pattern.includes('uuid') || pattern.includes('[0-9a-fA-F]{8}')) {
    return 'urn:uuid:550e8400-e29b-41d4-a716-446655440000';
  }

  // Date pattern YYYY-MM-DD
  if (pattern.includes('\\d{4}') && pattern.includes('\\d{2}')) {
    return '2024-01-15';
  }

  return null;
}

/**
 * Get example for data type
 */
function getTypeExample(expectedType) {
  const examples = {
    string: 'Example: "text value"',
    integer: 'Example: 42',
    decimal: 'Example: 3.14',
    boolean: 'Example: true or false',
    date: 'Example: 2024-01-15 (YYYY-MM-DD)',
    datetime: 'Example: 2024-01-15T10:30:00Z',
    guid: 'Example: 550e8400-e29b-41d4-a716-446655440000',
  };

  return examples[expectedType?.toLowerCase()] || null;
}
