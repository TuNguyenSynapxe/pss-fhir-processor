# Documentation Index
PSS FHIR Processor — Complete Documentation Guide  
Version 1.0 — December 2025

---

## Quick Start

**For Developers:**
1. Start with [01-overview.md](./01-overview.md) for project context
2. Read [02-architecture.md](./02-architecture.md) for system design
3. Check [10-implementation-guide.md](./10-implementation-guide.md) for setup

**For Vendors:**
1. Review [03-fhir-spec.md](./03-fhir-spec.md) for FHIR requirements
2. Check [14-validation-rules-reference.md](./14-validation-rules-reference.md) for validation rules
3. Use the Playground (see [08-webapp-playground.md](./08-webapp-playground.md)) for testing

**For QA/Testing:**
1. Review [11-unit-test-plan.md](./11-unit-test-plan.md) for test strategy
2. Check [13-appendix-part1.md](./13-appendix-part1.md) and [13-appendix-part2.md](./13-appendix-part2.md) for sample data

---

## Core Documentation (01-12)

### [01 — Overview](./01-overview.md)
Project introduction, goals, and high-level architecture overview.

### [02 — Architecture](./02-architecture.md)
System design, component interaction, and architectural patterns.

### [03 — FHIR Specification](./03-fhir-spec.md)
FHIR Bundle structure, resource requirements, and PSS-specific profiles.

### [04 — Data Model](./04-data-model.md)
Internal data structures, extraction models, and field mappings.

### [05 — Validation Engine](./05-validation-engine.md)
Validation engine design, rule evaluation logic, and error handling.

### [06 — Extraction Engine](./06-extraction-engine.md)
Data extraction process, field mapping, and output structure.

### [07 — CRM Mapping](./07-crm-mapping.md)
Mapping from FHIR to CRM entities, field transformations.

### [08 — WebApp Playground](./08-webapp-playground.md)
Interactive testing interface, playground features, and usage guide.

### [09 — Public API](./09-public-api.md)
REST API endpoints, request/response formats, authentication.

### [10 — Implementation Guide](./10-implementation-guide.md)
Setup instructions, configuration, deployment steps.

### [11 — Unit Test Plan](./11-unit-test-plan.md)
Testing strategy, test categories, coverage requirements.

### [12 — Deployment Guide](./12-deployment-guide.md)
Production deployment, IIS configuration, environment setup.

---

## Appendices (13)

### [13 — Appendix Part 1](./13-appendix-part1.md)
Sample FHIR bundles (valid/invalid), validation errors, codes master excerpts.

### [13 — Appendix Part 2](./13-appendix-part2.md)
Sample extraction results, observation items, RuleSet examples.

---

## Reference Documentation (14-15)

### [14 — Validation Rules Reference](./14-validation-rules-reference.md) ⭐
**Complete guide to all validation rule types:**
- Required, Type, Regex
- FixedValue, FixedCoding, AllowedValues
- CodesMaster, Conditional
- Reference, FullUrlIdMatch

Includes usage examples, error messages, and best practices.

### [15 — Implementation Notes](./15-implementation-notes.md) ⭐
**Advanced features and implementation details:**
- Dynamic metadata management
- GUID-URI validation
- Reference validation
- FullUrlIdMatch validation
- Regex patterns
- Multi-value support
- Logging system
- Test coverage

---

## Specialized Guides

### [LOGGING_GUIDE.md](./LOGGING_GUIDE.md)
Comprehensive logging configuration, log levels, output formats.

### [metadata-user-guide.md](./metadata-user-guide.md)
How to configure and manage validation metadata (RuleSets & CodesMaster).

### [folder-structure.md](./folder-structure.md)
Complete project directory structure and file organization.

### [TODO.md](./TODO.md)
Project status, completed features, remaining work, test results.

---

## Documentation by Role

### **Software Engineers**
1. [01-overview.md](./01-overview.md) - Understand the system
2. [02-architecture.md](./02-architecture.md) - See the design
3. [05-validation-engine.md](./05-validation-engine.md) - Core validation logic
4. [06-extraction-engine.md](./06-extraction-engine.md) - Data extraction
5. [10-implementation-guide.md](./10-implementation-guide.md) - Get started
6. [15-implementation-notes.md](./15-implementation-notes.md) - Advanced features

