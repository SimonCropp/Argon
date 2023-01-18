namespace Argon;

/// <summary>
/// Specifies that the type should have its <see cref="OnError"/> method called when a serialization error occurs.
/// </summary>
public interface IJsonOnError
{
    /// <summary>
    /// The method that is called after serialization.
    /// </summary>
    void OnError(object? originalObject, object? member, string path, Exception error, Action markAsHanded);
}