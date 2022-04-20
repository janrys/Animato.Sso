namespace Animato.Sso.Infrastructure.Services.Persistence;

public class AzureTableStorageOptions
{
    public const string CONFIGURATION_KEY = "AzureTable";
    public string ConnectionString { get; set; }
    public string AssetTable { get; set; } = "assetmetadata";
    public string TransformationTable { get; set; } = "transformationdefinition";
}
