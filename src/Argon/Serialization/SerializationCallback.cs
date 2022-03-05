namespace Argon;

/// <summary>
/// Handles <see cref="JsonSerializer" /> serialization callback events.
/// </summary>
/// <param name="o">The object that raised the callback event.</param>
/// <param name="context">The streaming context.</param>
public delegate void SerializationCallback(object o, StreamingContext context);