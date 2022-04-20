namespace Animato.Sso.Application.Common.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;

public interface IMetadataStorageService
{
    Task Seed();
    Task<IEnumerable<AssetMetadata>> GetAssets(CancellationToken cancellationToken);
    Task<AssetMetadata> GetAsset(Guid id, CancellationToken cancellationToken);
    Task<AssetMetadata> InsertAsset(AssetMetadata asset, CancellationToken cancellationToken);
    Task<AssetMetadata> UpdateAsset(AssetMetadata asset, CancellationToken cancellationToken);
    Task DeleteAsset(Guid id, CancellationToken cancellationToken);
}
