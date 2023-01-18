namespace Argon;

/// <summary>
/// Handles <see cref="JsonSerializer" /> serialization error callback events.
/// </summary>
/// <param name="current">The object that raised the callback event.</param>
/// <param name="errorContext">The error context.</param>
public delegate void OnError(object? current, ErrorContext errorContext);