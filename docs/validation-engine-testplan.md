# ValidationEngine – Comprehensive Unit Test Plan

## Overview
This document describes a comprehensive set of unit tests for the `ValidationEngine` class. The class performs metadata-driven validation of FHIR Bundles.

## Primary Features to Test
1. Structural validation of Bundle
2. Resource indexing logic
3. Required resource validation
4. Screening type validation (HS / OS / VS)
5. RuleSet evaluations:
   - Required
   - FixedValue
   - FixedCoding
   - AllowedValues
   - Conditional (JSONPath + Component modes)
6. CodesMaster metadata validation
7. Option-dependent behaviors (Strict, Normalize)
8. Error aggregation behavior
9. Logging behavior (optional)

---

## A. Structural Validation
| Test | Input | Expected |
|-----|-------|----------|
| A1 | Null bundle | INVALID_BUNDLE_STRUCTURE |
| A2 | Bundle with null entries | INVALID_BUNDLE_STRUCTURE |
| A3 | Empty entries | Missing required resources |

---

## B. Resource Indexing
| Test | Input | Expected |
|-----|-------|----------|
| B1 | Patient + Encounter | Index contains both |
| B2 | Observations w/ HS OS VS | Correct grouping keys |
| B3 | Observation without code | Skipped safely |
| B4 | Mixed resource types | No crash |

---

## C. Required Resources
| Test | Input | Expected |
|-----|-------|----------|
| C1 | Missing all required | One error per missing resource |
| C2 | All required exist | No errors |
| C3 | Some missing | Appropriate error |

---

## D. Screening Types
| Test | Input | Expected |
|-----|-------|----------|
| D1 | No observations | 3 missing errors |
| D2 | Only HS present | OS/VS missing |
| D3 | All present | Pass |
| D4 | Null-coded observations | Count as missing |

---

## E. RuleSets

### 1. Required Rule
- Field exists → PASS  
- Field missing → MANDATORY_MISSING  

### 2. FixedValue Rule
- Matching value → PASS  
- Non-matching → FIXED_VALUE_MISMATCH  

### 3. FixedCoding Rule
- Wrong system/code → FIXED_CODING_MISMATCH  
- JObject/scalar → handled safely  

### 4. AllowedValues Rule
- Valid value → PASS  
- Invalid value → INVALID_ANSWER_VALUE  

### 5. Conditional Rule (JSONPath)
- If exists, Then missing → CONDITIONAL_FAILED  
- Both exist → PASS  

### 6. Conditional Rule (Component)
- Condition met, Then missing → CONDITIONAL_FAILED  

---

## F. CodesMaster Validation
| Test | Case | Expected |
|-----|------|----------|
| F1 | Unknown question code | UNKNOWN_QUESTION_CODE |
| F2 | Wrong screening type | INVALID_SCREENING_TYPE_FOR_QUESTION |
| F3 | Missing display | QUESTION_DISPLAY_MISSING |
| F4 | Display mismatch | QUESTION_DISPLAY_MISMATCH |
| F5 | Allowed answers: missing match | INVALID_ANSWER_VALUE |
| F6 | Multi-value: invalid | INVALID_MULTI_VALUE |

---

## G. Options Behavior
| Test | Mode | Expected |
|-----|------|----------|
| G1 | Default | Lenient display match |
| G2 | StrictMatch | Must match exactly |
| G3 | Normalize | Case/space ignore |

---

## H. Error Aggregation
- Multiple independent errors collected  
- Not fail-fast except null bundle  

---

## I. End-to-End Scenarios
| Test | Input | Expected |
|-----|-------|----------|
| I1 | Valid bundle | IsValid = true |
| I2 | Multiple failures | IsValid = false; Errors > 1 |

---

## J. Edge Cases
| Test | Case | Expected |
|-----|------|----------|
| J1 | RuleSet with 0 rules | skip |
| J2 | Null rule object | skip |
| J3 | Unsupported rule type | skip |

---

## Suggested Test Tools & Libraries
- xUnit or NUnit
- FluentAssertions
- Moq / NSubstitute

---

## Expected Output Format
Each test must validate:
- `result.IsValid`
- Error count and error codes
- Specific field paths when applicable

---

## Notes for Implementation
- Create helper factories for Bundle and Observation
- Tests must not call external services
- Prioritize readability and isolation

---