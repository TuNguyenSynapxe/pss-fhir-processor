# Dynamic Metadata Management Implementation

## Overview
Implemented dynamic RuleSets and CodesMaster management allowing users to view and update validation metadata through the UI. All validation/extraction requests now use the user-configured metadata.

## Features Implemented

### 1. **Backend Changes**

#### New API Endpoints
- `PUT /api/fhir/rules` - Update RuleSets
- `PUT /api/fhir/codes-master` - Update CodesMaster

#### Updated Request Models
All request models now support optional metadata parameters:
- `ValidateRequest`
- `ExtractRequest`
- `ProcessRequest`

New properties:
```csharp
public Dictionary<string, string> RuleSets { get; set; }
public string CodesMaster { get; set; }
```

#### Service Layer Updates
`FhirProcessorService` now:
- Stores current metadata in memory (`_currentRuleSets`, `_currentCodesMaster`)
- Auto-seeds default values from `RuleSetSeed` and `CodesMasterSeed` on startup
- Accepts optional metadata in `Process()` method
- Provides `UpdateRuleSets()` and `UpdateCodesMaster()` methods
- Uses provided metadata per request or falls back to current/default

### 2. **Frontend Changes**

#### New Components

**MetadataContext** (`src/contexts/MetadataContext.jsx`)
- Global state management for RuleSets and CodesMaster
- Auto-loads metadata on app startup
- Provides `updateRuleSets()` and `updateCodesMaster()` functions
- Shared across Playground and ValidationRules pages

**MetadataEditor** (`src/components/MetadataEditor.jsx`)
- Modal-based JSON editor with tabs for RuleSets and CodesMaster
- Validates JSON syntax before saving
- Displays success/error messages
- Available in both Playground and ValidationRules pages

#### Updated Components

**App.jsx**
- Wrapped with `<MetadataProvider>` for global state

**Playground.jsx**
- Uses `useMetadata()` hook to access current metadata
- Sends `ruleSets` and `codesMaster` with every API request
- Shows "Edit Metadata" button in card header

**ValidationRules.jsx**
- Uses `useMetadata()` hook instead of local API calls
- Displays current metadata from context
- Shows "Edit Metadata" button in card header
- Updated `parseRules()` to handle both string and object formats

**api.js**
- Updated all endpoints to accept optional `ruleSets` and `codesMaster` parameters
- Added `updateRules()` and `updateCodesMaster()` functions

## User Workflow

### Viewing Current Metadata
1. Navigate to **Validation Rules** page
2. See all current RuleSets organized by scope (Event, Participant, HS, OS, VS)
3. View CodesMaster questions and code systems in tables

### Editing Metadata
1. Click **"Edit Metadata"** button (available in both Playground and Validation Rules pages)
2. Modal opens with 2 tabs:
   - **📋 Rule Sets**: JSON editor for validation rules
   - **📚 Codes Master**: JSON editor for questions and code systems
3. Edit JSON directly in the text area
4. Click **"Save Changes"** to update
5. System validates JSON syntax
6. On success, metadata is updated globally
7. All subsequent requests use the updated metadata

### Processing with Updated Metadata
1. Update metadata using the editor
2. Go to **Playground** page
3. Enter FHIR JSON and click **"Process"**
4. Backend automatically uses the updated RuleSets and CodesMaster
5. Validation results reflect the current metadata configuration

## Technical Details

### Default Behavior
- On app startup, backend loads default metadata from seed classes
- Frontend fetches and caches metadata in React context
- All requests include current metadata automatically

### Request Flow
```
User Input → Frontend (includes metadata from context) → Backend API
→ FhirProcessorService.Process(fhirJson, logLevel, strictDisplay, ruleSets, codesMaster)
→ Reload processor if metadata provided → Validate/Extract → Return results
```

### State Management
- **Backend**: Singleton service maintains current metadata in memory
- **Frontend**: React Context provides global state across all components
- **Sync**: Frontend updates trigger backend updates, then refresh local state

### Metadata Persistence
- Current implementation: **In-memory only** (resets on server restart)
- Default metadata always available from seed classes
- To persist: Add database storage or file-based persistence in future

## Benefits

1. **Flexibility**: Users can modify validation rules without code changes
2. **Real-time Testing**: Update rules and immediately test with sample data
3. **Shared State**: Both pages use the same metadata source
4. **Backward Compatible**: System works with default metadata if none provided
5. **No Deployment**: Rule changes don't require redeployment

## Future Enhancements

1. Add metadata versioning/history
2. Persist metadata to database or configuration files
3. Import/export metadata as JSON files
4. Add metadata validation (beyond JSON syntax)
5. Multiple metadata profiles (dev, staging, prod)
6. Audit log for metadata changes
