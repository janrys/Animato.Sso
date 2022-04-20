namespace Animato.Sso.Application.Common.Interfaces;

using System.IO;
using System.Threading.Tasks;
using Animato.Sso.Application.Models;

public interface IFileStorageService
{
    Task Seed();
    Task<AssetLocation> Save(Stream stream, Guid id, string name, string contentType, CancellationToken cancellationToken);
    Task Delete(Guid id, CancellationToken cancellationToken);
}
