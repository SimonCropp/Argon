namespace Argon;

/// <summary>
/// Specifies that the type should have its <see cref="OnSerialized"/> method called after serialization occurs.
/// </summary>
public interface IJsonOnSerialized
{
    /// <summary>
    /// The method that is called after serialization.
    /// </summary>
    void OnSerialized();
}