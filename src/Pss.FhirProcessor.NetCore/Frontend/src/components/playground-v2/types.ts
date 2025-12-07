// Type definitions for validation helper utility

export interface ValidationError {
  code: string;
  message: string;
  fieldPath: string;
  scope: string;
  ruleType?: string;
  rule?: any;
  context?: any;
  resourcePointer?: ResourcePointer;
}

export interface ResourcePointer {
  entryIndex: number;
  resourceType: string;
  resourceId?: string;
  fullUrl?: string;
}

export interface Helper {
  title: string;
  whatThisMeans?: string;
  description?: string;
  location?: string;
  breadcrumb?: string[];
  resourceType?: string;
  resourcePointer?: ResourcePointer;
  expected?: string;
  actual?: string;
  example?: string;
  allowedAnswers?: string[];
  allowedCodes?: Array<{ code: string; display: string }>;
  allowedTypes?: string[];
  questionCode?: string;
  questionDisplay?: string;
  isMultiValue?: boolean;
  howToFix?: string[];
  screeningType?: string;
}

declare module '../../utils/validationHelper' {
  export function generateHelper(error: ValidationError): Helper;
  export function humanizePath(scope: string, fieldPath: string, context: any): string;
}
