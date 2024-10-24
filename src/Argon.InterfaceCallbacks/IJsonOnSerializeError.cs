namespace Argon;

/// <summary>
/// Specifies that the type should have its <see cref="OnSerializeError"/> method called when a serialization error occurs.
/// </summary>
public interface IJsonOnSerializeError
{
    /// <summary>
    /// The method that is called on serialization error.
    /// </summary>
    void OnSerializeError(object? originalObject, ErrorLocation location, Exception exception, Action markAsHanded);
}