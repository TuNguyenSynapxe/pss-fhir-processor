# Dynamic Metadata User Guide

## Quick Start

### Step 1: View Current Metadata
Navigate to the **Validation Rules** page to see all current validation rules and code systems.

### Step 2: Edit Metadata
Click the **"Edit Metadata"** button (blue button in top-right of page).

### Step 3: Modify Rules or Codes
A modal will open with two tabs:

**Tab 1: 📋 Rule Sets**
```json
{
  "Event": "{\"Scope\":\"Event\",\"ResourceType\":\"Encounter\",\"Rules\":[...]}",
  "Participant": "{\"Scope\":\"Participant\",\"ResourceType\":\"Patient\",\"Rules\":[...]}",
  "HS": "{\"Scope\":\"HS\",\"ResourceType\":\"Observation\",\"Rules\":[...]}",
  "OS": "{\"Scope\":\"OS\",\"ResourceType\":\"Observation\",\"Rules\":[...]}",
  "VS": "{\"Scope\":\"VS\",\"ResourceType\":\"Observation\",\"Rules\":[...]}"
}
```

**Tab 2: 📚 Codes Master**
```json
{
  "Questions": [...],
  "CodeSystems": [...]
}
```

### Step 4: Save Changes
- Edit the JSON directly
- Click **"Save Changes"**
- System validates your JSON
- Success message appears if valid

### Step 5: Test with New Metadata
- Go to **Playground** page
- Enter FHIR JSON
- Click **"Process"**
- Validation now uses your updated rules!

## Example: Adding a New Required Field

### Current Event Rules:
```json
{
  "Event": "{\"Scope\":\"Event\",\"ResourceType\":\"Encounter\",\"Rules\":[{\"RuleType\":\"Required\",\"Path\":\"actualPeriod.start\",\"ErrorCode\":\"MANDATORY_MISSING\",\"Message\":\"Screening date is required.\"}]}"
}
```

### Add New Rule for `status`:
```json
{
  "Event": "{\"Scope\":\"Event\",\"ResourceType\":\"Encounter\",\"Rules\":[{\"RuleType\":\"Required\",\"Path\":\"actualPeriod.start\",\"ErrorCode\":\"MANDATORY_MISSING\",\"Message\":\"Screening date is required.\"},{\"RuleType\":\"Required\",\"Path\":\"status\",\"ErrorCode\":\"MANDATORY_MISSING\",\"Message\":\"Encounter status is required.\"}]}"
}
```

### Result:
All subsequent validations will now check for `status` field!

## Example: Adding a New Question

### Current Questions (HS screening):
```json
{
  "Questions": [
    {
      "QuestionCode": "SQ-L2H9-00000001",
      "QuestionDisplay": "Is the participant currently wearing hearing aid(s)?",
      "ScreeningType": "HS",
      "AllowedAnswers": ["Yes", "No"],
      "IsMultiValue": false
    }
  ]
}
```

### Add New Question:
```json
{
  "Questions": [
    {
      "QuestionCode": "SQ-L2H9-00000001",
      "QuestionDisplay": "Is the participant currently wearing hearing aid(s)?",
      "ScreeningType": "HS",
      "AllowedAnswers": ["Yes", "No"],
      "IsMultiValue": false
    },
    {
      "QuestionCode": "SQ-NEW-00000099",
      "QuestionDisplay": "Has participant had hearing test in last 12 months?",
      "ScreeningType": "HS",
      "AllowedAnswers": ["Yes", "No", "Unknown"],
      "IsMultiValue": false
    }
  ]
}
```

### Result:
New question code `SQ-NEW-00000099` is now recognized in validation!

## Tips

### ✅ Do's
- ✅ Test your JSON syntax before saving
- ✅ Keep a backup of working metadata
- ✅ Make small incremental changes
- ✅ Test immediately after updating

### ❌ Don'ts
- ❌ Don't break JSON syntax (missing commas, brackets)
- ❌ Don't remove required properties from rules
- ❌ Don't duplicate question codes
- ❌ Don't use invalid rule types

## Troubleshooting

### "Invalid JSON format" Error
**Problem**: JSON syntax error (missing comma, bracket, quote)
**Solution**: Use online JSON validator or check console for specific error

### Changes Not Applied
**Problem**: Metadata not refreshing
**Solution**: Refresh the page or click "Edit Metadata" to verify changes saved

### Validation Still Uses Old Rules
**Problem**: Backend not picking up changes
**Solution**: 
1. Check if save was successful (green success message)
2. Refresh browser
3. Check browser console for errors

## Architecture Notes

- **Frontend**: React Context stores current metadata globally
- **Backend**: Singleton service maintains in-memory metadata
- **Auto-seed**: Default metadata loaded from seed classes on startup
- **Request-level**: Each API call includes current metadata
- **Persistence**: Currently in-memory only (resets on server restart)

For persistent storage, contact the development team to configure database-backed metadata management.
