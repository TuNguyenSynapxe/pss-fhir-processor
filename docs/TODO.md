
# TODO — PSS FHIR Processor
Central task tracker for DLL, WebApp, API, metadata, tests, and documentation.

**Status: IMPLEMENTATION COMPLETE - 82/82 Tests Passing (100%)**

---

# 1. Core DLL Implementation

## 1.1 Validation Engine
- [x] Implement `ValidationEngine` class skeleton
- [x] Implement metadata loader for RuleSets
- [x] Implement metadata loader for Codes Master
- [x] Implement `ValidationOptions` & `LoggingOptions`
- [x] Implement PathResolver:
  - [x] Dot navigation
  - [x] Array index
  - [x] Wildcard
  - [x] Missing-path handling
- [x] Implement rule evaluators:
  - [x] Required
  - [x] FixedValue
  - [x] FixedCoding
  - [x] AllowedValues
  - [x] CodesMaster
  - [x] Conditional (with WhenValue support for value-based conditions)
- [x] Implement screening type existence checks (HS/OS/VS)
- [x] Implement display matching (strict & normalized)
- [x] Implement PureTone multi-value validator
- [x] Implement structured error creation
- [x] Implement logging (info/debug/verbose)
- [x] Aggregate ValidationResult

## 1.2 Extraction Engine
- [x] Implement `ExtractionEngine`
- [x] Extract Event data
- [x] Extract Participant data
- [x] Extract HS screening
- [x] Extract OS screening
- [x] Extract VS screening
- [x] Build ObservationItem structure
- [x] Implement multi-value splitting
- [x] Implement lookup helpers (GetHearing/GetOral/GetVision)

## 1.3 Shared Models
- [x] FHIR minimal models
- [x] Validation models
- [x] Flatten models
- [x] Codes Master models
- [x] RuleSet models

---

# 2. Metadata (RuleSets + Codes Master)

## 2.1 RuleSets JSON
- [x] Create RuleSets for Event/Participant/HS/OS/VS
- [x] Include Required, FixedValue, FixedCoding, Conditional, CodesMaster rules
- [x] Add umbrella CodesMaster rules
- [x] Validate RuleSet JSON structure

## 2.2 Codes Master JSON
- [x] Extract from Excel "Codes" tab
- [x] Build JSON with:
  - questionCode
  - questionDisplay
  - screeningType
  - allowedAnswers[]
- [x] Handle PureTone multi-value definitions

---

# 3. Playground WebApp

## 3.1 Setup
- [x] ASP.NET MVC 4.7.2 project
- [x] Controllers (Playground, Tests)
- [x] Views (Razor)

## 3.2 Playground Page
- [x] JSON Input
- [x] Format JSON button
- [x] Validate button
- [x] Validate+Extract button
- [x] Result tabs
- [x] LogLevel dropdown
- [x] Sample JSON selector

## 3.3 Test Cases Page
- [x] Table of cases
- [x] Run single
- [x] Run all
- [x] JSON modal
- [x] Expected vs actual display

---

# 4. Public API

## 4.1 Endpoints
- [x] POST /api/fhir/validate
- [x] POST /api/fhir/extract
- [x] POST /api/fhir/process
- [x] GET  /api/fhir/codes-master
- [x] GET  /api/fhir/rules

## 4.2 Models
- [x] Standardized result wrapper
- [x] Error + log output

---

# 5. Unit Tests

## 5.1 Function-Level Validation Tests
- [x] Required rule
- [x] FixedValue rule
- [x] FixedCoding rule
- [x] AllowedValues rule
- [x] CodesMaster rule
- [x] Conditional rules (JSONPath and Component modes)
- [x] Display matching
- [x] Screening types
- [x] PureTone validation
- [x] PathResolver logic
- [x] Metadata loader

## 5.2 Extraction Tests
- [x] Event extraction
- [x] Participant extraction
- [x] Screening extraction
- [x] Multi-value splitting
- [x] Lookup helpers

## 5.3 End-to-End Tests
- [x] Valid bundle
- [x] Missing HS/OS/VS
- [x] Invalid display
- [x] Invalid answer
- [x] Invalid PureTone
- [x] Conditional failure
- [x] Combined errors
- [x] Extraction correctness

## 5.4 Test Data
- [x] 82 test cases (100% passing)
- [x] Comprehensive coverage of all validation scenarios

---

# 6. Documentation
- [x] Finalize all docs in `/docs`
- [x] README.md
- [x] folder-structure.md
- [x] Appendices
- [x] Test plans
- [x] Logging guide

---

# 7. Deployment
- [ ] WebApp deployment
- [ ] DLL deployment for CRM plugin
- [ ] IIS configuration
- [ ] Configurable metadata paths

---

# 8. Optional Enhancements
- [ ] Auto-generate RuleSets from Excel
- [ ] Auto-generate CRM mapping template
- [ ] Metadata override upload via WebApp
- [ ] Vendor submission sandbox mode
- [ ] LLM-assisted rule builder

---

# Implementation Summary

**Completed**: Core DLL (Validation + Extraction), All Unit Tests, WebApp Playground, Public API
**Test Coverage**: 82/82 tests passing (100%)
**Remaining**: Production deployment, optional enhancements

---

# END OF TODO.md
