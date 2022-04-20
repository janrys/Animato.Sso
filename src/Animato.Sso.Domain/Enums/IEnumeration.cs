namespace Animato.Sso.Domain.Enums;

public interface IEnumeration : IComparable
{
    int Value { get; }

    string Name { get; }
}
