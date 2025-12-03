
# TODO — PSS FHIR Processor
Central task tracker for DLL, WebApp, API, metadata, tests, and documentation.

---

# 1. Core DLL Implementation

## 1.1 Validation Engine
- [ ] Implement `ValidationEngine` class skeleton
- [ ] Implement metadata loader for RuleSets
- [ ] Implement metadata loader for Codes Master
- [ ] Implement `ValidationOptions` & `LoggingOptions`
- [ ] Implement PathResolver:
  - [ ] Dot navigation
  - [ ] Array index
  - [ ] Wildcard
  - [ ] Missing-path handling
- [ ] Implement rule evaluators:
  - [ ] Required
  - [ ] FixedValue
  - [ ] FixedCoding
  - [ ] AllowedValues
  - [ ] CodesMaster
  - [ ] Conditional
- [ ] Implement screening type existence checks (HS/OS/VS)
- [ ] Implement display matching (strict & normalized)
- [ ] Implement PureTone multi-value validator
- [ ] Implement structured error creation
- [ ] Implement logging (info/debug/verbose)
- [ ] Aggregate ValidationResult

## 1.2 Extraction Engine
- [ ] Implement `ExtractionEngine`
- [ ] Extract Event data
- [ ] Extract Participant data
- [ ] Extract HS screening
- [ ] Extract OS screening
- [ ] Extract VS screening
- [ ] Build ObservationItem structure
- [ ] Implement multi-value splitting
- [ ] Implement lookup helpers (GetHearing/GetOral/GetVision)

## 1.3 Shared Models
- [ ] FHIR minimal models
- [ ] Validation models
- [ ] Flatten models
- [ ] Codes Master models
- [ ] RuleSet models

---

# 2. Metadata (RuleSets + Codes Master)

## 2.1 RuleSets JSON
- [ ] Create RuleSets for Event/Participant/HS/OS/VS
- [ ] Include Required, FixedValue, FixedCoding, Conditional, CodesMaster rules
- [ ] Add umbrella CodesMaster rules
- [ ] Validate RuleSet JSON structure

## 2.2 Codes Master JSON
- [ ] Extract from Excel "Codes" tab
- [ ] Build JSON with:
  - questionCode
  - questionDisplay
  - screeningType
  - allowedAnswers[]
- [ ] Handle PureTone multi-value definitions

---

# 3. Playground WebApp

## 3.1 Setup
- [ ] ASP.NET MVC 4.7.2 project
- [ ] Controllers (Playground, Tests)
- [ ] Views (Razor)

## 3.2 Playground Page
- [ ] JSON Input
- [ ] Format JSON button
- [ ] Validate button
- [ ] Validate+Extract button
- [ ] Result tabs
- [ ] LogLevel dropdown
- [ ] Sample JSON selector

## 3.3 Test Cases Page
- [ ] Table of cases
- [ ] Run single
- [ ] Run all
- [ ] JSON modal
- [ ] Expected vs actual display

---

# 4. Public API

## 4.1 Endpoints
- [ ] POST /api/fhir/validate
- [ ] POST /api/fhir/extract
- [ ] POST /api/fhir/process
- [ ] GET  /api/fhir/codes-master
- [ ] GET  /api/fhir/rules

## 4.2 Models
- [ ] Standardized result wrapper
- [ ] Error + log output

---

# 5. Unit Tests

## 5.1 Function-Level Validation Tests
- Required rule
- FixedValue rule
- FixedCoding rule
- AllowedValues rule
- CodesMaster rule
- Conditional rules
- Display matching
- Screening types
- PureTone validation
- PathResolver logic
- Metadata loader

## 5.2 Extraction Tests
- Event extraction
- Participant extraction
- Screening extraction
- Multi-value splitting
- Lookup helpers

## 5.3 End-to-End Tests
- Valid bundle
- Missing HS/OS/VS
- Invalid display
- Invalid answer
- Invalid PureTone
- Conditional failure
- Combined errors
- Extraction correctness

## 5.4 Test Data
- Minimum 20 bundles
- 5 valid, 15 invalid

---

# 6. Documentation
- [ ] Finalize all docs in `/docs`
- [ ] README.md
- [ ] folder-structure.md
- [ ] Appendices
- [ ] Test plans

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

# END OF TODO.md
