# Azure Deployment Checklist - Seed File Management

## ✅ Pre-Deployment Verification

### 1. **Seed Files Must Exist**
Verify these files exist in `Frontend/src/seed/`:
```bash
ls -lh src/Pss.FhirProcessor.NetCore/Frontend/src/seed/
```

Required files:
- ✅ `happy-sample-full.json` (25KB)
- ✅ `validation-metadata.json` (49KB)

### 2. **GitHub Actions Workflow**
The CI/CD pipeline **automatically**:
- ✅ Builds frontend with seed files
- ✅ Copies frontend to `Backend/wwwroot`
- ✅ **NEW**: Copies seed files to `publish/seed/` folder
- ✅ Includes `init-azure-seed.sh` script
- ✅ Packages everything for deployment

**No manual intervention needed for CI/CD!**

## 🔧 Azure Configuration

### **Required App Service Setting**

In Azure Portal → App Service → Configuration → Application settings:

```
WEBSITE_RUN_FROM_PACKAGE = 0
```

⚠️ **CRITICAL**: Set this **BEFORE** first deployment to allow file writes.

## 📁 File Storage Architecture

### Local Development
```
Frontend/src/seed/
├── happy-sample-full.json
└── validation-metadata.json
```

### Azure Production
```
/home/data/seed/
├── happy-sample-full.json
└── validation-metadata.json
```

## 🚀 Deployment Steps

### **Automatic** (via GitHub Actions)

1. **Push to main branch**
   ```bash
   git push origin main
   ```

2. **GitHub Actions automatically:**
   - ✅ Builds frontend
   - ✅ Builds backend
   - ✅ Copies seed files to deployment package
   - ✅ Deploys to Azure

### **Manual One-Time Setup** (After First Deployment)

**Only needed ONCE** on the very first deployment:

#### **Step 1: Set App Service Configuration**
Azure Portal → Your App Service → Configuration → Application settings
```
WEBSITE_RUN_FROM_PACKAGE = 0
```
Save and restart the app.

#### **Step 2: Copy Seed Files to /home/data/seed/**

Choose **any** of these methods:

---

##### **Option A: Azure Portal - Kudu File Manager** (Easiest, No CLI)

1. Go to: `https://pssfhir.scm.azurewebsites.net`
2. Click **"Debug console"** → **"CMD"** or **"PowerShell"**
3. Navigate to `/home/site/wwwroot`
4. Run the init script:
   ```bash
   bash init-azure-seed.sh
   ```
   
Or manually create and copy:
- Create folder: `mkdir /home/data/seed`
- Drag & drop files from `seed/` folder to `/home/data/seed/`

---

##### **Option B: Azure Portal - SSH / Cloud Shell**

1. Azure Portal → Your App Service → **SSH** (in Development Tools)
2. Run:
   ```bash
   mkdir -p /home/data/seed
   cd /home/site/wwwroot/seed
   cp happy-sample-full.json /home/data/seed/
   cp validation-metadata.json /home/data/seed/
   ```

---

##### **Option C: Azure Portal - FTP Upload**

1. Azure Portal → Your App Service → **Deployment Center**
2. Copy FTP credentials (username/password)
3. Use any FTP client (FileZilla, WinSCP):
   - Connect to your FTP endpoint
   - Navigate to `/site/wwwroot`
   - Create `/home/data/seed` folder
   - Upload both JSON files

---

##### **Option D: VS Code Azure Extension** (Developer Friendly)

1. Install "Azure App Service" extension in VS Code
2. Sign in to Azure
3. Right-click your App Service → **Browse Files**
4. Navigate to `/home/data/seed/`
5. Right-click → **Upload Files** → Select both JSON files

---

##### **Option E: Azure CLI** (Command Line)

```bash
# Login
az login

# Upload files
az webapp deploy --resource-group FHIR --name pssfhir \
  --src-path ./Frontend/src/seed/happy-sample-full.json \
  --type static --target-path /home/data/seed/happy-sample-full.json

az webapp deploy --resource-group FHIR --name pssfhir \
  --src-path ./Frontend/src/seed/validation-metadata.json \
  --type static --target-path /home/data/seed/validation-metadata.json
```

