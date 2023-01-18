// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

abstract class JsonSerializerInternalBase
{
    class ReferenceEqualsEqualityComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object? x, object? y) =>
            ReferenceEquals(x, y);

        int IEqualityComparer<object>.GetHashCode(object obj) =>
            // put objects in a bucket based on their reference
            RuntimeHelpers.GetHashCode(obj);
    }

    ErrorContext? currentErrorContext;
    BidirectionalDictionary<string, object>? mappings;

    internal readonly JsonSerializer Serializer;
    protected JsonSerializerProxy? InternalSerializer;

    protected JsonSerializerInternalBase(JsonSerializer serializer) =>
        Serializer = serializer;

    internal BidirectionalDictionary<string, object> DefaultReferenceMappings =>
        // override equality comparer for object key dictionary
        // object will be modified as it deserializes and might have mutable hashcode
        mappings ??= new(
            EqualityComparer<string>.Default,
            new ReferenceEqualsEqualityComparer(),
            "A different value already has the Id '{0}'.",
            "A different Id has already been assigned for value '{0}'. This error may be caused by an object being reused multiple times during deserialization and can be fixed with the setting ObjectCreationHandling.Replace.");

    protected NullValueHandling ResolvedNullValueHandling(JsonObjectContract? containerContract, JsonProperty property) =>
        property.NullValueHandling ??
        containerContract?.ItemNullValueHandling ??
        Serializer.NullValueHandling ??
        default;

    protected void ClearErrorContext()
    {
        if (currentErrorContext == null)
        {
            throw new InvalidOperationException("Could not clear error context. Error context is already null.");
        }

        currentErrorContext = null;
    }

    protected bool IsErrorHandled(object? currentObject, object? keyValue, string path, Exception exception)
    {
        if (currentErrorContext == null)
        {
            currentErrorContext = new(currentObject, keyValue, path, exception);
        }
        else if (currentErrorContext.Error != exception)
        {
            throw new InvalidOperationException("Current error context error is different to requested error.");
        }

        if (currentObject is IJsonOnError onError)
        {
            onError.OnError(currentErrorContext);
        }

        if (!currentErrorContext.Handled)
        {
            Serializer.Error?.Invoke(currentErrorContext);
        }

        return currentErrorContext.Handled;
    }
}