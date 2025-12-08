#!/bin/bash

# PSS FHIR Processor - Azure Deployment Script
# This script deploys both backend and frontend to Azure App Service

set -e

# Configuration
RESOURCE_GROUP="FHIR"  # Use your existing resource group
LOCATION="southeastasia"
APP_NAME="pssfiiir"  # Your existing web app
APP_SERVICE_PLAN="ASP-FHIR-9949"  # Your existing app service plan
SKU="B1"

echo "================================"
echo "PSS FHIR Processor - Azure Deploy"
echo "================================"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo "❌ Azure CLI is not installed. Please install it first:"
    echo "   https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in to Azure
echo "🔐 Checking Azure login status..."
if ! az account show &> /dev/null; then
    echo "❌ Not logged in to Azure. Running 'az login'..."
    az login
fi

SUBSCRIPTION=$(az account show --query name -o tsv)
echo "✅ Logged in to Azure"
echo "   Subscription: $SUBSCRIPTION"
echo ""

# Create resource group if it doesn't exist
echo "📦 Creating resource group: $RESOURCE_GROUP"
az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output none

echo "✅ Resource group ready"
echo ""

# Create App Service Plan if it doesn't exist
echo "📋 Creating App Service Plan: $APP_SERVICE_PLAN"
if ! az appservice plan show \
    --name "$APP_SERVICE_PLAN" \
    --resource-group "$RESOURCE_GROUP" &> /dev/null; then
    
    az appservice plan create \
        --name "$APP_SERVICE_PLAN" \
        --resource-group "$RESOURCE_GROUP" \
        --sku "$SKU" \
        --is-linux \
        --output none
    
    echo "✅ App Service Plan created"
else
    echo "✅ App Service Plan already exists"
fi
echo ""

# Create Web App if it doesn't exist
echo "🌐 Creating Web App: $APP_NAME"
if ! az webapp show \
    --name "$APP_NAME" \
    --resource-group "$RESOURCE_GROUP" &> /dev/null; then
    
    az webapp create \
        --name "$APP_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --plan "$APP_SERVICE_PLAN" \
        --runtime "DOTNETCORE:8.0" \
        --output none
    
    echo "✅ Web App created"
else
    echo "✅ Web App already exists"
fi
echo ""

# Configure Web App settings
echo "⚙️  Configuring Web App settings..."
az webapp config appsettings set \
    --name "$APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --settings \
        ASPNETCORE_ENVIRONMENT=Production \
        WEBSITE_RUN_FROM_PACKAGE=1 \
    --output none

az webapp config set \
    --name "$APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --always-on true \
    --http20-enabled true \
    --min-tls-version 1.2 \
    --ftps-state Disabled \
    --output none

echo "✅ Web App configured"
echo ""

# Build Frontend
echo "🎨 Building Frontend..."
cd "$(dirname "$0")/../src/Pss.FhirProcessor.NetCore/Frontend"
npm ci --silent
npm run build
echo "✅ Frontend built"
echo ""

# Build Backend
echo "🔧 Building Backend..."
cd ../Backend
dotnet restore --verbosity quiet
dotnet build --configuration Release --verbosity quiet
dotnet publish --configuration Release --output ./publish --verbosity quiet
echo "✅ Backend built"
echo ""

# Copy Frontend to Backend wwwroot
echo "📂 Copying Frontend to Backend..."
mkdir -p ./publish/wwwroot
cp -r ../Frontend/dist/* ./publish/wwwroot/
echo "✅ Frontend copied"
echo ""

# Create deployment package
echo "📦 Creating deployment package..."
cd ./publish
zip -r ../deploy.zip . > /dev/null
cd ..
echo "✅ Deployment package created"
echo ""

# Deploy to Azure
echo "🚀 Deploying to Azure..."
az webapp deployment source config-zip \
    --name "$APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --src "./deploy.zip" \
    --output none

# Clean up
rm -f ./deploy.zip
echo "✅ Deployed successfully"
echo ""

# Get the URL
APP_URL=$(az webapp show \
    --name "$APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query defaultHostName -o tsv)

echo "================================"
echo "✅ Deployment Complete!"
echo "================================"
echo ""
echo "🌐 Application URL: https://$APP_URL"
echo "📊 API Endpoint: https://$APP_URL/api/fhir/validate"
echo ""
echo "💡 Next steps:"
echo "   1. Open https://$APP_URL in your browser"
echo "   2. Test the validation functionality"
echo "   3. Monitor logs: az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP"
echo ""
