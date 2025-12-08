/**
 * Unit tests for Helper Generator
 */

import { generateHelper, ValidationError } from '../utils/helperGenerator';

describe('Helper Generator', () => {
  const sampleJson = {
    entry: [
      {
        fullUrl: 'urn:uuid:12345678-1234-1234-1234-123456789abc',
        resource: {
          resourceType: 'Patient',
          id: '12345678-1234-1234-1234-123456789abc',
          name: 'Test Patient',
          extension: [
            {
              url: 'https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity',
              valueCodeableConcept: {
                coding: [
                  {
                    code: 'CN',
                  },
                ],
              },
            },
          ],
        },
      },
    ],
  };

  describe('MANDATORY_MISSING error', () => {
    it('should generate helper for missing leaf node', () => {
      const error: ValidationError = {
        code: 'MANDATORY_MISSING',
        message: 'Ethnicity coding system is mandatory.',
        fieldPath: 'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].system',
        scope: 'Patient',
        ruleType: 'Required',
        rule: {
          path: 'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].system',
        },
        resourcePointer: {
          entryIndex: 0,
          resourceType: 'Patient',
          resourceId: '12345678-1234-1234-1234-123456789abc',
        },
      };

      const helper = generateHelper(error, sampleJson.entry[0].resource);

      expect(helper.whatThisMeans).toBeDefined();
      expect(helper.whatThisMeans.whatThisMeans).toContain('required field');
      
      expect(helper.location.entryIndex).toBe(0);
      expect(helper.location.resourceType).toBe('Patient');
      expect(helper.location.breadcrumb.length).toBeGreaterThan(0);
      
      expect(helper.pathBreakdown.length).toBeGreaterThan(0);
      const lastStatus = helper.pathBreakdown[helper.pathBreakdown.length - 1];
      expect(lastStatus.exists).toBe(false);
      expect(lastStatus.segment).toBe('system');
      
      expect(helper.exampleSnippet).toBeTruthy();
      expect(helper.exampleSnippet).toContain('system');
      
      expect(helper.howToFix.steps.length).toBeGreaterThan(0);
    });

    it('should generate helper for missing parent', () => {
      const error: ValidationError = {
        code: 'MANDATORY_MISSING',
        message: 'Language coding system is mandatory.',
        fieldPath: 'Patient.communication.language.coding.system',
        scope: 'Patient',
        ruleType: 'Required',
        resourcePointer: {
          entryIndex: 0,
          resourceType: 'Patient',
        },
      };

      const helper = generateHelper(error, sampleJson.entry[0].resource);

      expect(helper.howToFix.needsParentCreation).toBe(true);
      expect(helper.howToFix.missingSegments.length).toBeGreaterThan(1);
      
      expect(helper.exampleSnippet).toContain('communication');
      expect(helper.exampleSnippet).toContain('language');
      expect(helper.exampleSnippet).toContain('coding');
      expect(helper.exampleSnippet).toContain('system');
    });
  });

  describe('FIXED_VALUE_MISMATCH error', () => {
    it('should generate helper with expected value', () => {
      const error: ValidationError = {
        code: 'FIXED_VALUE_MISMATCH',
        message: 'Ethnicity must use the PSS ethnicity CodeSystem.',
        fieldPath: 'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].system',
        scope: 'Patient',
        ruleType: 'FixedValue',
        rule: {
          path: 'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].system',
          expectedValue: 'https://fhir.synapxe.sg/CodeSystem/ethnicity',
        },
        resourcePointer: {
          entryIndex: 0,
          resourceType: 'Patient',
        },
      };

      const helper = generateHelper(error, sampleJson.entry[0].resource);

      expect(helper.expected.type).toBe('value');
      expect(helper.expected.value).toBe('https://fhir.synapxe.sg/CodeSystem/ethnicity');
      expect(helper.expected.description).toContain('exact value');
      
      expect(helper.exampleSnippet).toContain('https://fhir.synapxe.sg/CodeSystem/ethnicity');
    });
  });

  describe('INVALID_CODE error', () => {
    it('should generate helper with allowed values', () => {
      const error: ValidationError = {
        code: 'INVALID_CODE',
        message: 'Ethnicity code must be a valid value from the ethnicity CodeSystem.',
        fieldPath: 'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].code',
        scope: 'Patient',
        ruleType: 'CodeSystem',
        rule: {
          path: 'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0]',
          system: 'https://fhir.synapxe.sg/CodeSystem/ethnicity',
          allowedValues: ['CN', 'MY', 'IN', 'XX'],
        },
        resourcePointer: {
          entryIndex: 0,
          resourceType: 'Patient',
        },
      };

      const helper = generateHelper(error, sampleJson.entry[0].resource);

      expect(helper.expected.type).toBe('codes');
      expect(helper.expected.allowedValues).toBeDefined();
      expect(helper.expected.allowedValues?.length).toBe(4);
      expect(helper.expected.allowedValues?.[0].code).toBe('CN');
    });
  });

  describe('ID_FULLURL_MISMATCH error', () => {
    it('should generate helper for ID mismatch', () => {
      const error: ValidationError = {
        code: 'ID_FULLURL_MISMATCH',
        message: 'Resource.id must match GUID portion of entry.fullUrl (urn:uuid:<GUID>).',
        fieldPath: '',
        scope: 'Patient',
        ruleType: 'FullUrlIdMatch',
        resourcePointer: {
          entryIndex: 0,
          resourceType: 'Patient',
          fullUrl: 'urn:uuid:12345678-1234-1234-1234-123456789abc',
          resourceId: '87654321-4321-4321-4321-cba987654321',
        },
      };

      const helper = generateHelper(error, sampleJson.entry[0].resource);

      expect(helper.whatThisMeans.whatThisMeans).toContain('must exactly match');
      expect(helper.howToFix.steps.some(step => step.includes('fullUrl'))).toBe(true);
      expect(helper.howToFix.steps.some(step => step.includes('resource.id'))).toBe(true);
      
      if (helper.completeExample) {
        expect(helper.completeExample).toContain('fullUrl');
        expect(helper.completeExample).toContain('urn:uuid:');
      }
    });
  });

  describe('TYPE_MISMATCH error', () => {
    it('should generate helper for type mismatch', () => {
      const error: ValidationError = {
        code: 'TYPE_MISMATCH',
        message: 'Patient.birthDate must be YYYY-MM-DD.',
        fieldPath: 'Patient.birthDate',
        scope: 'Patient',
        ruleType: 'Type',
        rule: {
          path: 'Patient.birthDate',
          expectedType: 'date',
        },
        resourcePointer: {
          entryIndex: 0,
          resourceType: 'Patient',
        },
      };

      const helper = generateHelper(error, sampleJson.entry[0].resource);

      expect(helper.expected.type).toBe('format');
      expect(helper.expected.value).toBe('date');
      expect(helper.exampleSnippet).toMatch(/\d{4}-\d{2}-\d{2}/);
    });
  });

  describe('REGEX_INVALID_NRIC error', () => {
    it('should generate helper for regex validation', () => {
      const error: ValidationError = {
        code: 'REGEX_INVALID_NRIC',
        message: 'NRIC format is invalid.',
        fieldPath: 'Patient.identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value',
        scope: 'Patient',
        ruleType: 'Regex',
        rule: {
          path: 'Patient.identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value',
          pattern: '^[STFG]\\d{7}[A-Z]$',
        },
        resourcePointer: {
          entryIndex: 0,
          resourceType: 'Patient',
        },
      };

      const helper = generateHelper(error, sampleJson.entry[0].resource);

      expect(helper.expected.type).toBe('pattern');
      expect(helper.expected.pattern).toBe('^[STFG]\\d{7}[A-Z]$');
      expect(helper.whatThisMeans.whatThisMeans).toContain('NRIC');
    });
  });

  describe('INVALID_ANSWER_VALUE error (CodesMaster)', () => {
    it('should generate helper with question context', () => {
      const error: ValidationError = {
        code: 'INVALID_ANSWER_VALUE',
        message: 'Observation components must use valid question codes, displays, and allowed answers.',
        fieldPath: 'Observation.component[0].valueString',
        scope: 'Observation.HearingScreening',
        ruleType: 'CodesMaster',
        context: {
          questionCode: 'SQ-L2H9-00000001',
          questionDisplay: 'Is the participant currently wearing hearing aid(s)?',
          allowedAnswers: [
            'Yes (Proceed to next question)',
            'No (To continue with hearing screening)',
          ],
        },
        resourcePointer: {
          entryIndex: 2,
          resourceType: 'Observation',
        },
      };

      const helper = generateHelper(error, sampleJson.entry[0].resource);

      expect(helper.expected.type).toBe('codes');
      expect(helper.expected.allowedValues?.length).toBe(2);
      expect(helper.expected.description).toContain('SQ-L2H9-00000001');
    });
  });
});
