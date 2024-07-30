// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

abstract class JsonSerializerInternalBase(JsonSerializer serializer)
{
    class ReferenceEqualsEqualityComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object? x, object? y) =>
            ReferenceEquals(x, y);

        int IEqualityComparer<object>.GetHashCode(object obj) =>
            // put objects in a bucket based on their reference
            RuntimeHelpers.GetHashCode(obj);
    }

    ErrorContext? currentSerializeErrorContext;
    ErrorContext? currentDeserializeErrorContext;
    BidirectionalDictionary<string, object>? mappings;

    internal readonly JsonSerializer Serializer = serializer;
    protected JsonSerializerProxy? InternalSerializer;

    protected static bool HasFlag(DefaultValueHandling? value, DefaultValueHandling flag)
    {
        if (value == null)
        {
            return false;
        }

        return (value & flag) == flag;
    }

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

    protected void ClearSerializeErrorContext()
    {
        if (currentSerializeErrorContext == null)
        {
            throw new InvalidOperationException("Could not clear error context. Error context is already null.");
        }

        currentSerializeErrorContext = null;
    }
    protected void ClearDeserializeErrorContext()
    {
        if (currentDeserializeErrorContext == null)
        {
            throw new InvalidOperationException("Could not clear error context. Error context is already null.");
        }

        currentDeserializeErrorContext = null;
    }

    protected bool IsDeserializeErrorHandled(object? currentObject, object? member, string path, Exception exception)
    {
        if (currentDeserializeErrorContext == null)
        {
            currentDeserializeErrorContext = new(currentObject, exception);
        }
        else if (currentDeserializeErrorContext.Exception != exception)
        {
            throw new InvalidOperationException("Current error context error is different to requested error.");
        }

        void MarkAsHandled() =>
            currentDeserializeErrorContext.Handled = true;

        if (currentObject is IJsonOnError onError)
        {
            onError.OnError(currentDeserializeErrorContext.OriginalObject, new(path, member), exception, MarkAsHandled);
        }

        if (!currentDeserializeErrorContext.Handled)
        {
            Serializer.DeserializeError?.Invoke(currentObject, currentDeserializeErrorContext.OriginalObject, new(path, member), exception, MarkAsHandled);
        }

        return currentDeserializeErrorContext.Handled;
    }
    protected bool IsSerializeErrorHandled(object? currentObject, object? member, string path, Exception exception)
    {
        if (currentSerializeErrorContext == null)
        {
            currentSerializeErrorContext = new(currentObject, exception);
        }
        else if (currentSerializeErrorContext.Exception != exception)
        {
            throw new InvalidOperationException("Current error context error is different to requested error.");
        }

        void MarkAsHandled() =>
            currentSerializeErrorContext.Handled = true;

        if (currentObject is IJsonOnError onError)
        {
            onError.OnError(currentSerializeErrorContext.OriginalObject, new(path, member), exception, MarkAsHandled);
        }

        if (!currentSerializeErrorContext.Handled)
        {
            Serializer.SerializeError?.Invoke(currentObject, currentSerializeErrorContext.OriginalObject, new(path, member), exception, MarkAsHandled);
        }

        return currentSerializeErrorContext.Handled;
    }
}