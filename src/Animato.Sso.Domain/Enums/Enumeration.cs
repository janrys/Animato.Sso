namespace Animato.Sso.Domain.Enums;
using System.Reflection;

public abstract class Enumeration : IEnumeration, IComparable
{
    protected Enumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public int Value { get; }

    public string Name { get; }

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        var type = typeof(T);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        foreach (var info in fields)
        {
            if (info.GetValue(null) is T locatedValue)
            {
                yield return locatedValue;
            }
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        if (!GetType().Equals(obj?.GetType()))
        {
            return false;
        }

        return Value.Equals(otherValue.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
    {
        var absoluteDifference = Math.Abs(firstValue.Value - secondValue.Value);
        return absoluteDifference;
    }

    public static T FromValue<T>(int value) where T : Enumeration
    {
        var matchingItem = Parse<T, int>(value, "value", item => item.Value == value);
        return matchingItem;
    }

    public static T FromName<T>(string name) where T : Enumeration
    {
        var matchingItem = Parse<T, string>(name, "name", item => item.Name == name);
        return matchingItem;
    }

    private static TEnum Parse<TEnum, TValue>(TValue value, string description, Func<TEnum, bool> predicate) where TEnum : Enumeration
    {
        var matchingItem = GetAll<TEnum>().FirstOrDefault(predicate);

        if (matchingItem == null)
        {
            var message = $"'{value}' is not a valid {description} in {typeof(TEnum)}";
            throw new ArgumentOutOfRangeException(message);
        }

        return matchingItem;
    }

    public int CompareTo(object obj)
    {
        var value = obj as Enumeration;
        return Value.CompareTo(value);
    }

    public static bool operator ==(Enumeration left, Enumeration right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Enumeration left, Enumeration right) => !(left == right);

    public static bool operator <(Enumeration left, Enumeration right) => left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(Enumeration left, Enumeration right) => left is null || left.CompareTo(right) <= 0;

    public static bool operator >(Enumeration left, Enumeration right) => left is not null && left.CompareTo(right) > 0;

    public static bool operator >=(Enumeration left, Enumeration right) => left is null ? right is null : left.CompareTo(right) >= 0;
}
