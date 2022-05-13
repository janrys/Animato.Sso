namespace Animato.Sso.WebApi.Common;
using Animato.Sso.Application.Features.Assets;
using Animato.Sso.Application.Features.Partners;
using Animato.Sso.Application.Features.Transformations;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using MediatR;

public interface ICommandBuilder
{
    IAssetCommandBuilder Asset { get; }
    ITransformationCommandBuilder Transformation { get; }
}

public interface IAssetCommandBuilder
{
    Task<AssetMetadata> Create(CreateAsset asset);
    Task<AssetMetadata> Update(UpdateAsset asset);
    Task Delete(Guid id);
}

public interface ITransformationCommandBuilder
{
    Task<TransformationDefinition> Create(CreateTransformationDefinition transformation);
    Task<TransformationDefinition> Update(UpdateTransformationDefinition transformation);
    Task Delete(Guid id);
}


public interface IQueryBuilder
{
    IAssetQueryBuilder Asset { get; }
    ITransformationQueryBuilder Transformation { get; }
}

public interface IAssetQueryBuilder
{
    Task<IEnumerable<AssetMetadata>> GetAll();
    Task<AssetMetadata> GetById(Guid id);
}

public interface ITransformationQueryBuilder
{
    Task<IEnumerable<TransformationDefinition>> GetAll();
    Task<TransformationDefinition> GetById(Guid id);
    Task<IEnumerable<string>> GetRegistered();
}

public class CommandQueryBuilder : ICommandBuilder, IAssetCommandBuilder, ITransformationCommandBuilder
    , IQueryBuilder, IAssetQueryBuilder, ITransformationQueryBuilder

{
    private readonly ISender mediator;
    private readonly CancellationToken cancellationToken;

    public CommandQueryBuilder(ISender mediator) : this(mediator, CancellationToken.None) { }
    public CommandQueryBuilder(ISender mediator, CancellationToken cancellationToken)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        this.cancellationToken = cancellationToken;
    }

    IAssetCommandBuilder ICommandBuilder.Asset => this;
    IAssetQueryBuilder IQueryBuilder.Asset => this;
    ITransformationCommandBuilder ICommandBuilder.Transformation => this;
    ITransformationQueryBuilder IQueryBuilder.Transformation => this;

    Task<IEnumerable<AssetMetadata>> IAssetQueryBuilder.GetAll() => mediator.Send(new GetAssetsQuery(), cancellationToken);
    Task<AssetMetadata> IAssetQueryBuilder.GetById(Guid id) => mediator.Send(new GetAssetQuery(id), cancellationToken);
    Task<AssetMetadata> IAssetCommandBuilder.Create(CreateAsset asset)
        => mediator.Send(new CreateAssetCommand(asset), cancellationToken);
    Task<AssetMetadata> IAssetCommandBuilder.Update(UpdateAsset asset)
        => mediator.Send(new UpdateAssetCommand(asset), cancellationToken);
    Task IAssetCommandBuilder.Delete(Guid id) => mediator.Send(new DeleteAssetCommand(id), cancellationToken);
    Task<TransformationDefinition> ITransformationCommandBuilder.Create(CreateTransformationDefinition transformation)
        => mediator.Send(new CreateTransformationCommand(transformation), cancellationToken);
    Task<TransformationDefinition> ITransformationCommandBuilder.Update(UpdateTransformationDefinition transformation)
        => mediator.Send(new UpdateTransformationCommand(transformation), cancellationToken);
    Task ITransformationCommandBuilder.Delete(Guid id) => mediator.Send(new DeleteTransformationCommand(id), cancellationToken);
    Task<IEnumerable<TransformationDefinition>> ITransformationQueryBuilder.GetAll()
        => mediator.Send(new GetTransformationsQuery(), cancellationToken);
    Task<TransformationDefinition> ITransformationQueryBuilder.GetById(Guid id) => mediator.Send(new GetTransformationQuery(id), cancellationToken);
    Task<IEnumerable<string>> ITransformationQueryBuilder.GetRegistered() => mediator.Send(new GetRegisteredTransformationsQuery(), cancellationToken);
}
