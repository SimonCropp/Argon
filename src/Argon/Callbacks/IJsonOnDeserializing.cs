namespace Argon;

/// <summary>
/// Specifies that the type should have its <see cref="OnDeserializing"/> method called before deserialization occurs.
/// </summary>
public interface IJsonOnDeserializing
{
    /// <summary>
    /// The method that is called before deserialization.
    /// </summary>
    void OnDeserializing();
}