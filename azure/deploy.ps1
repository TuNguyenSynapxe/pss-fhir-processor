# PSS FHIR Processor - Azure Deployment Script (PowerShell)
# This script deploys both backend and frontend to Azure App Service

# Configuration
$ResourceGroup = "FHIR"  # Use your existing resource group
$Location = "southeastasia"
$AppName = "pssfiiir"  # Your existing web app
$AppServicePlan = "ASP-FHIR-9949"  # Your existing app service plan
$SKU = "B1"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "PSS FHIR Processor - Azure Deploy" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Check if Azure CLI is installed
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Azure CLI is not installed. Please install it first:" -ForegroundColor Red
    Write-Host "   https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
}

# Check if logged in to Azure
Write-Host "🔐 Checking Azure login status..." -ForegroundColor Yellow
try {
    $account = az account show 2>$null | ConvertFrom-Json
    if ($null -eq $account) {
        Write-Host "❌ Not logged in to Azure. Running 'az login'..." -ForegroundColor Red
        az login
        $account = az account show | ConvertFrom-Json
    }
    Write-Host "✅ Logged in to Azure" -ForegroundColor Green
    Write-Host "   Subscription: $($account.name)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Azure login failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Create resource group
Write-Host "📦 Creating resource group: $ResourceGroup" -ForegroundColor Yellow
az group create `
    --name $ResourceGroup `
    --location $Location `
    --output none

Write-Host "✅ Resource group ready" -ForegroundColor Green
Write-Host ""

# Create App Service Plan
Write-Host "📋 Creating App Service Plan: $AppServicePlan" -ForegroundColor Yellow
$planExists = az appservice plan show --name $AppServicePlan --resource-group $ResourceGroup 2>$null
if ($null -eq $planExists) {
    az appservice plan create `
        --name $AppServicePlan `
        --resource-group $ResourceGroup `
        --sku $SKU `
        --is-linux `
        --output none
    Write-Host "✅ App Service Plan created" -ForegroundColor Green
} else {
    Write-Host "✅ App Service Plan already exists" -ForegroundColor Green
}
Write-Host ""

# Create Web App
Write-Host "🌐 Creating Web App: $AppName" -ForegroundColor Yellow
$webAppExists = az webapp show --name $AppName --resource-group $ResourceGroup 2>$null
if ($null -eq $webAppExists) {
    az webapp create `
        --name $AppName `
        --resource-group $ResourceGroup `
        --plan $AppServicePlan `
        --runtime "DOTNETCORE:8.0" `
        --output none
    Write-Host "✅ Web App created" -ForegroundColor Green
} else {
    Write-Host "✅ Web App already exists" -ForegroundColor Green
}
Write-Host ""

# Configure Web App
Write-Host "⚙️  Configuring Web App settings..." -ForegroundColor Yellow
az webapp config appsettings set `
    --name $AppName `
    --resource-group $ResourceGroup `
    --settings `
        ASPNETCORE_ENVIRONMENT=Production `
        WEBSITE_RUN_FROM_PACKAGE=1 `
    --output none

az webapp config set `
    --name $AppName `
    --resource-group $ResourceGroup `
    --always-on true `
    --http20-enabled true `
    --min-tls-version 1.2 `
    --ftps-state Disabled `
    --output none

Write-Host "✅ Web App configured" -ForegroundColor Green
Write-Host ""

# Build Frontend
Write-Host "🎨 Building Frontend..." -ForegroundColor Yellow
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location "$scriptDir\..\src\Pss.FhirProcessor.NetCore\Frontend"
npm ci --silent
npm run build
Write-Host "✅ Frontend built" -ForegroundColor Green
Write-Host ""

# Build Backend
Write-Host "🔧 Building Backend..." -ForegroundColor Yellow
Push-Location "..\Backend"
dotnet restore --verbosity quiet
dotnet build --configuration Release --verbosity quiet
dotnet publish --configuration Release --output .\publish --verbosity quiet
Write-Host "✅ Backend built" -ForegroundColor Green
Write-Host ""

# Copy Frontend to Backend
Write-Host "📂 Copying Frontend to Backend..." -ForegroundColor Yellow
New-Item -Path ".\publish\wwwroot" -ItemType Directory -Force | Out-Null
Copy-Item -Path "..\Frontend\dist\*" -Destination ".\publish\wwwroot\" -Recurse -Force
Write-Host "✅ Frontend copied" -ForegroundColor Green
Write-Host ""

# Create deployment package
Write-Host "📦 Creating deployment package..." -ForegroundColor Yellow
Push-Location ".\publish"
Compress-Archive -Path "*" -DestinationPath "..\deploy.zip" -Force
Pop-Location
Write-Host "✅ Deployment package created" -ForegroundColor Green
Write-Host ""

# Deploy to Azure
Write-Host "🚀 Deploying to Azure..." -ForegroundColor Yellow
az webapp deployment source config-zip `
    --name $AppName `
    --resource-group $ResourceGroup `
    --src ".\deploy.zip" `
    --output none

# Clean up
Remove-Item ".\deploy.zip" -Force
Write-Host "✅ Deployed successfully" -ForegroundColor Green
Write-Host ""

# Get the URL
$appUrl = (az webapp show --name $AppName --resource-group $ResourceGroup --query defaultHostName -o tsv)

Pop-Location
Pop-Location

Write-Host "================================" -ForegroundColor Cyan
Write-Host "✅ Deployment Complete!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "🌐 Application URL: https://$appUrl" -ForegroundColor White
Write-Host "📊 API Endpoint: https://$appUrl/api/fhir/validate" -ForegroundColor White
Write-Host ""
Write-Host "💡 Next steps:" -ForegroundColor Yellow
Write-Host "   1. Open https://$appUrl in your browser"
Write-Host "   2. Test the validation functionality"
Write-Host "   3. Monitor logs: az webapp log tail --name $AppName --resource-group $ResourceGroup"
Write-Host ""
