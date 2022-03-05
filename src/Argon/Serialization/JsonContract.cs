﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Contract details for a <see cref="System.Type" /> used by the <see cref="JsonSerializer" />.
/// </summary>
public abstract class JsonContract
{
    internal bool IsNullable;
    internal bool IsConvertable;
    internal bool IsEnum;
    internal Type NonNullableUnderlyingType;
    internal ReadType InternalReadType;
    internal JsonContractType ContractType;
    internal bool IsReadOnlyOrFixedSize;
    internal bool IsSealed;
    internal bool IsInstantiable;

    List<SerializationCallback>? onDeserializedCallbacks;
    List<SerializationCallback>? onDeserializingCallbacks;
    List<SerializationCallback>? onSerializedCallbacks;
    List<SerializationCallback>? onSerializingCallbacks;
    List<SerializationErrorCallback>? onErrorCallbacks;
    Type createdType;

    /// <summary>
    /// Gets the underlying type for the contract.
    /// </summary>
    public Type UnderlyingType { get; }

    /// <summary>
    /// Gets or sets the type created during deserialization.
    /// </summary>
    public Type CreatedType
    {
        get => createdType;
        set
        {
            createdType = value;

            IsSealed = createdType.IsSealed;
            IsInstantiable = !(createdType.IsInterface || createdType.IsAbstract);
        }
    }

    /// <summary>
    /// Gets or sets whether this type contract is serialized as a reference.
    /// </summary>
    public bool? IsReference { get; set; }

    /// <summary>
    /// Gets or sets the default <see cref="JsonConverter" /> for this contract.
    /// </summary>
    public JsonConverter? Converter { get; set; }

    /// <summary>
    /// Gets the internally resolved <see cref="JsonConverter" /> for the contract's type.
    /// This converter is used as a fallback converter when no other converter is resolved.
    /// Setting <see cref="Converter" /> will always override this converter.
    /// </summary>
    public JsonConverter? InternalConverter { get; internal set; }

    /// <summary>
    /// Gets or sets all methods called immediately after deserialization of the object.
    /// </summary>
    public List<SerializationCallback> OnDeserializedCallbacks => onDeserializedCallbacks ??= new();

    /// <summary>
    /// Gets or sets all methods called during deserialization of the object.
    /// </summary>
    public List<SerializationCallback> OnDeserializingCallbacks => onDeserializingCallbacks ??= new();

    /// <summary>
    /// Gets or sets all methods called after serialization of the object graph.
    /// </summary>
    public List<SerializationCallback> OnSerializedCallbacks => onSerializedCallbacks ??= new();

    /// <summary>
    /// Gets or sets all methods called before serialization of the object.
    /// </summary>
    public List<SerializationCallback> OnSerializingCallbacks => onSerializingCallbacks ??= new();

    /// <summary>
    /// Gets or sets all method called when an error is thrown during the serialization of the object.
    /// </summary>
    public List<SerializationErrorCallback> OnErrorCallbacks => onErrorCallbacks ??= new();

    /// <summary>
    /// Gets or sets the default creator method used to create the object.
    /// </summary>
    public Func<object>? DefaultCreator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the default creator is non-public.
    /// </summary>
    public bool DefaultCreatorNonPublic { get; set; }

    internal JsonContract(Type underlyingType)
    {
        UnderlyingType = underlyingType;

        // resolve ByRef types
        // typically comes from in and ref parameters on methods/ctors
        underlyingType = underlyingType.EnsureNotByRefType();

        IsNullable = underlyingType.IsNullable();

        if (IsNullable && underlyingType.IsNullableType())
        {
            NonNullableUnderlyingType = Nullable.GetUnderlyingType(underlyingType)!;
        }
        else
        {
            NonNullableUnderlyingType = underlyingType;
        }

        createdType = CreatedType = NonNullableUnderlyingType;

        IsConvertable = NonNullableUnderlyingType.IsConvertible();
        IsEnum = NonNullableUnderlyingType.IsEnum;

        InternalReadType = ReadType.Read;
    }

    internal void InvokeOnSerializing(object o, StreamingContext? context)
    {
        var contextToUse = ContextToUse(context);
        if (onSerializingCallbacks != null)
        {
            foreach (var callback in onSerializingCallbacks)
            {
                callback(o, contextToUse);
            }
        }
    }

    internal void InvokeOnSerialized(object o, StreamingContext? context)
    {
        var contextToUse = ContextToUse(context);
        if (onSerializedCallbacks != null)
        {
            foreach (var callback in onSerializedCallbacks)
            {
                callback(o, contextToUse);
            }
        }
    }

    internal void InvokeOnDeserializing(object o, StreamingContext? context)
    {
        var contextToUse = ContextToUse(context);
        if (onDeserializingCallbacks != null)
        {
            foreach (var callback in onDeserializingCallbacks)
            {
                callback(o, contextToUse);
            }
        }
    }

    internal void InvokeOnDeserialized(object o, StreamingContext? context)
    {
        var contextToUse = ContextToUse(context);
        if (onDeserializedCallbacks != null)
        {
            foreach (var callback in onDeserializedCallbacks)
            {
                callback(o, contextToUse);
            }
        }
    }

    internal void InvokeOnError(object o, StreamingContext? context, ErrorContext errorContext)
    {
        var contextToUse = ContextToUse(context);
        if (onErrorCallbacks != null)
        {
            foreach (var callback in onErrorCallbacks)
            {
                callback(o, contextToUse, errorContext);
            }
        }
    }

    static StreamingContext ContextToUse(StreamingContext? context)
    {
        var contextToUse = context ?? JsonSerializerSettings.DefaultContext;
        return contextToUse;
    }

    internal static SerializationCallback CreateSerializationCallback(MethodInfo callbackMethodInfo)
    {
        return (o, context) => callbackMethodInfo.Invoke(o, new object[] {context});
    }

    internal static SerializationErrorCallback CreateSerializationErrorCallback(MethodInfo callbackMethodInfo)
    {
        return (o, context, econtext) => callbackMethodInfo.Invoke(o, new object[] {context, econtext});
    }
}