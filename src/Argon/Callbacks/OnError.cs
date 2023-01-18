namespace Argon;

/// <summary>
/// Handles <see cref="JsonSerializer" /> serialization error callback events.
/// </summary>
public delegate void OnError(object? currentObject, object? originalObject, ErrorLocation location, Exception exception, Action markAsHandled);