#!/bin/bash

# Test script for Validation Helper System

echo "==================================="
echo "Testing PSS FHIR Validation Helper"
echo "==================================="
echo ""

# Test 1: Required field error
echo "Test 1: Testing Required field validation..."
curl -s -X POST http://localhost:5063/api/fhir/validate \
  -H "Content-Type: application/json" \
  -d '{
    "fhirJson": "{\"resourceType\":\"Bundle\",\"type\":\"transaction\",\"entry\":[{\"resource\":{\"resourceType\":\"Patient\"}}]}",
    "validationMetadata": "{\"Version\":\"11.0\",\"PathSyntax\":\"CPS1\",\"RuleSets\":[{\"Scope\":\"Patient\",\"Rules\":[{\"RuleType\":\"Required\",\"Path\":\"Patient.identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value\",\"ErrorCode\":\"MANDATORY_MISSING\",\"Message\":\"NRIC is mandatory.\"}],\"ScopeDefinition\":{\"ResourceType\":\"Patient\"}}],\"CodesMaster\":{\"Questions\":[],\"CodeSystems\":[]}}",
    "logLevel": "info"
  }' | jq '.validation.errors[0]' 2>/dev/null

echo ""
echo "-----------------------------------"
echo ""

# Check if we got enriched data
echo "Checking for enriched fields (ruleType, rule, context)..."
curl -s -X POST http://localhost:5063/api/fhir/validate \
  -H "Content-Type: application/json" \
  -d '{
    "fhirJson": "{\"resourceType\":\"Bundle\",\"type\":\"transaction\",\"entry\":[{\"resource\":{\"resourceType\":\"Patient\"}}]}",
    "validationMetadata": "{\"Version\":\"11.0\",\"PathSyntax\":\"CPS1\",\"RuleSets\":[{\"Scope\":\"Patient\",\"Rules\":[{\"RuleType\":\"Required\",\"Path\":\"Patient.id\",\"ErrorCode\":\"MANDATORY_MISSING\",\"Message\":\"Patient ID is required.\"}]}]}",
    "logLevel": "info"
  }' | jq '.validation.errors[0] | {code, ruleType, hasRule: (.rule != null), hasContext: (.context != null)}' 2>/dev/null

echo ""
echo "==================================="
echo "Test complete!"
echo "==================================="
