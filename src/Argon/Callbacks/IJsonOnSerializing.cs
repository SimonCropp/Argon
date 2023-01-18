namespace Argon;

/// <summary>
/// Specifies that the type should have its <see cref="OnSerializing"/> method called before serialization occurs.
/// </summary>
public interface IJsonOnSerializing
{
    /// <summary>
    /// The method that is called before serialization.
    /// </summary>
    void OnSerializing();
}