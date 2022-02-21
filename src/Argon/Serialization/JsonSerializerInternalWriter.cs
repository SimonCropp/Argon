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

using System.Dynamic;

class JsonSerializerInternalWriter : JsonSerializerInternalBase
{
    Type? _rootType;
    int _rootLevel;
    readonly List<object> _serializeStack = new();

    public JsonSerializerInternalWriter(JsonSerializer serializer)
        : base(serializer)
    {
    }

    public void Serialize(JsonWriter jsonWriter, object? value, Type? type)
    {
        if (jsonWriter == null)
        {
            throw new ArgumentNullException(nameof(jsonWriter));
        }

        _rootType = type;
        _rootLevel = _serializeStack.Count + 1;

        var contract = GetContractSafe(value);

        try
        {
            if (ShouldWriteReference(value, null, contract, null, null))
            {
                WriteReference(jsonWriter, value!);
            }
            else
            {
                SerializeValue(jsonWriter, value, contract, null, null, null);
            }
        }
        catch (Exception ex)
        {
            if (IsErrorHandled(null, contract, null, null, jsonWriter.Path, ex))
            {
                HandleError(jsonWriter, 0);
            }
            else
            {
                // clear context in case serializer is being used inside a converter
                // if the converter wraps the error then not clearing the context will cause this error:
                // "Current error context error is different to requested error."
                ClearErrorContext();
                throw;
            }
        }
        finally
        {
            // clear root contract to ensure that if level was > 1 then it won't
            // accidentally be used for non root values
            _rootType = null;
        }
    }

    JsonSerializerProxy GetInternalSerializer()
    {
        if (InternalSerializer == null)
        {
            InternalSerializer = new JsonSerializerProxy(this);
        }

        return InternalSerializer;
    }

    JsonContract? GetContractSafe(object? value)
    {
        if (value == null)
        {
            return null;
        }

        return GetContract(value);
    }

    JsonContract GetContract(object value)
    {
        return Serializer._contractResolver.ResolveContract(value.GetType());
    }

