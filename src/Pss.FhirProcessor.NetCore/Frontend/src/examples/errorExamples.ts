/**
 * Example validation errors for testing the Error Helper Panel
 * These examples cover all major error types and scenarios
 */

export const EXAMPLE_VALIDATION_ERRORS = {
  // 1. Missing Leaf Node
  missingLeafNode: {
    code: 'MANDATORY_MISSING',
    message: 'Ethnicity coding system is mandatory.',
    fieldPath:
      'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].system',
    scope: 'Patient',
    ruleType: 'Required',
    rule: {
      path:
        'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].system',
    },
    resourcePointer: {
      entryIndex: 0,
      resourceType: 'Patient',
      resourceId: '12345678-1234-1234-1234-123456789abc',
    },
  },

  // 2. Missing Middle Parent
  missingMiddleParent: {
    code: 'MANDATORY_MISSING',
    message: 'Language coding system is mandatory.',
    fieldPath: 'Patient.communication.language.coding.system',
    scope: 'Patient',
    ruleType: 'Required',
    rule: {
      path: 'Patient.communication.language.coding.system',
    },
    resourcePointer: {
      entryIndex: 0,
      resourceType: 'Patient',
    },
  },

  // 3. Missing Top-Level Parent
  missingTopLevelParent: {
    code: 'MANDATORY_MISSING',
    message: 'At least one address line is mandatory.',
    fieldPath: 'Patient.address.line',
    scope: 'Patient',
    ruleType: 'Required',
    rule: {
      path: 'Patient.address.line',
    },
    resourcePointer: {
      entryIndex: 0,
      resourceType: 'Patient',
    },
  },

  // 4. Wrong Array Index
  wrongArrayIndex: {
    code: 'MANDATORY_MISSING',
    message: 'Ethnicity code is mandatory.',
    fieldPath:
      'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[1].code',
    scope: 'Patient',
    ruleType: 'Required',
    rule: {
      path:
        'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[1].code',
    },
    resourcePointer: {
      entryIndex: 0,
      resourceType: 'Patient',
    },
  },

  // 5. Invalid Code Mismatch
  invalidCodeMismatch: {
    code: 'INVALID_CODE',
    message: 'Ethnicity code must be a valid value from the ethnicity CodeSystem.',
    fieldPath:
      'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].code',
    scope: 'Patient',
    ruleType: 'CodeSystem',
    rule: {
      path:
        'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0]',
      system: 'https://fhir.synapxe.sg/CodeSystem/ethnicity',
      allowedValues: ['CN', 'MY', 'IN', 'XX'],
    },
    resourcePointer: {
      entryIndex: 0,
      resourceType: 'Patient',
    },
  },

  // 6. Fixed Value Mismatch
  fixedValueMismatch: {
    code: 'FIXED_VALUE_MISMATCH',
    message: 'Ethnicity must use the PSS ethnicity CodeSystem.',
    fieldPath:
      'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].system',
    scope: 'Patient',
    ruleType: 'FixedValue',
    rule: {
      path:
        'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].system',
      expectedValue: 'https://fhir.synapxe.sg/CodeSystem/ethnicity',
    },
    resourcePointer: {
      entryIndex: 0,
      resourceType: 'Patient',
    },
  },

  // 7. ID-FullUrl Mismatch
  idFullUrlMismatch: {
    code: 'ID_FULLURL_MISMATCH',
    message:
      "Resource.id must match GUID portion of entry.fullUrl (urn:uuid:<GUID>). (id: a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91, fullUrl: urn:uuid:a3f9c2d1-2b40-4abx9-8133-5b1f2d4e9f91)",
    fieldPath: '',
    scope: 'Patient',
    ruleType: 'FullUrlIdMatch',
    resourcePointer: {
      entryIndex: 0,
      resourceType: 'Patient',
      fullUrl: 'urn:uuid:a3f9c2d1-2b40-4abx9-8133-5b1f2d4e9f91',
      resourceId: 'a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91',
    },
  },

  // 8. Type Mismatch - Date Format
  typeMismatchDate: {
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
  },

  // 9. Type Mismatch - GUID
  typeMismatchGuid: {
    code: 'TYPE_MISMATCH',
    message: 'Patient.id must be GUID.',
    fieldPath: 'Patient.id',
    scope: 'Patient',
    ruleType: 'Type',
    rule: {
      path: 'Patient.id',
      expectedType: 'guid',
    },
    resourcePointer: {
      entryIndex: 0,
      resourceType: 'Patient',
    },
  },

  // 10. Regex Invalid - NRIC
  regexInvalidNric: {
    code: 'REGEX_INVALID_NRIC',
    message: 'NRIC format is invalid.',
    fieldPath:
      'Patient.identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value',
    scope: 'Patient',
    ruleType: 'Regex',
    rule: {
      path:
        'Patient.identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value',
      pattern: '^[STFG]\\d{7}[A-Z]$',
    },
    resourcePointer: {
      entryIndex: 0,
      resourceType: 'Patient',
    },
  },

  // 11. Regex Invalid - Postal Code
  regexInvalidPostal: {
    code: 'REGEX_INVALID_POSTAL',
    message: 'Location.postalCode must be a 6-digit value.',
    fieldPath: 'Location.address.postalCode',
    scope: 'Location',
    ruleType: 'Regex',
    rule: {
      path: 'Location.address.postalCode',
      pattern: '^\\d{6}$',
    },
    resourcePointer: {
      entryIndex: 3,
      resourceType: 'Location',
    },
  },

  // 12. Reference Not Found
  referenceNotFound: {
    code: 'REF_SUBJECT_INVALID',
    message: 'Encounter.subject.reference must reference a Patient.',
    fieldPath: 'Encounter.subject.reference',
    scope: 'Encounter',
    ruleType: 'Reference',
    rule: {
      path: 'Encounter.subject.reference',
      targetTypes: ['Patient'],
    },
    resourcePointer: {
      entryIndex: 1,
      resourceType: 'Encounter',
    },
  },

  // 13. Invalid Answer Value (CodesMaster)
  invalidAnswerValue: {
    code: 'INVALID_ANSWER_VALUE',
    message:
      "Invalid answer 'Maybe' for question 'SQ-L2H9-00000001' | Allowed: ['Yes (Proceed to next question)', 'No (To continue with hearing screening)']",
    fieldPath: 'Observation.component[0].valueString',
    scope: 'Observation.HearingScreening',
    ruleType: 'CodesMaster',
    context: {
      resourceType: 'Observation',
      screeningType: 'HS',
      questionCode: 'SQ-L2H9-00000001',
      questionDisplay: 'Is the participant currently wearing hearing aid(s)?',
      allowedAnswers: [
        'Yes (Proceed to next question)',
        'No (To continue with hearing screening)',
      ],
    },
    resourcePointer: {
      entryIndex: 4,
      resourceType: 'Observation',
    },
  },

  // 14. Missing Resource in Bundle
  missingResourceInBundle: {
    code: 'MISSING_PATIENT',
    message: 'Bundle must contain a Patient resource.',
    fieldPath: 'Bundle.entry[Patient]',
    scope: 'Bundle',
    ruleType: 'Required',
    rule: {
      path: 'Bundle.entry[Patient]',
    },
  },

  // 15. Invalid FullUrl Format
  invalidFullUrlFormat: {
    code: 'TYPE_INVALID_FULLURL',
    message: 'Bundle.entry.fullUrl must be urn:uuid:<GUID>.',
    fieldPath: 'Bundle.entry[0].fullUrl',
    scope: 'Bundle',
    ruleType: 'Regex',
    rule: {
      path: 'Bundle.entry[*].fullUrl',
      pattern:
        '^urn:uuid:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$',
    },
    resourcePointer: {
      entryIndex: 0,
    },
  },

  // 16. Component Display Mismatch
  componentDisplayMismatch: {
    code: 'INVALID_ANSWER_VALUE',
    message:
      "Question display mismatch for 'SQ-L2H9-00000001' | Expected: 'Is the participant currently wearing hearing aid(s)?' | Actual: 'Wrong display text'",
    fieldPath: 'Observation.component[0].code.coding[0].display',
    scope: 'Observation.HearingScreening',
    ruleType: 'CodesMaster',
    context: {
      questionCode: 'SQ-L2H9-00000001',
      questionDisplay: 'Is the participant currently wearing hearing aid(s)?',
    },
    resourcePointer: {
      entryIndex: 4,
      resourceType: 'Observation',
    },
  },

  // 17. Multiple Answers Not Allowed
  multipleAnswersNotAllowed: {
    code: 'INVALID_ANSWER_VALUE',
    message:
      "Question 'SQ-L2H9-00000001' does not allow multiple answers | Found: 2 values",
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
      entryIndex: 4,
      resourceType: 'Observation',
    },
  },
};

