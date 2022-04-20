namespace Animato.Sso.Infrastructure.Services.Persistence;

public class AzureBlobStorageOptions
{
    public const string CONFIGURATION_KEY = "AzureBlob";
    public string ConnectionString { get; set; }
    public string AssetContainer { get; set; } = "assets";
}
