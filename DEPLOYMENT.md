# PSS FHIR Processor - Deployment Guide

## Azure Web App Configuration

**Web App:** `pssfiiir.azurewebsites.net`  
**Resource Group:** `FHIR`  
**App Service Plan:** `ASP-FHIR-9949 (B1: 1)`  
**Runtime:** `.NET Core 8.0 on Linux`

---

## Option 1: GitHub Actions CI/CD (Recommended)

### Setup Steps

1. **Get Azure Publish Profile**
   
   In Azure Portal:
   - Navigate to your Web App: `pssfiiir`
   - Click **"Download publish profile"** in the top toolbar
   - Save the `.PublishSettings` file

2. **Add GitHub Secret**
   
   In your GitHub repository:
   - Go to **Settings** → **Secrets and variables** → **Actions**
   - Click **"New repository secret"**
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Paste the entire contents of the `.PublishSettings` file
   - Click **"Add secret"**

3. **Enable GitHub Actions**
   
   The workflow file is already configured at `.github/workflows/azure-deploy.yml`
   
   - Push to `main` branch triggers automatic deployment
   - Manual deployment: Go to **Actions** tab → **Deploy to Azure Web App** → **Run workflow**

4. **Verify Deployment**
   
   After the workflow completes:
   - Open: https://pssfiiir.azurewebsites.net
   - Test API: https://pssfiiir.azurewebsites.net/api/fhir/validate

### Workflow Details

The GitHub Actions workflow:
- ✅ Builds frontend (React + Vite)
- ✅ Builds backend (.NET 8)
- ✅ Copies frontend to backend `wwwroot`
- ✅ Deploys combined package to Azure
- ✅ Runs on every push to `main` branch

---

## Option 2: Manual Deployment (PowerShell)

### Prerequisites
- Azure CLI installed
- .NET 8 SDK installed
- Node.js 18+ installed

### Steps

```powershell
cd azure
.\deploy.ps1
```

The script will:
1. Login to Azure (if needed)
2. Verify resource group and web app exist
3. Build frontend
4. Build backend
5. Copy frontend to backend wwwroot
6. Deploy to Azure

---

## Option 3: Manual Deployment (Bash)

### Prerequisites
- Azure CLI installed
- .NET 8 SDK installed
- Node.js 18+ installed

### Steps

```bash
cd azure
chmod +x deploy.sh
./deploy.sh
```

---

## Architecture

```
┌─────────────────────────────────────────┐
│    Azure App Service (pssfiiir)         │
├─────────────────────────────────────────┤
│                                         │
│  Backend (.NET 8)                       │
│  ├─ /api/fhir/*  → API Controllers      │
│  └─ /*           → Static Files (SPA)   │
│                                         │
│  Frontend (React + Vite)                │
│  └─ wwwroot/                            │
│      ├─ index.html                      │
│      ├─ assets/                         │
│      └─ ...                             │
│                                         │
└─────────────────────────────────────────┘
```

### How it works:

1. **API Requests** (`/api/*`):
   - Handled by ASP.NET Core controllers
   - Returns JSON responses

2. **Frontend Requests** (`/*`):
   - Served as static files from `wwwroot`
   - SPA fallback to `index.html` for client-side routing

3. **Production Configuration**:
   - Frontend uses relative URLs (`/api/fhir/...`)
   - No CORS needed (same origin)
   - HTTPS enforced

---

## Verify Deployment

### 1. Check Web App Status

```bash
az webapp show \
  --name pssfiiir \
  --resource-group FHIR \
  --query "{name:name, state:state, defaultHostName:defaultHostName}"
```

### 2. Test Frontend

Open in browser:
```
https://pssfiiir.azurewebsites.net
```

You should see the PSS FHIR Processor playground interface.

### 3. Test API

```bash
curl -X POST https://pssfiiir.azurewebsites.net/api/fhir/test-cases \
  -H "Content-Type: application/json"
```

Or use Postman/Thunder Client:
```
GET https://pssfiiir.azurewebsites.net/api/fhir/test-cases
```

### 4. Check Logs

```bash
# Stream logs
az webapp log tail \
  --name pssfiiir \
  --resource-group FHIR

# Download logs
az webapp log download \
  --name pssfiiir \
  --resource-group FHIR \
  --log-file logs.zip
```

