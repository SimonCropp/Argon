namespace Argon;

/// <summary>
/// Handles <see cref="JsonSerializer" /> serialization error callback events.
/// </summary>
public delegate void OnSerializeError(object? currentObject, object? originalObject, string path, object? member, Exception exception, Action markAsHandled);