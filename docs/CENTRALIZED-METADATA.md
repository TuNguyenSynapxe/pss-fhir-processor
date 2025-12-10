# Centralized Metadata Implementation Summary

## What Was Implemented

### **Single Source of Truth: Option A**

Metadata is now stored in **ONE location only**: `src/Pss.FhirProcessor/Metadata/validation-metadata.json`

All clients (Frontend, Tests, Sandbox API) will reference this single file.

---

## Architecture

```
src/Pss.FhirProcessor/
  Metadata/
    validation-metadata.json  ← SINGLE SOURCE OF TRUTH
                                 (Contains: Version, PathSyntax, RuleSets, CodesMaster)

Backend API:
  GET /api/metadata/validation  → Returns validation-metadata.json
  GET /api/metadata/info        → Returns metadata stats (version, counts, last modified)

Frontend:
  - Fetches from /api/metadata/validation on app load
  - Falls back to local copy for development mode
  - Shows indicator of metadata source (backend/local)

Tests:
  - Reference ../../Pss.FhirProcessor/Metadata/ directly
  - Same files as production

Sandbox API (Future):
  - Uses same MetadataController
  - Serves consistent rules to external clients
```

---

## Files Modified

### 1. **Backend - New MetadataController**
**File:** `Backend/Controllers/MetadataController.cs`

- **GET /api/metadata/validation** - Returns unified metadata JSON
- **GET /api/metadata/info** - Returns metadata statistics

```csharp
[HttpGet("validation")]
public IActionResult GetValidationMetadata()
{
    var metadataPath = Path.Combine(_env.ContentRootPath,
        "../../Pss.FhirProcessor/Metadata/validation-metadata.json");
    var json = System.IO.File.ReadAllText(metadataPath);
    return Content(json, "application/json");
}
```

### 2. **Frontend - Updated MetadataContext**
**File:** `Frontend/src/contexts/MetadataContext.jsx`

- Fetches from `/api/metadata/validation` first (backend API)
- Falls back to local `seed/validation-metadata.json` if API unavailable
- Exposes `metadataSource` ('backend', 'local', or 'none')

```javascript
// Try backend API first
const response = await fetch('/api/metadata/validation');
if (response.ok) {
  const metadata = await response.json();
  // Use backend metadata...
  setMetadataSource('backend');
} else {
  // Fallback to local...
  setMetadataSource('local');
}
```

---

## Benefits

✅ **Single Update Point** - Change metadata once in `Pss.FhirProcessor/Metadata/`  
✅ **Consistency** - All clients use exact same rules  
✅ **Version Control** - Metadata changes tracked in Git  
✅ **Ready for Sandbox API** - External clients get same rules  
✅ **Development Friendly** - Local fallback for offline development  
✅ **No Duplication** - Removed need to copy files across projects  

---

## How to Update Metadata

### ✏️ **To Update Validation Rules:**

1. Edit **ONLY** this file:
   ```
   src/Pss.FhirProcessor/Metadata/validation-metadata.json
   ```

2. All clients automatically get updates:
   - Frontend: Next page load fetches new version
   - Tests: Reference file directly
   - Sandbox API: Serves updated version immediately

### 🔄 **Frontend Refresh:**
```javascript
const { refreshMetadata } = useMetadata();
await refreshMetadata(); // Re-fetch from backend
```

---

## Future: Sandbox API

When implementing sandbox API with x-api-key authentication:

```csharp
[ApiController]
[Route("api/sandbox")]
public class SandboxController : ControllerBase
{
    private readonly MetadataController _metadataController;
    
    [HttpPost("process")]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public IActionResult Process([FromBody] SandboxRequest request)
    {
        // Load server-side metadata (single source)
        var metadataResponse = _metadataController.GetValidationMetadata();
        var metadata = /* extract JSON */;
        
        // Process with unified metadata
        var result = _fhirService.Process(
            request.FhirJson,
            metadata,
            request.LogLevel ?? "info",
            request.StrictDisplay ?? true
        );
        
        return Ok(result);
    }
}
```

**API consumers only send:**
- FHIR JSON
- Optional: logLevel, strictDisplay flags

**Server provides:**
- Validation metadata (rules + codes master)
- Consistent validation across all clients

---

## Migration Checklist

- [x] Created `MetadataController` with `/api/metadata/validation` endpoint
- [x] Updated `MetadataContext` to fetch from backend API
- [x] Added fallback to local metadata for development
- [x] Backend build successful
- [x] Frontend context updated
- [ ] Test the API endpoint (`/api/metadata/validation`)
- [ ] Verify frontend loads metadata from backend
- [ ] Update test projects to reference backend metadata path
- [ ] Remove duplicate metadata files (optional cleanup)
- [ ] Document for team

---

## Testing

### Test Backend API:
```bash
# Start backend
cd src/Pss.FhirProcessor.NetCore/Backend
dotnet run

# Test endpoint
curl http://localhost:5000/api/metadata/validation
curl http://localhost:5000/api/metadata/info
```

### Test Frontend:
```bash
# Start frontend (backend must be running)
cd src/Pss.FhirProcessor.NetCore/Frontend
npm run dev

# Check browser console:
# Should see: "✓ Metadata loaded from BACKEND API (single source of truth)"
```

---

## Rollback Plan

If issues arise, rollback is simple:

1. Frontend continues to work with local fallback
2. Remove `MetadataController.cs`
3. Revert `MetadataContext.jsx` to load only from local

No data loss risk - metadata file unchanged.
