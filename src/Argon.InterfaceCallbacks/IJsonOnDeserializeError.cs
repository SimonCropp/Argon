namespace Argon;

/// <summary>
/// Specifies that the type should have its <see cref="OnDeserializeError"/> method called when a deserialization error occurs.
/// </summary>
public interface IJsonOnDeserializeError
{
    void OnDeserializeError(object? originalObject, string path, object? member, Exception exception, Action markAsHanded);
}