---

## Configuration

### Environment Variables

Already configured in Azure Web App:
- `ASPNETCORE_ENVIRONMENT=Production`
- `WEBSITE_RUN_FROM_PACKAGE=1`

### App Settings

To add custom settings:

```bash
az webapp config appsettings set \
  --name pssfiiir \
  --resource-group FHIR \
  --settings KEY=VALUE
```

---

## Troubleshooting

### Issue: Deployment fails

**Solution 1**: Check GitHub Actions logs
- Go to **Actions** tab in GitHub
- Click on the failed workflow
- Review error messages

**Solution 2**: Verify publish profile
- Download fresh publish profile from Azure
- Update GitHub secret

### Issue: API returns 404

**Solution**: Verify Program.cs configuration
```csharp
// Should have:
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
```

### Issue: Frontend shows blank page

**Solution 1**: Check browser console for errors
- May be API connection issue
- Verify `/api` routes are working

**Solution 2**: Rebuild and redeploy
```bash
cd azure
.\deploy.ps1  # or ./deploy.sh
```

### Issue: CORS errors

**Solution**: In production, CORS should not be needed
- Frontend and backend are same origin
- If you see CORS errors, check frontend API URL configuration

---

## Monitoring

### Application Insights (Optional)

To enable Application Insights:

1. Create Application Insights resource
2. Connect to Web App:
```bash
az webapp config appsettings set \
  --name pssfiiir \
  --resource-group FHIR \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="<your-connection-string>"
```

### Health Checks

The app automatically configures:
- Always On: `true`
- HTTP/2: `enabled`
- TLS 1.2: `minimum version`

---

## Scaling

### Vertical Scaling (Upgrade Plan)

```bash
# Scale up to S1 (better performance)
az appservice plan update \
  --name ASP-FHIR-9949 \
  --resource-group FHIR \
  --sku S1
```

### Horizontal Scaling (Add Instances)

```bash
# Scale out to 2 instances
az appservice plan update \
  --name ASP-FHIR-9949 \
  --resource-group FHIR \
  --number-of-workers 2
```

---

## Security Checklist

- ✅ HTTPS only (enforced)
- ✅ TLS 1.2 minimum
- ✅ FTP disabled
- ✅ Always On enabled
- ⚠️ Consider adding authentication (Azure AD, API keys)
- ⚠️ Consider adding rate limiting
- ⚠️ Set up custom domain (optional)

---

## Custom Domain (Optional)

To use a custom domain:

1. Add custom domain in Azure Portal
2. Configure DNS records
3. Add SSL certificate (free with Azure)

```bash
# Add custom domain
az webapp config hostname add \
  --webapp-name pssfiiir \
  --resource-group FHIR \
  --hostname yourdomain.com

# Bind SSL certificate
az webapp config ssl bind \
  --name pssfiiir \
  --resource-group FHIR \
  --certificate-thumbprint <thumbprint> \
  --ssl-type SNI
```

---

## Backup & Recovery

### Backup Settings

```bash
# Configure backup
az webapp config backup create \
  --resource-group FHIR \
  --webapp-name pssfiiir \
  --backup-name manual-backup \
  --storage-account-url "<sas-url>"
```

### Quick Rollback

If deployment fails:
1. Go to Azure Portal → Deployment Center
2. Click "Disconnect" to stop CI/CD temporarily
3. Use "Swap slots" or "Rollback" feature
4. Reconnect CI/CD after fixing issues

---

## Support

For issues or questions:
- Check Azure App Service logs
- Review GitHub Actions workflow logs
- Test locally first: `dotnet run` (backend) and `npm run dev` (frontend)
- Verify all environment variables are set correctly

---

## Quick Commands Reference

```bash
# Deploy manually
cd azure && ./deploy.sh

# Check app status
az webapp show --name pssfiiir --resource-group FHIR

# Stream logs
az webapp log tail --name pssfiiir --resource-group FHIR

# Restart app
az webapp restart --name pssfiiir --resource-group FHIR

# Open in browser
az webapp browse --name pssfiiir --resource-group FHIR
```
