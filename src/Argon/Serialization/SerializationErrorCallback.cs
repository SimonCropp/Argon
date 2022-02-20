namespace Argon;

/// <summary>
/// Handles <see cref="JsonSerializer"/> serialization error callback events.
/// </summary>
/// <param name="o">The object that raised the callback event.</param>
/// <param name="context">The streaming context.</param>
/// <param name="errorContext">The error context.</param>
public delegate void SerializationErrorCallback(object o, StreamingContext context, ErrorContext errorContext);