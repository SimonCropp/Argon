namespace Argon;

/// <summary>
/// Sets extension data for an object during deserialization.
/// </summary>
/// <param name="key">The extension data key.</param>
/// <param name="value">The extension data value.</param>
public delegate void ExtensionDataSetter(object o, string key, object? value);