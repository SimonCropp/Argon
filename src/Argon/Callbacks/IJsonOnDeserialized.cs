namespace Argon;

/// <summary>
/// Specifies that the JSON type should have its <see cref="OnDeserialized"/> method called after deserialization occurs.
/// </summary>
public interface IJsonOnDeserialized
{
    /// <summary>
    /// The method that is called after deserialization.
    /// </summary>
    void OnDeserialized();
}