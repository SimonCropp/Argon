namespace Argon;

/// <summary>
/// Handles <see cref="JsonSerializer" /> deserialization error callback events.
/// </summary>
public delegate void OnDeserializeError(object? currentObject, object? originalObject, ErrorLocation location, Exception exception, Action markAsHandled);