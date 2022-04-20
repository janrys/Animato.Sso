namespace Animato.Sso.Domain.Entities;

public class TransformationDefinition
{
    public const string TRANSFORMATION_SPLIT = "|";

    public TransformationDefinition() : this(Guid.NewGuid()) { }

    public TransformationDefinition(Guid id) => Id = id;

    public Guid Id { get; set; }
    public string Definition { get; set; }
    public string Description { get; set; }
}

public static class TransformationDefinitionExtensions
{


    public static IEnumerable<string> GetTransformations(this TransformationDefinition definition)
    {
        if (definition is null || string.IsNullOrEmpty(definition.Definition))
        {
            return Array.Empty<string>();
        }

        return definition.Definition.Split(TransformationDefinition.TRANSFORMATION_SPLIT);
    }
}