    void SerializePrimitive(JsonWriter writer, object value, JsonPrimitiveContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
    {
        if (contract.TypeCode == PrimitiveTypeCode.Bytes)
        {
            // if type name handling is enabled then wrap the base64 byte string in an object with the type name
            var includeTypeDetails = ShouldWriteType(TypeNameHandling.Objects, contract, member, containerContract, containerProperty);
            if (includeTypeDetails)
            {
                writer.WriteStartObject();
                WriteTypeProperty(writer, contract.CreatedType);
                writer.WritePropertyName(JsonTypeReflector.ValuePropertyName, false);

                JsonWriter.WriteValue(writer, contract.TypeCode, value);

                writer.WriteEndObject();
                return;
            }
        }

        JsonWriter.WriteValue(writer, contract.TypeCode, value);
    }

    void SerializeValue(JsonWriter writer, object? value, JsonContract? valueContract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        MiscellaneousUtils.Assert(valueContract != null);

        var converter =
            member?.Converter ??
            containerProperty?.ItemConverter ??
            containerContract?.ItemConverter ??
            valueContract.Converter ??
            Serializer.GetMatchingConverter(valueContract.UnderlyingType) ??
            valueContract.InternalConverter;

        if (converter is {CanWrite: true})
        {
            SerializeConvertable(writer, converter, value, valueContract, containerContract, containerProperty);
            return;
        }

        switch (valueContract.ContractType)
        {
            case JsonContractType.Object:
                SerializeObject(writer, value, (JsonObjectContract)valueContract, member, containerContract, containerProperty);
                break;
            case JsonContractType.Array:
                var arrayContract = (JsonArrayContract)valueContract;
                if (arrayContract.IsMultidimensionalArray)
                {
                    SerializeMultidimensionalArray(writer, (Array) value, arrayContract, member, containerContract, containerProperty);
                }
                else
                {
                    SerializeList(writer, (IEnumerable) value, arrayContract, member, containerContract, containerProperty);
                }

                break;
            case JsonContractType.Primitive:
                SerializePrimitive(writer, value, (JsonPrimitiveContract)valueContract, member, containerContract, containerProperty);
                break;
            case JsonContractType.String:
                SerializeString(writer, value, (JsonStringContract)valueContract);
                break;
            case JsonContractType.Dictionary:
                var dictionaryContract = (JsonDictionaryContract)valueContract;
                SerializeDictionary(writer, value is IDictionary dictionary ? dictionary : dictionaryContract.CreateWrapper(value), dictionaryContract, member, containerContract, containerProperty);
                break;
            case JsonContractType.Dynamic:
                SerializeDynamic(writer, (IDynamicMetaObjectProvider)value, (JsonDynamicContract)valueContract, member, containerContract, containerProperty);
                break;
            case JsonContractType.Serializable:
                SerializeISerializable(writer, (ISerializable)value, (JsonISerializableContract)valueContract, member, containerContract, containerProperty);
                break;
            case JsonContractType.Linq:
                ((JToken)value).WriteTo(writer, Serializer.Converters.ToArray());
                break;
        }
    }

    static bool? ResolveIsReference(JsonContract contract, JsonProperty? property, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
    {
        bool? isReference = null;

        // value could be coming from a dictionary or array and not have a property
        if (property != null)
        {
            isReference = property.IsReference;
        }

        if (isReference == null && containerProperty != null)
        {
            isReference = containerProperty.ItemIsReference;
        }

        if (isReference == null && collectionContract != null)
        {
            isReference = collectionContract.ItemIsReference;
        }

        if (isReference == null)
        {
            isReference = contract.IsReference;
        }

        return isReference;
    }

    bool ShouldWriteReference(object? value, JsonProperty? property, JsonContract? valueContract, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
    {
        if (value == null)
        {
            return false;
        }

        MiscellaneousUtils.Assert(valueContract != null);

        if (valueContract.ContractType is JsonContractType.Primitive or JsonContractType.String)
        {
            return false;
        }

        var isReference = ResolveIsReference(valueContract, property, collectionContract, containerProperty);

        if (isReference == null)
        {
            if (valueContract.ContractType == JsonContractType.Array)
            {
                isReference = HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Arrays);
            }
            else
            {
                isReference = HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Objects);
            }
        }

        if (!isReference.GetValueOrDefault())
        {
            return false;
        }

        return Serializer.GetReferenceResolver().IsReferenced(this, value);
    }

    bool ShouldWriteProperty(object? memberValue, JsonObjectContract? containerContract, JsonProperty property)
    {
        if (memberValue == null && ResolvedNullValueHandling(containerContract, property) == NullValueHandling.Ignore)
        {
            return false;
        }

        return !HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Ignore) ||
               !MiscellaneousUtils.ValueEquals(memberValue, property.GetResolvedDefaultValue());
    }

