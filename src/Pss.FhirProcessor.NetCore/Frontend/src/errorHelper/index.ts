/**
 * Error Helper System - Main Exports
 * 
 * Import this module to access all error helper functionality:
 * 
 * @example
 * import { 
 *   generateHelper, 
 *   ErrorHelperPanel,
 *   parseFieldPath,
 *   analyzePath 
 * } from './errorHelper';
 */

// Main components
export { default as ErrorHelperPanel } from '../components/ErrorHelperPanel';
export { default as ValidationTabWithHelper } from '../components/ValidationTabWithHelper';

// Core utilities
export {
  generateHelper,
  getResourceByEntryIndex,
  navigateToParent,
  type ValidationError,
  type HelperMessage,
  type LocationInfo,
  type ExpectedInfo,
} from '../utils/helperGenerator';

export {
  parseFieldPath,
  analyzePath,
  findFirstMissingSegment,
  buildPathString,
  formatSegment,
  getParentSegments,
  type PathSegment,
  type PathStatus,
} from '../utils/pathParser';

export {
  buildJsonSnippet,
  buildCompleteExample,
  formatJsonSnippet,
  type SnippetOptions,
} from '../utils/snippetBuilder';

export {
  getErrorTemplate,
  generateFixInstructions,
  type ErrorCodeTemplate,
  type FixInstructions,
  ERROR_CODE_TEMPLATES,
} from '../utils/helperTemplates';

// Example data for testing
export {
  EXAMPLE_VALIDATION_ERRORS,
  EXAMPLE_FHIR_BUNDLE,
} from '../examples/errorExamples';
