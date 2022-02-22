#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

namespace Argon;

/// <summary>
/// Contract details for a <see cref="System.Type"/> used by the <see cref="JsonSerializer"/>.
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
    /// Gets the internally resolved <see cref="JsonConverter"/> for the contract's type.
    /// This converter is used as a fallback converter when no other converter is resolved.
    /// Setting <see cref="Converter"/> will always override this converter.
    /// </summary>
    public JsonConverter? InternalConverter { get; internal set; }

    /// <summary>
    /// Gets or sets all methods called immediately after deserialization of the object.
    /// </summary>
    public IList<SerializationCallback> OnDeserializedCallbacks => onDeserializedCallbacks ??= new List<SerializationCallback>();

    /// <summary>
    /// Gets or sets all methods called during deserialization of the object.
    /// </summary>
    public IList<SerializationCallback> OnDeserializingCallbacks => onDeserializingCallbacks ??= new List<SerializationCallback>();

    /// <summary>
    /// Gets or sets all methods called after serialization of the object graph.
    /// </summary>
    public IList<SerializationCallback> OnSerializedCallbacks => onSerializedCallbacks ??= new List<SerializationCallback>();

    /// <summary>
    /// Gets or sets all methods called before serialization of the object.
    /// </summary>
    public IList<SerializationCallback> OnSerializingCallbacks => onSerializingCallbacks ??= new List<SerializationCallback>();

    /// <summary>
    /// Gets or sets all method called when an error is thrown during the serialization of the object.
    /// </summary>
    public IList<SerializationErrorCallback> OnErrorCallbacks => onErrorCallbacks ??= new List<SerializationErrorCallback>();

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
        underlyingType = ReflectionUtils.EnsureNotByRefType(underlyingType);

        IsNullable = ReflectionUtils.IsNullable(underlyingType);

        NonNullableUnderlyingType = IsNullable && ReflectionUtils.IsNullableType(underlyingType) ? Nullable.GetUnderlyingType(underlyingType) : underlyingType;

        createdType = CreatedType = NonNullableUnderlyingType;

        IsConvertable = NonNullableUnderlyingType.IsConvertible();
        IsEnum = NonNullableUnderlyingType.IsEnum;

        InternalReadType = ReadType.Read;
    }

    internal void InvokeOnSerializing(object o, StreamingContext context)
    {
        if (onSerializingCallbacks != null)
        {
            foreach (var callback in onSerializingCallbacks)
            {
                callback(o, context);
            }
        }
    }

    internal void InvokeOnSerialized(object o, StreamingContext context)
    {
        if (onSerializedCallbacks != null)
        {
            foreach (var callback in onSerializedCallbacks)
            {
                callback(o, context);
            }
        }
    }

    internal void InvokeOnDeserializing(object o, StreamingContext context)
    {
        if (onDeserializingCallbacks != null)
        {
            foreach (var callback in onDeserializingCallbacks)
            {
                callback(o, context);
            }
        }
    }

    internal void InvokeOnDeserialized(object o, StreamingContext context)
    {
        if (onDeserializedCallbacks != null)
        {
            foreach (var callback in onDeserializedCallbacks)
            {
                callback(o, context);
            }
        }
    }

    internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
    {
        if (onErrorCallbacks != null)
        {
            foreach (var callback in onErrorCallbacks)
            {
                callback(o, context, errorContext);
            }
        }
    }

    internal static SerializationCallback CreateSerializationCallback(MethodInfo callbackMethodInfo)
    {
        return (o, context) => callbackMethodInfo.Invoke(o, new object[] { context });
    }

    internal static SerializationErrorCallback CreateSerializationErrorCallback(MethodInfo callbackMethodInfo)
    {
        return (o, context, econtext) => callbackMethodInfo.Invoke(o, new object[] { context, econtext });
    }
}