    bool CheckForCircularReference(JsonWriter writer, object? value, JsonProperty? property, JsonContract? contract, JsonContainerContract? containerContract, JsonProperty? containerProperty)
    {
        if (value == null)
        {
            return true;
        }

        MiscellaneousUtils.Assert(contract != null);

        if (contract.ContractType is JsonContractType.Primitive or JsonContractType.String)
        {
            return true;
        }

        ReferenceLoopHandling? referenceLoopHandling = null;

        if (property != null)
        {
            referenceLoopHandling = property.ReferenceLoopHandling;
        }

        if (referenceLoopHandling == null && containerProperty != null)
        {
            referenceLoopHandling = containerProperty.ItemReferenceLoopHandling;
        }

        if (referenceLoopHandling == null && containerContract != null)
        {
            referenceLoopHandling = containerContract.ItemReferenceLoopHandling;
        }

        var exists = Serializer._equalityComparer != null
            ? _serializeStack.Contains(value, Serializer._equalityComparer)
            : _serializeStack.Contains(value);

        if (exists)
        {
            var message = "Self referencing loop detected";
            if (property != null)
            {
                message += $" for property '{property.PropertyName}'";
            }
            message += $" with type '{value.GetType()}'.";

            switch (referenceLoopHandling.GetValueOrDefault(Serializer._referenceLoopHandling))
            {
                case ReferenceLoopHandling.Error:
                    throw JsonSerializationException.Create(null, writer.ContainerPath, message, null);
                case ReferenceLoopHandling.Ignore:
                    if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
                    {
                        TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, $"{message}. Skipping serializing self referenced value."), null);
                    }

                    return false;
                case ReferenceLoopHandling.Serialize:
                    if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
                    {
                        TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, $"{message}. Serializing self referenced value."), null);
                    }

                    return true;
            }
        }

        return true;
    }

    void WriteReference(JsonWriter writer, object value)
    {
        var reference = GetReference(writer, value);

        if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
        {
            TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, $"Writing object reference to Id '{reference}' for {value.GetType()}."), null);
        }

        writer.WriteStartObject();
        writer.WritePropertyName(JsonTypeReflector.RefPropertyName, false);
        writer.WriteValue(reference);
        writer.WriteEndObject();
    }

    string GetReference(JsonWriter writer, object value)
    {
        try
        {
            var reference = Serializer.GetReferenceResolver().GetReference(this, value);

            return reference;
        }
        catch (Exception ex)
        {
            throw JsonSerializationException.Create(null, writer.ContainerPath, $"Error writing object reference for '{value.GetType()}'.", ex);
        }
    }

    internal static bool TryConvertToString(object value, Type type, [NotNullWhen(true)]out string? s)
    {
        if (JsonTypeReflector.CanTypeDescriptorConvertString(type, out var converter))
        {
            s = converter.ConvertToInvariantString(value);
            return true;
        }

        if (value is Guid or Uri or TimeSpan)
        {
            s = value.ToString();
            return true;
        }

        if (value is Type t)
        {
            s = t.AssemblyQualifiedName;
            return true;
        }

        s = null;
        return false;
    }

    void SerializeString(JsonWriter writer, object value, JsonStringContract contract)
    {
        OnSerializing(writer, contract, value);

        TryConvertToString(value, contract.UnderlyingType, out var s);
        writer.WriteValue(s);

        OnSerialized(writer, contract, value);
    }

    void OnSerializing(JsonWriter writer, JsonContract contract, object value)
    {
        if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
        {
            TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, $"Started serializing {contract.UnderlyingType}"), null);
        }

        contract.InvokeOnSerializing(value, Serializer._context);
    }

    void OnSerialized(JsonWriter writer, JsonContract contract, object value)
    {
        if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
        {
            TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, $"Finished serializing {contract.UnderlyingType}"), null);
        }

        contract.InvokeOnSerialized(value, Serializer._context);
    }

    void SerializeObject(JsonWriter writer, object value, JsonObjectContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
    {
        OnSerializing(writer, contract, value);

        _serializeStack.Add(value);

        WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);

        var initialDepth = writer.Top;

        for (var index = 0; index < contract.Properties.Count; index++)
        {
            var property = contract.Properties[index];
            try
            {
                if (!CalculatePropertyValues(writer, value, contract, member, property, out var memberContract, out var memberValue))
                {
                    continue;
                }

                property.WritePropertyName(writer);
                SerializeValue(writer, memberValue, memberContract, property, contract, member);
            }
            catch (Exception ex)
            {
                if (IsErrorHandled(value, contract, property.PropertyName, null, writer.ContainerPath, ex))
                {
                    HandleError(writer, initialDepth);
                }
                else
                {
                    throw;
                }
            }
        }

        var extensionData = contract.ExtensionDataGetter?.Invoke(value);
        if (extensionData != null)
        {
            foreach (var e in extensionData)
            {
                var keyContract = GetContract(e.Key);
                var valueContract = GetContractSafe(e.Value);

                var propertyName = GetPropertyName(writer, e.Key, keyContract, out _);

                propertyName = contract.ExtensionDataNameResolver != null
                    ? contract.ExtensionDataNameResolver(propertyName)
                    : propertyName;

                if (ShouldWriteReference(e.Value, null, valueContract, contract, member))
                {
                    writer.WritePropertyName(propertyName);
                    WriteReference(writer, e.Value!);
                }
                else
                {
                    if (!CheckForCircularReference(writer, e.Value, null, valueContract, contract, member))
                    {
                        continue;
                    }

                    writer.WritePropertyName(propertyName);

                    SerializeValue(writer, e.Value, valueContract, null, contract, member);
                }
            }
        }

        writer.WriteEndObject();

        _serializeStack.RemoveAt(_serializeStack.Count - 1);

        OnSerialized(writer, contract, value);
    }

    bool CalculatePropertyValues(JsonWriter writer, object value, JsonContainerContract contract, JsonProperty? member, JsonProperty property, [NotNullWhen(true)]out JsonContract? memberContract, out object? memberValue)
    {
        if (!property.Ignored && property.Readable && ShouldSerialize(writer, property, value) && IsSpecified(writer, property, value))
        {
            property.PropertyContract ??= Serializer._contractResolver.ResolveContract(property.PropertyType!);

            memberValue = property.ValueProvider!.GetValue(value);
            memberContract = property.PropertyContract.IsSealed ? property.PropertyContract : GetContractSafe(memberValue);

            if (ShouldWriteProperty(memberValue, contract as JsonObjectContract, property))
            {
                if (ShouldWriteReference(memberValue, property, memberContract, contract, member))
                {
                    property.WritePropertyName(writer);
                    WriteReference(writer, memberValue!);
                    return false;
                }

                if (!CheckForCircularReference(writer, memberValue, property, memberContract, contract, member))
                {
                    return false;
                }

                if (memberValue == null)
                {
                    var objectContract = contract as JsonObjectContract;
                    var resolvedRequired = property._required ?? objectContract?.ItemRequired ?? Required.Default;
                    if (resolvedRequired == Required.Always)
                    {
                        throw JsonSerializationException.Create(null, writer.ContainerPath, $"Cannot write a null value for property '{property.PropertyName}'. Property requires a value.", null);
                    }
                    if (resolvedRequired == Required.DisallowNull)
                    {
                        throw JsonSerializationException.Create(null, writer.ContainerPath, $"Cannot write a null value for property '{property.PropertyName}'. Property requires a non-null value.", null);
                    }
                }

#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
                return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
            }
        }

        memberContract = null;
        memberValue = null;
        return false;
    }

    void WriteObjectStart(JsonWriter writer, object value, JsonContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
    {
        writer.WriteStartObject();

        var isReference = ResolveIsReference(contract, member, collectionContract, containerProperty) ?? HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Objects);
        // don't make readonly fields that aren't creator parameters the referenced value because they can't be deserialized to
        if (isReference && (member == null || member.Writable || HasCreatorParameter(collectionContract, member)))
        {
            WriteReferenceIdProperty(writer, contract.UnderlyingType, value);
        }
        if (ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionContract, containerProperty))
        {
            WriteTypeProperty(writer, contract.UnderlyingType);
        }
    }

    static bool HasCreatorParameter(JsonContainerContract? contract, JsonProperty property)
    {
        if (contract is JsonObjectContract objectContract)
        {
            return objectContract.CreatorParameters.Contains(property.PropertyName!);
        }
        
        return false;
    }

    void WriteReferenceIdProperty(JsonWriter writer, Type type, object value)
    {
        var reference = GetReference(writer, value);

        if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
        {
            TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, $"Writing object reference Id '{reference}' for {type}."), null);
        }

        writer.WritePropertyName(JsonTypeReflector.IdPropertyName, false);
        writer.WriteValue(reference);
    }

    void WriteTypeProperty(JsonWriter writer, Type type)
    {
        var typeName = ReflectionUtils.GetTypeName(type, Serializer._typeNameAssemblyFormatHandling, Serializer._serializationBinder);

        if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
        {
            TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, $"Writing type name '{typeName}' for {type}."), null);
        }

        writer.WritePropertyName(JsonTypeReflector.TypePropertyName, false);
        writer.WriteValue(typeName);
    }

    static bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
    {
        return (value & flag) == flag;
    }

    static bool HasFlag(PreserveReferencesHandling value, PreserveReferencesHandling flag)
    {
        return (value & flag) == flag;
    }

    static bool HasFlag(TypeNameHandling value, TypeNameHandling flag)
    {
        return (value & flag) == flag;
    }

    void SerializeConvertable(JsonWriter writer, JsonConverter converter, object value, JsonContract contract, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
    {
        if (ShouldWriteReference(value, null, contract, collectionContract, containerProperty))
        {
            WriteReference(writer, value);
        }
        else
        {
            if (!CheckForCircularReference(writer, value, null, contract, collectionContract, containerProperty))
            {
                return;
            }

            _serializeStack.Add(value);

            if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, $"Started serializing {value.GetType()} with converter {converter.GetType()}."), null);
            }

            converter.WriteJson(writer, value, GetInternalSerializer());

            if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, $"Finished serializing {value.GetType()} with converter {converter.GetType()}."), null);
            }

            _serializeStack.RemoveAt(_serializeStack.Count - 1);
        }
    }

    void SerializeList(JsonWriter writer, IEnumerable values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
    {
        var underlyingList = values is IWrappedCollection wrappedCollection ? wrappedCollection.UnderlyingCollection : values;

        OnSerializing(writer, contract, underlyingList);

        _serializeStack.Add(underlyingList);

        var hasWrittenMetadataObject = WriteStartArray(writer, underlyingList, contract, member, collectionContract, containerProperty);

        writer.WriteStartArray();

        var initialDepth = writer.Top;

        var index = 0;
        // note that an error in the IEnumerable won't be caught
        foreach (var value in values)
        {
            try
            {
                var valueContract = contract.FinalItemContract ?? GetContractSafe(value);

                if (ShouldWriteReference(value, null, valueContract, contract, member))
                {
                    WriteReference(writer, value);
                }
                else
                {
                    if (CheckForCircularReference(writer, value, null, valueContract, contract, member))
                    {
                        SerializeValue(writer, value, valueContract, null, contract, member);
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsErrorHandled(underlyingList, contract, index, null, writer.ContainerPath, ex))
                {
                    HandleError(writer, initialDepth);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                index++;
            }
        }

        writer.WriteEndArray();

        if (hasWrittenMetadataObject)
        {
            writer.WriteEndObject();
        }

        _serializeStack.RemoveAt(_serializeStack.Count - 1);

        OnSerialized(writer, contract, underlyingList);
    }

    void SerializeMultidimensionalArray(JsonWriter writer, Array values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
    {
        OnSerializing(writer, contract, values);

        _serializeStack.Add(values);

        var hasWrittenMetadataObject = WriteStartArray(writer, values, contract, member, collectionContract, containerProperty);

        SerializeMultidimensionalArray(writer, values, contract, member, writer.Top, CollectionUtils.ArrayEmpty<int>());

        if (hasWrittenMetadataObject)
        {
            writer.WriteEndObject();
        }

        _serializeStack.RemoveAt(_serializeStack.Count - 1);

        OnSerialized(writer, contract, values);
    }

    void SerializeMultidimensionalArray(JsonWriter writer, Array values, JsonArrayContract contract, JsonProperty? member, int initialDepth, int[] indices)
    {
        var dimension = indices.Length;
        var newIndices = new int[dimension + 1];
        for (var i = 0; i < dimension; i++)
        {
            newIndices[i] = indices[i];
        }

        writer.WriteStartArray();

        for (var i = values.GetLowerBound(dimension); i <= values.GetUpperBound(dimension); i++)
        {
            newIndices[dimension] = i;
            var isTopLevel = newIndices.Length == values.Rank;

            if (isTopLevel)
            {
                var value = values.GetValue(newIndices);

                try
                {
                    var valueContract = contract.FinalItemContract ?? GetContractSafe(value);

                    if (ShouldWriteReference(value, null, valueContract, contract, member))
                    {
                        WriteReference(writer, value);
                    }
                    else
                    {
                        if (CheckForCircularReference(writer, value, null, valueContract, contract, member))
                        {
                            SerializeValue(writer, value, valueContract, null, contract, member);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(values, contract, i, null, writer.ContainerPath, ex))
                    {
                        HandleError(writer, initialDepth + 1);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                SerializeMultidimensionalArray(writer, values, contract, member, initialDepth + 1, newIndices);
            }
        }

        writer.WriteEndArray();
    }

    bool WriteStartArray(JsonWriter writer, object values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
    {
        var isReference = ResolveIsReference(contract, member, containerContract, containerProperty) ?? HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Arrays);
        // don't make readonly fields that aren't creator parameters the referenced value because they can't be deserialized to
        isReference = isReference && (member == null || member.Writable || HasCreatorParameter(containerContract, member));

        var includeTypeDetails = ShouldWriteType(TypeNameHandling.Arrays, contract, member, containerContract, containerProperty);
        var writeMetadataObject = isReference || includeTypeDetails;

        if (writeMetadataObject)
        {
            writer.WriteStartObject();

            if (isReference)
            {
                WriteReferenceIdProperty(writer, contract.UnderlyingType, values);
            }
            if (includeTypeDetails)
            {
                WriteTypeProperty(writer, values.GetType());
            }
            writer.WritePropertyName(JsonTypeReflector.ArrayValuesPropertyName, false);
        }

        contract.ItemContract ??= Serializer._contractResolver.ResolveContract(contract.CollectionItemType ?? typeof(object));

        return writeMetadataObject;
    }

    void SerializeISerializable(JsonWriter writer, ISerializable value, JsonISerializableContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
    {
        OnSerializing(writer, contract, value);
        _serializeStack.Add(value);

        WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);

        var serializationInfo = new SerializationInfo(contract.UnderlyingType, new FormatterConverter());
        value.GetObjectData(serializationInfo, Serializer._context);

        foreach (var serializationEntry in serializationInfo)
        {
            var valueContract = GetContractSafe(serializationEntry.Value);

            if (ShouldWriteReference(serializationEntry.Value, null, valueContract, contract, member))
            {
                writer.WritePropertyName(serializationEntry.Name);
                WriteReference(writer, serializationEntry.Value);
            }
            else if (CheckForCircularReference(writer, serializationEntry.Value, null, valueContract, contract, member))
            {
                writer.WritePropertyName(serializationEntry.Name);
                SerializeValue(writer, serializationEntry.Value, valueContract, null, contract, member);
            }
        }

        writer.WriteEndObject();

        _serializeStack.RemoveAt(_serializeStack.Count - 1);
        OnSerialized(writer, contract, value);
    }

    void SerializeDynamic(JsonWriter writer, IDynamicMetaObjectProvider value, JsonDynamicContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
    {
        OnSerializing(writer, contract, value);
        _serializeStack.Add(value);

        WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);

        var initialDepth = writer.Top;

        for (var index = 0; index < contract.Properties.Count; index++)
        {
            var property = contract.Properties[index];

            // only write non-dynamic properties that have an explicit attribute
            if (property.HasMemberAttribute)
            {
                try
                {
                    if (!CalculatePropertyValues(writer, value, contract, member, property, out var memberContract, out var memberValue))
                    {
                        continue;
                    }

                    property.WritePropertyName(writer);
                    SerializeValue(writer, memberValue, memberContract, property, contract, member);
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(value, contract, property.PropertyName, null, writer.ContainerPath, ex))
                    {
                        HandleError(writer, initialDepth);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        foreach (var memberName in value.GetDynamicMemberNames())
        {
            if (contract.TryGetMember(value, memberName, out var memberValue))
            {
                try
                {
                    var valueContract = GetContractSafe(memberValue);

                    if (!ShouldWriteDynamicProperty(memberValue))
                    {
                        continue;
                    }

                    if (CheckForCircularReference(writer, memberValue, null, valueContract, contract, member))
                    {
                        var resolvedPropertyName = contract.PropertyNameResolver != null
                            ? contract.PropertyNameResolver(memberName)
                            : memberName;

                        writer.WritePropertyName(resolvedPropertyName);
                        SerializeValue(writer, memberValue, valueContract, null, contract, member);
                    }
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(value, contract, memberName, null, writer.ContainerPath, ex))
                    {
                        HandleError(writer, initialDepth);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        writer.WriteEndObject();

        _serializeStack.RemoveAt(_serializeStack.Count - 1);
        OnSerialized(writer, contract, value);
    }

    bool ShouldWriteDynamicProperty(object? memberValue)
    {
        if (Serializer._nullValueHandling == NullValueHandling.Ignore && memberValue == null)
        {
            return false;
        }

        return !HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Ignore) || 
               (memberValue != null && !MiscellaneousUtils.ValueEquals(memberValue, ReflectionUtils.GetDefaultValue(memberValue.GetType())));
    }

    bool ShouldWriteType(TypeNameHandling typeNameHandlingFlag, JsonContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
    {
        var resolvedTypeNameHandling =
            member?.TypeNameHandling
            ?? containerProperty?.ItemTypeNameHandling
            ?? containerContract?.ItemTypeNameHandling
            ?? Serializer._typeNameHandling;

        if (HasFlag(resolvedTypeNameHandling, typeNameHandlingFlag))
        {
            return true;
        }

        // instance type and the property's type's contract default type are different (no need to put the type in JSON because the type will be created by default)
        if (HasFlag(resolvedTypeNameHandling, TypeNameHandling.Auto))
        {
            if (member != null)
            {
                if (contract.NonNullableUnderlyingType != member.PropertyContract!.CreatedType)
                {
                    return true;
                }
            }
            else if (containerContract != null)
            {
                if (containerContract.ItemContract == null || contract.NonNullableUnderlyingType != containerContract.ItemContract.CreatedType)
                {
                    return true;
                }
            }
            else if (_rootType != null && _serializeStack.Count == _rootLevel)
            {
                var rootContract = Serializer._contractResolver.ResolveContract(_rootType);

                if (contract.NonNullableUnderlyingType != rootContract.CreatedType)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void SerializeDictionary(JsonWriter writer, IDictionary values, JsonDictionaryContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
    {
#pragma warning disable CS8600, CS8602, CS8604
        var underlyingDictionary = values is IWrappedDictionary wrappedDictionary ? wrappedDictionary.UnderlyingDictionary : values;

        OnSerializing(writer, contract, underlyingDictionary);
        _serializeStack.Add(underlyingDictionary);

        WriteObjectStart(writer, underlyingDictionary, contract, member, collectionContract, containerProperty);

        contract.ItemContract ??= Serializer._contractResolver.ResolveContract(contract.DictionaryValueType ?? typeof(object));

        contract.KeyContract ??= Serializer._contractResolver.ResolveContract(contract.DictionaryKeyType ?? typeof(object));

        var initialDepth = writer.Top;

        // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
        var e = values.GetEnumerator();
        try
        {
            while (e.MoveNext())
            {
                var entry = e.Entry;

                var propertyName = GetPropertyName(writer, entry.Key, contract.KeyContract, out var escape);

                propertyName = contract.DictionaryKeyResolver != null
                    ? contract.DictionaryKeyResolver(propertyName)
                    : propertyName;

                try
                {
                    var value = entry.Value;
                    var valueContract = contract.FinalItemContract ?? GetContractSafe(value);

                    if (ShouldWriteReference(value, null, valueContract, contract, member))
                    {
                        writer.WritePropertyName(propertyName, escape);
                        WriteReference(writer, value);
                    }
                    else
                    {
                        if (!CheckForCircularReference(writer, value, null, valueContract, contract, member))
                        {
                            continue;
                        }

                        writer.WritePropertyName(propertyName, escape);

                        SerializeValue(writer, value, valueContract, null, contract, member);
                    }
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(underlyingDictionary, contract, propertyName, null, writer.ContainerPath, ex))
                    {
                        HandleError(writer, initialDepth);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        finally
        {
            (e as IDisposable)?.Dispose();
        }

        writer.WriteEndObject();

        _serializeStack.RemoveAt(_serializeStack.Count - 1);

        OnSerialized(writer, contract, underlyingDictionary);
#pragma warning restore CS8600, CS8602, CS8604
    }

    static string GetPropertyName(JsonWriter writer, object name, JsonContract contract, out bool escape)
    {
        if (contract.ContractType == JsonContractType.Primitive)
        {
            var primitiveContract = (JsonPrimitiveContract)contract;
            switch (primitiveContract.TypeCode)
            {
                case PrimitiveTypeCode.DateTime:
                case PrimitiveTypeCode.DateTimeNullable:
                {
                    var dt = DateTimeUtils.EnsureDateTime((DateTime)name, writer.DateTimeZoneHandling);

                    escape = false;
                    var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                    DateTimeUtils.WriteDateTimeString(stringWriter, dt, writer.DateFormatHandling, writer.DateFormatString, writer.Culture);
                    return stringWriter.ToString();
                }
                case PrimitiveTypeCode.DateTimeOffset:
                case PrimitiveTypeCode.DateTimeOffsetNullable:
                {
                    escape = false;
                    var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                    DateTimeUtils.WriteDateTimeOffsetString(stringWriter, (DateTimeOffset)name, writer.DateFormatHandling, writer.DateFormatString, writer.Culture);
                    return stringWriter.ToString();
                }
                case PrimitiveTypeCode.Double:
                case PrimitiveTypeCode.DoubleNullable:
                {
                    var d = (double)name;

                    escape = false;
                    return d.ToString("R", CultureInfo.InvariantCulture);
                }
                case PrimitiveTypeCode.Single:
                case PrimitiveTypeCode.SingleNullable:
                {
                    var f = (float)name;

                    escape = false;
                    return f.ToString("R", CultureInfo.InvariantCulture);
                }
                default:
                {
                    escape = true;

                    if (primitiveContract.IsEnum && EnumUtils.TryToString(primitiveContract.NonNullableUnderlyingType, name, null, out var enumName))
                    {
                        return enumName;
                    }

                    return Convert.ToString(name, CultureInfo.InvariantCulture);
                }
            }
        }

        if (TryConvertToString(name, name.GetType(), out var propertyName))
        {
            escape = true;
            return propertyName;
        }

        escape = true;
        return name.ToString();
    }

    void HandleError(JsonWriter writer, int initialDepth)
    {
        ClearErrorContext();

        if (writer.WriteState == WriteState.Property)
        {
            writer.WriteNull();
        }

        while (writer.Top > initialDepth)
        {
            writer.WriteEnd();
        }
    }

    bool ShouldSerialize(JsonWriter writer, JsonProperty property, object target)
    {
        if (property.ShouldSerialize == null)
        {
            return true;
        }

        var shouldSerialize = property.ShouldSerialize(target);

        if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
        {
            TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, $"ShouldSerialize result for property '{property.PropertyName}' on {property.DeclaringType}: {shouldSerialize}"), null);
        }

        return shouldSerialize;
    }

    bool IsSpecified(JsonWriter writer, JsonProperty property, object target)
    {
        if (property.GetIsSpecified == null)
        {
            return true;
        }

        var isSpecified = property.GetIsSpecified(target);

        if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
        {
            TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, $"IsSpecified result for property '{property.PropertyName}' on {property.DeclaringType}: {isSpecified}"), null);
        }

        return isSpecified;
    }
}