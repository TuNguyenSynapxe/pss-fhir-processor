
# 13 — Appendix & Sample Bundles (Part 2/2)

# 6. Sample FlattenResult

```json
{
  "event": {
    "start": "2025-01-10T09:00:00+08:00",
    "end": "2025-01-10T09:20:00+08:00",
    "venueName": "ABC CC",
    "postalCode": "123456",
    "grc": "Ang Mo Kio GRC",
    "constituency": "Ang Mo Kio",
    "providerName": "XYZ Provider",
    "clusterName": "NHG"
  },
  "participant": {
    "nric": "S1234567A",
    "name": "John Tan",
    "gender": "male",
    "birthDate": "1950-01-01"
  }
}
```

---

# 7. Sample Hearing Screening ObservationItem

```json
{
  "question": {
    "code": "SQ-L2H9-00000010",
    "display": "Pure Tone @ 500Hz (Left)"
  },
  "values": [
    "500Hz – R",
    "500Hz – NR"
  ]
}
```

---

# 8. RuleSet Excerpt (HS)

```json
{
  "Scope": "HS",
  "Rules": [
    { "RuleType": "Required", "Path": "Observation.component[]" },
    { "RuleType": "CodesMaster", "Path": "Observation.component" }
  ]
}
```

---

# 9. Help for Vendors

- Ensure all 3 screening types exist  
- Ensure all displays exactly match  
- Use correct Codes Master values  
- Multi-value answers must be pipe-separated  
- Use WebApp Playground before submitting to CRM  

---

# 10. Summary

This appendix provides:
- Examples of correct/incorrect bundles  
- Examples of extraction output  
- Metadata references  
- Developer & vendor guidance  

---

# END OF FILE 13
