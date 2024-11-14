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

    [field: AllowNull, MaybeNull]
    internal BidirectionalDictionary<string, object> DefaultReferenceMappings =>
        // override equality comparer for object key dictionary
        // object will be modified as it deserializes and might have mutable hashcode
        field ??= new(
            EqualityComparer<string>.Default,
            new ReferenceEqualsEqualityComparer(),
            "A different value already has the Id '{0}'.",
            "A different Id has already been assigned for value '{0}'. This error may be caused by an object being reused multiple times during deserialization and can be fixed with the setting ObjectCreationHandling.Replace.");

    protected NullValueHandling ResolvedNullValueHandling(JsonObjectContract? containerContract, JsonProperty property) =>
        property.NullValueHandling ??
        containerContract?.ItemNullValueHandling ??
        Serializer.NullValueHandling ??
        default;
}