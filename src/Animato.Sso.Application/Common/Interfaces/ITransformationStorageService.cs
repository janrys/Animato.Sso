namespace Animato.Sso.Application.Common.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface ITransformationStorageService
{
    Task Seed();
    Task<IEnumerable<TransformationDefinition>> GetTransformations(CancellationToken cancellationToken);
    Task<TransformationDefinition> GetTransformation(Guid id, CancellationToken cancellationToken);
    Task<TransformationDefinition> InsertTransformation(TransformationDefinition transformation, CancellationToken cancellationToken);
    Task<TransformationDefinition> UpdateTransformation(TransformationDefinition transformation, CancellationToken cancellationToken);
    Task DeleteTransformation(Guid id, CancellationToken cancellationToken);
}
