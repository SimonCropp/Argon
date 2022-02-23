namespace Argon;

/// <summary>
/// Gets extension data for an object during serialization.
/// </summary>
public delegate IEnumerable<KeyValuePair<object, object>>? ExtensionDataGetter(object o);