### **QA / Testing**
1. [11-unit-test-plan.md](./11-unit-test-plan.md) - Test strategy
2. [13-appendix-part1.md](./13-appendix-part1.md) - Sample bundles
3. [13-appendix-part2.md](./13-appendix-part2.md) - Expected outputs
4. [14-validation-rules-reference.md](./14-validation-rules-reference.md) - All validation rules

### **Vendors / Integration Partners**
1. [03-fhir-spec.md](./03-fhir-spec.md) - FHIR requirements
2. [14-validation-rules-reference.md](./14-validation-rules-reference.md) - Validation rules
3. [08-webapp-playground.md](./08-webapp-playground.md) - Test your submissions
4. [09-public-api.md](./09-public-api.md) - API integration

### **DevOps / System Administrators**
1. [10-implementation-guide.md](./10-implementation-guide.md) - Setup
2. [12-deployment-guide.md](./12-deployment-guide.md) - Production deployment
3. [LOGGING_GUIDE.md](./LOGGING_GUIDE.md) - Logging configuration
4. [folder-structure.md](./folder-structure.md) - Directory structure

### **Business Analysts / Project Managers**
1. [01-overview.md](./01-overview.md) - Project overview
2. [03-fhir-spec.md](./03-fhir-spec.md) - Business requirements
3. [TODO.md](./TODO.md) - Project status
4. [14-validation-rules-reference.md](./14-validation-rules-reference.md) - Business rules

---

## Documentation Standards

### File Naming Convention
- **01-12**: Sequential core documentation
- **13**: Appendices (multiple parts)
- **14-15**: Reference documentation
- **ALL_CAPS.md**: Specialized guides

### Version Control
All documents include:
- Title and description
- Version number
- Last updated date
- Status indicator

### Update Process
1. Make changes to relevant document
2. Update version/date in header
3. Update this index if needed
4. Update TODO.md status

---

## Recent Changes (December 2025)

### Added
- ✅ [14-validation-rules-reference.md](./14-validation-rules-reference.md) - Comprehensive validation rules guide
- ✅ [15-implementation-notes.md](./15-implementation-notes.md) - Advanced features documentation

### Consolidated
- ✅ Merged 3 type-validation docs into [14-validation-rules-reference.md](./14-validation-rules-reference.md)
- ✅ Merged 2 implementation guides into [15-implementation-notes.md](./15-implementation-notes.md)

### Removed
- ✅ `type-validation-examples.md` (consolidated)
- ✅ `type-validation-quick-reference.md` (consolidated)
- ✅ `type-validation-implementation-summary.md` (consolidated)
- ✅ `dynamic-metadata-implementation.md` (consolidated)
- ✅ `guid-uri-and-regex-implementation.md` (consolidated)
- ✅ `validation-engine-testplan.md` (superseded by [11-unit-test-plan.md](./11-unit-test-plan.md))

### Updated
- ✅ [TODO.md](./TODO.md) - Reflects 213/213 tests passing, new features
- ✅ [README.md](../README.md) - Updated test count and documentation structure

---

## Document Statistics

**Total Documents:** 20
- Core documentation: 12
- Appendices: 2
- Reference guides: 2
- Specialized guides: 4

**Total Pages (estimated):** ~150+ pages
**Test Coverage:** 213/213 (100%)
**Last Major Update:** December 6, 2025

---

## Contributing to Documentation

### Guidelines
1. Keep language clear and concise
2. Use code examples liberally
3. Include visual aids where helpful
4. Update index when adding new docs
5. Follow naming conventions
6. Include version/date in headers

### Review Process
1. Technical accuracy review
2. Completeness check
3. Format/style consistency
4. Update references and cross-links

---

## Need Help?

**Can't find what you're looking for?**
1. Check this index for the right document
2. Use the role-based guide above
3. Search for keywords in relevant documents
4. Check [TODO.md](./TODO.md) for status updates

**Found an issue?**
- Document errors or gaps
- Submit for review
- Update and increment version

---

**Maintained by:** PSS FHIR Processor Team  
**Last Updated:** December 6, 2025  
**Version:** 1.0
