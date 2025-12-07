// Core FHIR Types
export interface ValidationError {
  code: string;
  scope: string;
  fieldPath: string;
  message: string;
  ruleType?: string;
  rule?: ValidationRuleMetadata;
  context?: ValidationErrorContext;
  resourcePointer?: ResourcePointer;
}

export interface ValidationRuleMetadata {
  path?: string;
  expectedType?: string;
  expectedValue?: string;
  pattern?: string;
  system?: string;
  targetTypes?: string[];
  isRequired?: boolean;
}

export interface ValidationErrorContext {
  screeningType?: string;
  questionDisplay?: string;
  allowedAnswers?: string[];
  codeSystemConcepts?: CodeSystemConcept[];
}

export interface CodeSystemConcept {
  code: string;
  display?: string;
}

export interface ResourcePointer {
  entryIndex?: number;
  fullUrl?: string;
  resourceType?: string;
  resolvedTreePath?: string;
}

export interface ValidationResult {
  isValid: boolean;
  errors: ValidationError[];
}

export interface ProcessingResult {
  success: boolean;
  validation?: ValidationResult;
  flatten?: any;
  originalBundle?: any;
  logs?: string[];
}

// Tree View Types
export interface TreeNode {
  title: React.ReactNode;
  key: string;
  icon?: React.ReactNode;
  children?: TreeNode[];
  isLeaf?: boolean;
}

// JSON Editor Types
export interface JSONEditorState {
  value: string;
  isValid: boolean;
  error?: string;
  errorPosition?: { line: number; column: number };
}

// Sample File Types
export interface SampleFile {
  name: string;
  fileName: string;
  path: string;
  loader: () => Promise<any>;
}
