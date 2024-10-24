namespace Argon;

/// <summary>
/// Specifies that the type should have its <see cref="OnDeserializeError"/> method called when a deserialization error occurs.
/// </summary>
public interface IJsonOnDeserializeError
{
    void OnDeserializeError(object? originalObject, ErrorLocation location, Exception exception, Action markAsHanded);
}