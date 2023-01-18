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

    ErrorContext GetErrorContext(object? currentObject, object? member, string path, Exception error)
    {
        if (currentErrorContext == null)
        {
            return currentErrorContext = new(currentObject, member, path, error);
        }

        if (currentErrorContext.Error == error)
        {
            return currentErrorContext;
        }

        throw new InvalidOperationException("Current error context error is different to requested error.");
    }

    protected void ClearErrorContext()
    {
        if (currentErrorContext == null)
        {
            throw new InvalidOperationException("Could not clear error context. Error context is already null.");
        }

        currentErrorContext = null;
    }

    protected bool IsErrorHandled(object? currentObject, JsonContract? contract, object? keyValue, string path, Exception exception)
    {
        var errorContext = GetErrorContext(currentObject, keyValue, path, exception);

        // attribute method is non-static so don't invoke if no object
        if (contract != null && currentObject != null)
        {
            contract.InvokeOnError(currentObject, Serializer.Context, errorContext);
        }

        if (!errorContext.Handled)
        {
            Serializer.OnError(new(currentObject, errorContext));
        }

        return errorContext.Handled;
    }
}