---

##### **Option F: PowerShell Script** (Windows)

```powershell
# Set variables
$resourceGroup = "FHIR"
$webAppName = "pssfhir"
$kuduUrl = "https://$webAppName.scm.azurewebsites.net/api/vfs/home/data/seed/"

# Get publish credentials
$credentials = az webapp deployment list-publishing-credentials `
  --name $webAppName --resource-group $resourceGroup | ConvertFrom-Json

$base64 = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$($credentials.publishingUserName):$($credentials.publishingPassword)"))

# Upload files
Invoke-RestMethod -Uri "$kuduUrl/happy-sample-full.json" `
  -Method PUT -InFile "./Frontend/src/seed/happy-sample-full.json" `
  -Headers @{Authorization = "Basic $base64"}

Invoke-RestMethod -Uri "$kuduUrl/validation-metadata.json" `
  -Method PUT -InFile "./Frontend/src/seed/validation-metadata.json" `
  -Headers @{Authorization = "Basic $base64"}
```

---

#### **Verify Files Were Copied**

Using any method above, verify:
```bash
ls -lh /home/data/seed/
```

You should see both files with correct sizes (~25KB and ~49KB).

### **All Future Deployments**

✅ **Fully automatic** - Just push to main branch!

Files in `/home/data/seed/` persist across deployments.

## 🔄 How It Works

### Environment Detection
```csharp
// Detects Azure via WEBSITE_INSTANCE_ID environment variable
if (Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") != null)
{
    // Azure: Use /home/data/seed (persistent)
}
else
{
    // Local: Use Frontend/src/seed
}
```

### Dynamic Loading
1. **validation-metadata.json**
   - Endpoint: `GET /api/metadata/validation`
   - Loaded dynamically on every request
   - Updates take effect immediately

2. **happy-sample-full.json**
   - Endpoint: `GET /api/seed/public/happy-sample-full.json`
   - Loaded when user clicks "Load Sample"
   - Updates take effect immediately

### Admin Panel Updates
- Edit files via Admin panel → Changes persist in `/home/data/seed/`
- All users see updated files immediately (no restart needed)

## 🧪 Testing Checklist

After deployment, verify:

- [ ] Navigate to Admin page
- [ ] Login with password: `Synapxe@CRM@PSS`
- [ ] See both seed files loaded in Monaco editor
- [ ] Make a small edit and save
- [ ] Verify file saved successfully
- [ ] Reload page - changes should persist
- [ ] Go to Playground - Load Sample should work
- [ ] Validation should use updated metadata

## 🔐 Security

- ✅ Read operations: No authentication required
- ✅ Write operations: Password-protected (Admin panel only)
- ✅ Password: `Synapxe@CRM@PSS` (hardcoded in backend)

## 📊 Monitoring

Check Application Logs for:
```
SeedFileService initialized with path: /home/data/seed
Reading seed file: /home/data/seed/validation-metadata.json
Successfully saved seed file: /home/data/seed/happy-sample-full.json
```

## ⚠️ Known Limitations

1. **File Size Limit**: No hard limit, but keep files reasonable (<100MB)
2. **Concurrent Writes**: Last write wins (no file locking)
3. **Backup**: Files in `/home` are backed up by Azure, but consider manual backups for critical changes

## 🐛 Troubleshooting

### Seed files not found
```
Solution: Run init-azure-seed.sh or manually copy files to /home/data/seed/
```

### Changes not persisting
```
Solution: Verify WEBSITE_RUN_FROM_PACKAGE = 0 in App Settings
```

### Permission errors
```
Solution: /home directory should be writable by default in App Service
```

## ✅ Summary

| Aspect | Status |
|--------|--------|
| **Local Development** | ✅ Works with Frontend/src/seed/ |
| **Azure Detection** | ✅ Auto-detects via WEBSITE_INSTANCE_ID |
| **File Persistence** | ✅ Files in /home/data/seed/ persist |
| **Dynamic Loading** | ✅ Changes take effect immediately |
| **Admin Panel** | ✅ Edit and save via web interface |
| **Current Files** | ✅ Both files exist and ready |

**Ready for Azure deployment!** 🚀
