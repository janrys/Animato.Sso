# configuration
$environment = "test";
$systemPrefix = "sso";
$resourceLocation = "West Europe";
$skipLogin = $false;
$cleanResources = $true;


# setup variables and functions
$resourceNamePrefix = "$systemPrefix-$environment";
$startTime = Get-Date;
$foregroundColorInfo = "white";
$foregroundColorWarn = "yellow";
$foregroundColorError = "red";

Function LogMessage
{
    Param ($message, $color)

    Write-Host (Get-Date -Format "yyyyMMdd HH:mm:ss")"> $message" -ForegroundColor $color;
}

Function LogInfo
{
    Param ($message)
    LogMessage -message $message -color $foregroundColorInfo;
}

Function LogWarning
{
    Param ($message)
    LogMessage -message $message -color $foregroundColorWarn;
}

Function LogError
{
    Param ($message)
    LogMessage -message $message -color $foregroundColorError;
}


# prepare resource group names
$resourceGroup = "$resourceNamePrefix-rg";
$resourceGroupShared = "animato-$environment-shared-rg";

# prepare resource names
$appInsightsName = "animato-$environment-shared-apin";
$storageAccountName = $resourceNamePrefix.Replace("-", "") + "datasa01";
$appServicePlanName = "$resourceNamePrefix-asp";
$appServiceAPIName = "animato-$resourceNamePrefix-api";

# connect
if(!$skipLogin){
    az login --use-device-code;
}

# clean resources
if($cleanResources){
LogInfo -message "Cleaning resources";
az webapp delete -n $appServiceAPIName -g $resourceGroup;
az appservice plan delete -n $appServicePlanName -g $resourceGroup -y;
az storage account delete -n $storageAccountName -g $resourceGroup -y;
az group delete -n $resourceGroup -y;
LogInfo -message "Cleaning resources finished OK";
}

# create resources
LogInfo -message "Creating resources";

$statusMessage = "Creating resource group $resourceGroup";
LogInfo -message $statusMessage;
az group create -l $resourceLocation -n $resourceGroup;
LogInfo -message "$statusMessage finished OK";

$statusMessage = "Creating storage account $storageAccountName";
LogInfo -message $statusMessage;
az storage account create -n $storageAccountName -g $resourceGroup -l $resourceLocation --sku Standard_LRS --kind StorageV2;
LogInfo -message "$statusMessage finished OK";

$statusMessage = "Creating app service $appServicePlanName";
LogInfo -message $statusMessage;
az appservice plan create -n $appServicePlanName -g $resourceGroup -l $resourceLocation --sku F1;
LogInfo -message "$statusMessage finished OK";

$statusMessage = "Creating web site $appServiceAPIName";
LogInfo -message $statusMessage;
az webapp create -n $appServiceAPIName -g $resourceGroup -p $appServicePlanName --runtime "dotnet:6";

[String]$instrumentationKey = (az monitor app-insights component show --app $appInsightsName --resource-group $resourceGroupShared --query  "instrumentationKey" --output tsv)
az webapp config appsettings set --name $appServiceAPIName --resource-group $resourceGroup --settings APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=$instrumentationKey ApplicationInsightsAgent_EXTENSION_VERSION=~2
LogInfo -message "$statusMessage finished OK";

LogInfo -message "Creating resources finished OK";


