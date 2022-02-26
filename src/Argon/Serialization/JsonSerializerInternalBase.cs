// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using ErrorEventArgs = Argon.ErrorEventArgs;

abstract class JsonSerializerInternalBase
{
    class ReferenceEqualsEqualityComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object? x, object? y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            // put objects in a bucket based on their reference
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    ErrorContext? currentErrorContext;
    BidirectionalDictionary<string, object>? mappings;

    internal readonly JsonSerializer Serializer;
    internal readonly ITraceWriter? TraceWriter;
    protected JsonSerializerProxy? InternalSerializer;

    protected JsonSerializerInternalBase(JsonSerializer serializer)
    {
        Serializer = serializer;
        TraceWriter = serializer.TraceWriter;
    }

    internal BidirectionalDictionary<string, object> DefaultReferenceMappings
    {
        get
        {
            // override equality comparer for object key dictionary
            // object will be modified as it deserializes and might have mutable hashcode
            return mappings ??= new BidirectionalDictionary<string, object>(
                EqualityComparer<string>.Default,
                new ReferenceEqualsEqualityComparer(),
                "A different value already has the Id '{0}'.",
                "A different Id has already been assigned for value '{0}'. This error may be caused by an object being reused multiple times during deserialization and can be fixed with the setting ObjectCreationHandling.Replace.");
        }
    }

    protected NullValueHandling ResolvedNullValueHandling(JsonObjectContract? containerContract, JsonProperty property)
    {
        return property.NullValueHandling ??
               containerContract?.ItemNullValueHandling ??
               Serializer.NullValueHandling ??
               default;
    }

    ErrorContext GetErrorContext(object? currentObject, object? member, string path, Exception error)
    {
        currentErrorContext ??= new ErrorContext(currentObject, member, path, error);

        if (currentErrorContext.Error != error)
        {
            throw new InvalidOperationException("Current error context error is different to requested error.");
        }

        return currentErrorContext;
    }

    protected void ClearErrorContext()
    {
        if (currentErrorContext == null)
        {
            throw new InvalidOperationException("Could not clear error context. Error context is already null.");
        }

        currentErrorContext = null;
    }

    protected bool IsErrorHandled(object? currentObject, JsonContract? contract, object? keyValue, IJsonLineInfo? lineInfo, string path, Exception ex)
    {
        var errorContext = GetErrorContext(currentObject, keyValue, path, ex);

        if (TraceWriter is {LevelFilter: >= TraceLevel.Error} && !errorContext.Traced)
        {
            // only write error once
            errorContext.Traced = true;

            // kind of a hack but meh. might clean this up later
            var message = GetType() == typeof(JsonSerializerInternalWriter) ? "Error serializing" : "Error deserializing";
            if (contract != null)
            {
                message += $" {contract.UnderlyingType}";
            }
            message += $". {ex.Message}";

            // add line information to non-json.net exception message
            if (ex is not JsonException)
            {
                message = JsonPosition.FormatMessage(lineInfo, path, message);
            }

            TraceWriter.Trace(TraceLevel.Error, message, ex);
        }

        // attribute method is non-static so don't invoke if no object
        if (contract != null && currentObject != null)
        {
            contract.InvokeOnError(currentObject, Serializer.Context, errorContext);
        }

        if (!errorContext.Handled)
        {
            Serializer.OnError(new ErrorEventArgs(currentObject, errorContext));
        }

        return errorContext.Handled;
    }
}