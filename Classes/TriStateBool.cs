using Newtonsoft.Json.Linq;

namespace CkCommons.Classes;

/// <summary>
///     Based off OtterGui.OptionalBool, but with more utility.
/// </summary>
public readonly struct TriStateBool : IEquatable<TriStateBool>, IEquatable<bool?>, IEquatable<bool>
{
    private readonly byte _value;
    
    public static readonly TriStateBool True  = new(true);
    public static readonly TriStateBool False = new(false);
    public static readonly TriStateBool Null  = new();

    public TriStateBool()
        => _value = byte.MaxValue;

    public TriStateBool(bool? value)
        => _value = (byte)(value == null ? byte.MaxValue : value.Value ? 1 : 0);

    public bool HasValue
        => _value < 2;

    public bool? Value
        => _value switch
        {
            1 => true,
            0 => false,
            _ => null,
        };

    /// <summary> Update the TriStateBool's Value to a new one. </summary>
    /// <returns> A new TriStateBool with the updated value. </returns>
    public TriStateBool SetValue(bool? value)
        => new(value);

    /// <summary> Update the TriStateBool's Value to a next value in sequence. </summary>
    internal TriStateBool NextValue()
        => Value switch
        {
            null => True,
            true => False,
            false => Null,
        };

    /// <summary> Update the TriStateBool's Value to a previous value in sequence. </summary>
    internal TriStateBool PreviousValue()
        => Value switch
        {
            null => False,
            true => Null,
            false => True,
        };

    /// <summary> Attempt to parse out the TriStateBool from a string. </summary>
    /// <remarks> Usually useful for JParsing serialized values. </remarks>
    public static bool TryParse(string text, out TriStateBool b)
    {
        switch (text.ToLowerInvariant())
        {
            case "true":
                b = True;
                return true;
            case "false":
                b = False;
                return true;
            case "null":
                b = Null;
                return true;
            default:
                b = Null;
                return false;
        }
    }

    // Casting Helpers
    public static implicit operator TriStateBool(bool? v)
        => new(v);

    public static implicit operator TriStateBool(bool v)
        => new(v);

    public static implicit operator bool?(TriStateBool v)
        => v.Value;

    public static bool operator ==(TriStateBool left, TriStateBool right)
        => left.Equals(right);
    public static bool operator !=(TriStateBool left, TriStateBool right)
        => !left.Equals(right);

    // Equality Operators
    public bool Equals(TriStateBool other)
        => _value == other._value;

    public bool Equals(bool? other)
        => _value switch
        {
            1 when other != null => other.Value,
            0 when other != null => !other.Value,
            _ when other == null => true,
            _                    => false,
        };

    public bool Equals(bool other)
        => other ? _value == 1 : _value == 0;

    public override bool Equals(object? obj)
        => obj is TriStateBool other && Equals(other);

    public override int GetHashCode()
        => _value;

    public static TriStateBool FromJObject(JToken? token)
    {
        if (token is null || token.Value<string>() is not { } tokenValue)
            return TriStateBool.Null;

        return tokenValue.ToLowerInvariant() switch
        {
            "true" => TriStateBool.True,
            "false" => TriStateBool.False,
            "null" => TriStateBool.Null,
            _ => throw new ArgumentException("Invalid string value for OptionalBool")
        };
    }


    public override string ToString()
        => _value switch
        {
            1 => true.ToString(),
            0 => false.ToString(),
            _ => "null",
        };
}
