namespace Argon;

/// <summary>
/// Handles <see cref="JsonSerializer" /> serialization error callback events.
/// </summary>
public delegate void OnError(object? currentObject, object? originalObject, object? member, string path, Exception error, Action markAsHandled);