/**
 * Sample JSON for testing (matches the errors above)
 */
export const EXAMPLE_FHIR_BUNDLE = {
  resourceType: 'Bundle',
  type: 'collection',
  entry: [
    {
      fullUrl: 'urn:uuid:a3f9c2d1-2b40-4abx9-8133-5b1f2d4e9f91',
      resource: {
        resourceType: 'Patient',
        id: 'a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91', // Mismatch with fullUrl
        name: 'Test Patient',
        birthDate: '2024/01/15', // Wrong format
        identifier: [
          {
            system: 'https://fhir.synapxe.sg/NamingSystem/nric',
            value: 'S1234567', // Missing checksum letter
          },
        ],
        extension: [
          {
            url: 'https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity',
            valueCodeableConcept: {
              coding: [
                {
                  // Missing system
                  code: 'ZZ', // Invalid code
                },
              ],
            },
          },
        ],
        // Missing communication, address
      },
    },
    {
      fullUrl: 'urn:uuid:22222222-2222-2222-2222-222222222222',
      resource: {
        resourceType: 'Encounter',
        id: '22222222-2222-2222-2222-222222222222',
        status: 'completed',
        subject: {
          reference: 'Patient/non-existent-patient', // Reference not found
        },
      },
    },
    {
      fullUrl: 'urn:uuid:33333333-3333-3333-3333-333333333333',
      resource: {
        resourceType: 'Location',
        id: '33333333-3333-3333-3333-333333333333',
        address: {
          line: ['123 Test St'],
          postalCode: '12345', // Only 5 digits instead of 6
        },
      },
    },
    {
      fullUrl: 'urn:uuid:44444444-4444-4444-4444-444444444444',
      resource: {
        resourceType: 'Observation',
        id: '44444444-4444-4444-4444-444444444444',
        code: {
          coding: [
            {
              system: 'https://fhir.synapxe.sg/CodeSystem/screening-type',
              code: 'HS',
            },
          ],
        },
        component: [
          {
            code: {
              coding: [
                {
                  system: 'https://fhir.synapxe.sg/CodeSystem/screening-question',
                  code: 'SQ-L2H9-00000001',
                  display: 'Wrong display text', // Display mismatch
                },
              ],
            },
            valueString: 'Maybe', // Invalid answer
          },
        ],
      },
    },
  ],
};
