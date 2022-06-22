# configuration
$environment = "dev";
$systemPrefix = "kasab";
$resourceLocation = "West Europe";
$skipLogin = $true;
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
$resourceGroupShared = "$resourceNamePrefix-shared-rg";

# prepare resource names
$logAnalyticsWorkspaceName = "$resourceNamePrefix-law";
$appInsightsName = "$resourceNamePrefix-apin";
$keyVaultName = "$resourceNamePrefix-kv";
$storageAccountName = $systemPrefix + $environment + "datasa01";
$appServicePlanName = "$resourceNamePrefix-asp01";
$appServiceFEName = "$resourceNamePrefix-wfe-ws";
$appServiceBEName = "$resourceNamePrefix-wapi-ws";
$appServiceFunctionName = "$resourceNamePrefix-azf-fnc";
$cosmosDbName = "$resourceNamePrefix-cdb";
$searchServiceName = "$resourceNamePrefix-azs";
$redisCacheName = "$resourceNamePrefix-red";

# prepare PEN names
$keyVaultPENName = "$keyVaultName-pen";
$storageBlobPENName = "$resourceNamePrefix-blob-datasa01-pen";
$storageTablePENName = "$resourceNamePrefix-table-datasa01-pen";
$appServiceFEPENName = "$resourceNamePrefix-wfe-pen";
$appServiceBEPENName = "$resourceNamePrefix-wapi-pen";
$searchServicePENName = "$searchServiceName-pen";
$cosmosDbPENName = "$cosmosDbName-pen";
$redisCachePENName = "$redisCacheName-pen";

# connect
if(!$skipLogin){
    az login;
}

# clean resources
if($cleanResources){
LogInfo -message "Cleaning resources";
az monitor app-insights component delete -a $appInsightsName -g $resourceGroupShared;
az monitor log-analytics workspace delete  -n $logAnalyticsWorkspaceName -g $resourceGroupShared -f -y;
LogInfo -message "Cleaning resources finished OK";
}

# create resources
LogInfo -message "Creating resources";
$resourceExistenceCheck = az monitor log-analytics workspace list --query "[?name=='$logAnalyticsWorkspaceName']" | ConvertFrom-Json;

if ($resourceExistenceCheck.Length -ne 0) {
    LogError -message "Error creating log analytics worskpace. It already exists.";
    break;
}

$statusMessage = "Creating log analytics worskpace $logAnalyticsWorkspaceName";
LogInfo -message $statusMessage;
$logAnalyticsWorskpaceInstance = az monitor log-analytics workspace create -g $resourceGroupShared -n $logAnalyticsWorkspaceName -l $resourceLocation;
LogInfo -message "$statusMessage finished OK";


$resourceExistenceCheck = az monitor app-insights component show -a $appInsightsName -g $resourceGroupShared | ConvertFrom-Json;

if ($resourceExistenceCheck.Length -ne 0) {
    LogError -message "Error creating application insights. It already exists.";
    break;
}

$statusMessage = "Creating application insights $appInsightsName";
LogInfo -message $statusMessage;
az monitor app-insights component create -a $appInsightsName -l $resourceLocation -g $resourceGroupShared --workspace $logAnalyticsWorskpaceInstance.id;
LogInfo -message "$statusMessage finished OK";
LogInfo -message "Creating resources finished OK";


