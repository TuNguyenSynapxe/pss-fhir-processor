/**
 * Unit tests for Path Parser and Analyzer
 */

import {
  parseFieldPath,
  analyzePath,
  findFirstMissingSegment,
  buildPathString,
  formatSegment,
  PathSegment,
} from '../utils/pathParser';

describe('Path Parser', () => {
  describe('parseFieldPath', () => {
    it('should parse simple property path', () => {
      const result = parseFieldPath('Patient.name');
      expect(result).toEqual([
        { type: 'property', name: 'Patient' },
        { type: 'property', name: 'name' },
      ]);
    });

    it('should parse array index path', () => {
      const result = parseFieldPath('coding[0]');
      expect(result).toEqual([
        { type: 'array', name: 'coding', index: 0 },
      ]);
    });

    it('should parse filtered array path', () => {
      const result = parseFieldPath('extension[url:https://example.com]');
      expect(result).toEqual([
        {
          type: 'filteredArray',
          name: 'extension',
          filter: { property: 'url', value: 'https://example.com' },
        },
      ]);
    });

    it('should parse wildcard array path', () => {
      const result = parseFieldPath('component[*]');
      expect(result).toEqual([
        { type: 'array', name: 'component', index: 0 },
      ]);
    });

    it('should parse complex nested path', () => {
      const result = parseFieldPath(
        'Patient.extension[url:https://test.com].valueCodeableConcept.coding[0].system'
      );
      expect(result).toHaveLength(5);
      expect(result[0]).toEqual({ type: 'property', name: 'Patient' });
      expect(result[1]).toEqual({
        type: 'filteredArray',
        name: 'extension',
        filter: { property: 'url', value: 'https://test.com' },
      });
      expect(result[2]).toEqual({ type: 'property', name: 'valueCodeableConcept' });
      expect(result[3]).toEqual({ type: 'array', name: 'coding', index: 0 });
      expect(result[4]).toEqual({ type: 'property', name: 'system' });
    });

    it('should handle empty path', () => {
      const result = parseFieldPath('');
      expect(result).toEqual([]);
    });
  });

  describe('analyzePath', () => {
    const sampleJson = {
      Patient: {
        id: '123',
        name: 'John Doe',
        extension: [
          {
            url: 'https://example.com/ext1',
            valueString: 'value1',
          },
          {
            url: 'https://example.com/ext2',
            valueCodeableConcept: {
              coding: [
                {
                  system: 'https://example.com/system',
                  code: 'CODE1',
                },
              ],
            },
          },
        ],
      },
    };

    it('should find existing simple path', () => {
      const segments = parseFieldPath('Patient.name');
      const result = analyzePath(sampleJson, segments);
      
      expect(result).toHaveLength(2);
      expect(result[0].exists).toBe(true);
      expect(result[0].segment).toBe('Patient');
      expect(result[1].exists).toBe(true);
      expect(result[1].segment).toBe('name');
      expect(result[1].nodeValue).toBe('John Doe');
    });

    it('should detect missing leaf node', () => {
      const segments = parseFieldPath('Patient.missing');
      const result = analyzePath(sampleJson, segments);
      
      expect(result).toHaveLength(2);
      expect(result[0].exists).toBe(true);
      expect(result[1].exists).toBe(false);
      expect(result[1].isMissingParent).toBe(false);
    });

    it('should detect missing parent', () => {
      const segments = parseFieldPath('Patient.missingParent.child');
      const result = analyzePath(sampleJson, segments);
      
      expect(result).toHaveLength(3);
      expect(result[0].exists).toBe(true);
      expect(result[1].exists).toBe(false);
      expect(result[1].isMissingParent).toBe(false);
      expect(result[2].exists).toBe(false);
      expect(result[2].isMissingParent).toBe(true);
    });

    it('should handle filtered array - match found', () => {
      const segments = parseFieldPath('Patient.extension[url:https://example.com/ext2].valueCodeableConcept');
      const result = analyzePath(sampleJson, segments);
      
      expect(result).toHaveLength(3);
      expect(result[0].exists).toBe(true);
      expect(result[1].exists).toBe(true);
      expect(result[1].segment).toContain('extension[url:');
      expect(result[2].exists).toBe(true);
      expect(result[2].nodeValue).toHaveProperty('coding');
    });

    it('should handle filtered array - no match', () => {
      const segments = parseFieldPath('Patient.extension[url:https://notfound.com].valueString');
      const result = analyzePath(sampleJson, segments);
      
      expect(result[1].exists).toBe(false);
      expect(result[2].isMissingParent).toBe(true);
    });

    it('should handle array index - in bounds', () => {
      const segments = parseFieldPath('Patient.extension[0].url');
      const result = analyzePath(sampleJson, segments);
      
      expect(result[1].exists).toBe(true);
      expect(result[2].exists).toBe(true);
      expect(result[2].nodeValue).toBe('https://example.com/ext1');
    });

    it('should handle array index - out of bounds', () => {
      const segments = parseFieldPath('Patient.extension[10].url');
      const result = analyzePath(sampleJson, segments);
      
      expect(result[1].exists).toBe(false);
      expect(result[2].isMissingParent).toBe(true);
    });
  });

  describe('findFirstMissingSegment', () => {
    it('should return -1 when all segments exist', () => {
      const statuses = [
        { exists: true, segment: 'Patient', nodeValue: {}, isMissingParent: false, depth: 0 },
        { exists: true, segment: 'name', nodeValue: 'John', isMissingParent: false, depth: 1 },
      ];
      expect(findFirstMissingSegment(statuses)).toBe(-1);
    });

    it('should return index of first missing segment', () => {
      const statuses = [
        { exists: true, segment: 'Patient', nodeValue: {}, isMissingParent: false, depth: 0 },
        { exists: false, segment: 'missing', nodeValue: undefined, isMissingParent: false, depth: 1 },
        { exists: false, segment: 'child', nodeValue: undefined, isMissingParent: true, depth: 2 },
      ];
      expect(findFirstMissingSegment(statuses)).toBe(1);
    });
  });

  describe('buildPathString', () => {
    it('should build path string from segments', () => {
      const segments: PathSegment[] = [
        { type: 'property', name: 'Patient' },
        { type: 'array', name: 'extension', index: 0 },
        { type: 'property', name: 'url' },
      ];
      expect(buildPathString(segments)).toBe('Patient.extension[0].url');
    });
  });
});

describe('Integration Tests', () => {
  const testBundle = {
    resourceType: 'Bundle',
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
                    system: 'https://fhir.synapxe.sg/CodeSystem/ethnicity',
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

  it('should correctly analyze missing coding.display', () => {
    const path = 'Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].display';
    const segments = parseFieldPath(path);
    const statuses = analyzePath(testBundle.entry[0].resource, segments);
    
    const missingIndex = findFirstMissingSegment(statuses);
    expect(missingIndex).toBeGreaterThan(0);
    expect(statuses[missingIndex].segment).toBe('display');
  });

  it('should correctly analyze missing parent valueCodeableConcept', () => {
    const path = 'Patient.extension[url:https://notfound.com].valueCodeableConcept.coding[0].system';
    const segments = parseFieldPath(path);
    const statuses = analyzePath(testBundle.entry[0].resource, segments);
    
    const missingIndex = findFirstMissingSegment(statuses);
    expect(missingIndex).toBe(1); // extension filter doesn't match
    expect(statuses[2].isMissingParent).toBe(true);
  });
});
