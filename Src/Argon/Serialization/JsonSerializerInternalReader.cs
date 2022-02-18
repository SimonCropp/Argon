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
using Argon;

class JsonSerializerInternalReader : JsonSerializerInternalBase
{
    internal enum PropertyPresence
    {
        None = 0,
        Null = 1,
        Value = 2
    }

    public JsonSerializerInternalReader(JsonSerializer serializer)
        : base(serializer)
    {
    }

    public void Populate(JsonReader reader, object target)
    {
        ValidationUtils.ArgumentNotNull(target, nameof(target));

        var objectType = target.GetType();

        var contract = Serializer._contractResolver.ResolveContract(objectType);

        if (!reader.MoveToContent())
        {
            throw JsonSerializationException.Create(reader, "No JSON content found.");
        }

        if (reader.TokenType == JsonToken.StartArray)
        {
            if (contract.ContractType == JsonContractType.Array)
            {
                var arrayContract = (JsonArrayContract)contract;

                PopulateList(arrayContract.ShouldCreateWrapper ? arrayContract.CreateWrapper(target) : (IList)target, reader, arrayContract, null, null);
            }
            else
            {
                throw JsonSerializationException.Create(reader,string.Format( "Cannot populate JSON array onto type '{0}'.", objectType));
            }
        }
        else if (reader.TokenType == JsonToken.StartObject)
        {
            reader.ReadAndAssert();

            string? id = null;
            if (Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore
                && reader.TokenType == JsonToken.PropertyName
                && string.Equals(reader.Value!.ToString(), JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
            {
                reader.ReadAndAssert();
                id = reader.Value?.ToString();
                reader.ReadAndAssert();
            }

            if (contract.ContractType == JsonContractType.Dictionary)
            {
                var dictionaryContract = (JsonDictionaryContract)contract;
                PopulateDictionary(dictionaryContract.ShouldCreateWrapper ? dictionaryContract.CreateWrapper(target) : (IDictionary)target, reader, dictionaryContract, null, id);
            }
            else if (contract.ContractType == JsonContractType.Object)
            {
                PopulateObject(target, reader, (JsonObjectContract)contract, null, id);
            }
            else
            {
                throw JsonSerializationException.Create(reader,string.Format( "Cannot populate JSON object onto type '{0}'.", objectType));
            }
        }
        else
        {
            throw JsonSerializationException.Create(reader, string.Format("Unexpected initial token '{0}' when populating object. Expected JSON object or array.", reader.TokenType));
        }
    }

    JsonContract? GetContractSafe(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        return GetContract(type);
    }

    JsonContract GetContract(Type type)
    {
        return Serializer._contractResolver.ResolveContract(type);
    }

    public object? Deserialize(JsonReader reader, Type? objectType, bool checkAdditionalContent)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        var contract = GetContractSafe(objectType);

        try
        {
            var converter = GetConverter(contract, null, null, null);

            if (reader.TokenType == JsonToken.None && !reader.ReadForType(contract, converter != null))
            {
                if (contract is {IsNullable: false})
                {
                    throw JsonSerializationException.Create(reader, string.Format("No JSON content found and type '{0}' is not nullable.", contract.UnderlyingType));
                }

                return null;
            }

            object? deserializedValue;

            if (converter is {CanRead: true})
            {
                deserializedValue = DeserializeConvertable(converter, reader, objectType!, null);
            }
            else
            {
                deserializedValue = CreateValueInternal(reader, objectType, contract, null, null, null, null);
            }

            if (checkAdditionalContent)
            {
                while (reader.Read())
                {
                    if (reader.TokenType != JsonToken.Comment)
                    {
                        throw JsonSerializationException.Create(reader, "Additional text found in JSON string after finishing deserializing object.");
                    }
                }
            }

            return deserializedValue;
        }
        catch (Exception ex)
        {
            if (IsErrorHandled(null, contract, null, reader as IJsonLineInfo, reader.Path, ex))
            {
                HandleError(reader, false, 0);
                return null;
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
    }

    JsonSerializerProxy GetInternalSerializer()
    {
        if (InternalSerializer == null)
        {
            InternalSerializer = new JsonSerializerProxy(this);
        }

        return InternalSerializer;
    }

    JToken? CreateJToken(JsonReader reader, JsonContract? contract)
    {
        ValidationUtils.ArgumentNotNull(reader, nameof(reader));

        if (contract != null)
        {
            if (contract.UnderlyingType == typeof(JRaw))
            {
                return JRaw.Create(reader);
            }
            if (reader.TokenType == JsonToken.Null
                && !(contract.UnderlyingType == typeof(JValue) || contract.UnderlyingType == typeof(JToken)))
            {
                return null;
            }
        }

        JToken? token;
        using (var writer = new JTokenWriter())
        {
            writer.WriteToken(reader);
            token = writer.Token;
        }

        if (contract != null && token != null)
        {
            if (!contract.UnderlyingType.IsAssignableFrom(token.GetType()))
            {
                throw JsonSerializationException.Create(reader, $"Deserialized JSON type '{token.GetType().FullName}' is not compatible with expected type '{contract.UnderlyingType.FullName}'.");
            }
        }

        return token;
    }

    JToken CreateJObject(JsonReader reader)
    {
        ValidationUtils.ArgumentNotNull(reader, nameof(reader));

        // this is needed because we've already read inside the object, looking for metadata properties
        using (var writer = new JTokenWriter())
        {
            writer.WriteStartObject();

            do
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = (string)reader.Value!;
                    if (!reader.ReadAndMoveToContent())
                    {
                        break;
                    }

                    if (CheckPropertyName(reader, propertyName))
                    {
                        continue;
                    }

                    writer.WritePropertyName(propertyName);
                    writer.WriteToken(reader, true, true, false);
                }
                else if (reader.TokenType == JsonToken.Comment)
                {
                    // eat
                }
                else
                {
                    writer.WriteEndObject();
                    return writer.Token!;
                }
            } while (reader.Read());

            throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
        }
    }

    object? CreateValueInternal(JsonReader reader, Type? objectType, JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, object? existingValue)
    {
        if (contract is {ContractType: JsonContractType.Linq})
        {
            return CreateJToken(reader, contract);
        }

        do
        {
            switch (reader.TokenType)
            {
                // populate a typed object or generic dictionary/array
                // depending upon whether an objectType was supplied
                case JsonToken.StartObject:
                    return CreateObject(reader, objectType, contract, member, containerContract, containerMember, existingValue);
                case JsonToken.StartArray:
                    return CreateList(reader, objectType, contract, member, existingValue, null);
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Boolean:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
                case JsonToken.String:
                    var s = (string)reader.Value!;

                    // string that needs to be returned as a byte array should be base 64 decoded
                    if (objectType == typeof(byte[]))
                    {
                        return Convert.FromBase64String(s);
                    }

                    // convert empty string to null automatically for nullable types
                    if (CoerceEmptyStringToNull(objectType, contract, s))
                    {
                        return null;
                    }

                    return EnsureType(reader, s, CultureInfo.InvariantCulture, contract, objectType);
                case JsonToken.StartConstructor:
                    var constructorName = reader.Value!.ToString();

                    return EnsureType(reader, constructorName, CultureInfo.InvariantCulture, contract, objectType);
                case JsonToken.Null:
                case JsonToken.Undefined:
                    if (objectType == typeof(DBNull))
                    {
                        return DBNull.Value;
                    }

                    return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
                case JsonToken.Raw:
                    return new JRaw((string?)reader.Value);
                case JsonToken.Comment:
                    // ignore
                    break;
                default:
                    throw JsonSerializationException.Create(reader, "Unexpected token while deserializing object: " + reader.TokenType);
            }
        } while (reader.Read());

        throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
    }

    static bool CoerceEmptyStringToNull(Type? objectType, JsonContract? contract, string s)
    {
        return StringUtils.IsNullOrEmpty(s) && objectType != null && objectType != typeof(string) && objectType != typeof(object) && contract is {IsNullable: true};
    }

    internal string GetExpectedDescription(JsonContract contract)
    {
        switch (contract.ContractType)
        {
            case JsonContractType.Object:
            case JsonContractType.Dictionary:
            case JsonContractType.Serializable:
            case JsonContractType.Dynamic:
                return @"JSON object (e.g. {""name"":""value""})";
            case JsonContractType.Array:
                return @"JSON array (e.g. [1,2,3])";
            case JsonContractType.Primitive:
                return @"JSON primitive value (e.g. string, number, boolean, null)";
            case JsonContractType.String:
                return @"JSON string value";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    JsonConverter? GetConverter(JsonContract? contract, JsonConverter? memberConverter, JsonContainerContract? containerContract, JsonProperty? containerProperty)
    {
        JsonConverter? converter = null;
        if (memberConverter != null)
        {
            // member attribute converter
            converter = memberConverter;
        }
        else if (containerProperty?.ItemConverter != null)
        {
            converter = containerProperty.ItemConverter;
        }
        else if (containerContract?.ItemConverter != null)
        {
            converter = containerContract.ItemConverter;
        }
        else if (contract != null)
        {
            if (contract.Converter != null)
            {
                // class attribute converter
                converter = contract.Converter;
            }
            else if (Serializer.GetMatchingConverter(contract.UnderlyingType) is JsonConverter matchingConverter)
            {
                // passed in converters
                converter = matchingConverter;
            }
            else if (contract.InternalConverter != null)
            {
                // internally specified converter
                converter = contract.InternalConverter;
            }
        }
        return converter;
    }

    object? CreateObject(JsonReader reader, Type? objectType, JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, object? existingValue)
    {
        string? id;
        var resolvedObjectType = objectType;

        if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.Ignore)
        {
            // don't look for metadata properties
            reader.ReadAndAssert();
            id = null;
        }
        else if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
        {
            if (!(reader is JTokenReader tokenReader))
            {
                var t = JToken.ReadFrom(reader);
                tokenReader = (JTokenReader)t.CreateReader();
                tokenReader.Culture = reader.Culture;
                tokenReader.DateFormatString = reader.DateFormatString;
                tokenReader.DateParseHandling = reader.DateParseHandling;
                tokenReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                tokenReader.FloatParseHandling = reader.FloatParseHandling;
                tokenReader.SupportMultipleContent = reader.SupportMultipleContent;

                // start
                tokenReader.ReadAndAssert();

                reader = tokenReader;
            }

            if (ReadMetadataPropertiesToken(tokenReader, ref resolvedObjectType, ref contract, member, containerContract, containerMember, existingValue, out var newValue, out id))
            {
                return newValue;
            }
        }
        else
        {
            reader.ReadAndAssert();
            if (ReadMetadataProperties(reader, ref resolvedObjectType, ref contract, member, containerContract, containerMember, existingValue, out var newValue, out id))
            {
                return newValue;
            }
        }

        if (HasNoDefinedType(contract))
        {
            return CreateJObject(reader);
        }

        MiscellaneousUtils.Assert(resolvedObjectType != null);
        MiscellaneousUtils.Assert(contract != null);

        switch (contract.ContractType)
        {
            case JsonContractType.Object:
            {
                var createdFromNonDefaultCreator = false;
                var objectContract = (JsonObjectContract)contract;
                object targetObject;
                // check that if type name handling is being used that the existing value is compatible with the specified type
                if (existingValue != null && (resolvedObjectType == objectType || resolvedObjectType.IsAssignableFrom(existingValue.GetType())))
                {
                    targetObject = existingValue;
                }
                else
                {
                    targetObject = CreateNewObject(reader, objectContract, member, containerMember, id, out createdFromNonDefaultCreator);
                }

                // don't populate if read from non-default creator because the object has already been read
                if (createdFromNonDefaultCreator)
                {
                    return targetObject;
                }

                return PopulateObject(targetObject, reader, objectContract, member, id);
            }
            case JsonContractType.Primitive:
            {
                var primitiveContract = (JsonPrimitiveContract)contract;
                // if the content is inside $value then read past it
                if (Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore
                    && reader.TokenType == JsonToken.PropertyName
                    && string.Equals(reader.Value!.ToString(), JsonTypeReflector.ValuePropertyName, StringComparison.Ordinal))
                {
                    reader.ReadAndAssert();

                    // the token should not be an object because the $type value could have been included in the object
                    // without needing the $value property
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing primitive value: " + reader.TokenType);
                    }

                    var value = CreateValueInternal(reader, resolvedObjectType, primitiveContract, member, null, null, existingValue);

                    reader.ReadAndAssert();
                    return value;
                }
                break;
            }
            case JsonContractType.Dictionary:
            {
                var dictionaryContract = (JsonDictionaryContract)contract;
                object targetDictionary;

                if (existingValue == null)
                {
                    var dictionary = CreateNewDictionary(reader, dictionaryContract, out var createdFromNonDefaultCreator);

                    if (createdFromNonDefaultCreator)
                    {
                        if (id != null)
                        {
                            throw JsonSerializationException.Create(reader, string.Format("Cannot preserve reference to readonly dictionary, or dictionary created from a non-default constructor: {0}.", contract.UnderlyingType));
                        }

                        if (contract.OnSerializingCallbacks.Count > 0)
                        {
                            throw JsonSerializationException.Create(reader, string.Format("Cannot call OnSerializing on readonly dictionary, or dictionary created from a non-default constructor: {0}.", contract.UnderlyingType));
                        }

                        if (contract.OnErrorCallbacks.Count > 0)
                        {
                            throw JsonSerializationException.Create(reader, string.Format("Cannot call OnError on readonly list, or dictionary created from a non-default constructor: {0}.", contract.UnderlyingType));
                        }

                        if (!dictionaryContract.HasParameterizedCreatorInternal)
                        {
                            throw JsonSerializationException.Create(reader, string.Format("Cannot deserialize readonly or fixed size dictionary: {0}.", contract.UnderlyingType));
                        }
                    }

                    PopulateDictionary(dictionary, reader, dictionaryContract, member, id);

                    if (createdFromNonDefaultCreator)
                    {
                        var creator = (dictionaryContract.OverrideCreator ?? dictionaryContract.ParameterizedCreator)!;

                        return creator(dictionary);
                    }
                    else if (dictionary is IWrappedDictionary wrappedDictionary)
                    {
                        return wrappedDictionary.UnderlyingDictionary;
                    }

                    targetDictionary = dictionary;
                }
                else
                {
                    targetDictionary = PopulateDictionary(dictionaryContract.ShouldCreateWrapper || !(existingValue is IDictionary) ? dictionaryContract.CreateWrapper(existingValue) : (IDictionary)existingValue, reader, dictionaryContract, member, id);
                }

                return targetDictionary;
            }
            case JsonContractType.Dynamic:
                var dynamicContract = (JsonDynamicContract)contract;
                return CreateDynamic(reader, dynamicContract, member, id);
            case JsonContractType.Serializable:
                var serializableContract = (JsonISerializableContract)contract;
                return CreateISerializable(reader, serializableContract, member, id);
        }

        var message = @"Cannot deserialize the current JSON object (e.g. {{""name"":""value""}}) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine +
                      @"To fix this error either change the JSON to a {1} or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object." + Environment.NewLine;
        message = string.Format(message, resolvedObjectType, GetExpectedDescription(contract));

        throw JsonSerializationException.Create(reader, message);
    }

    bool ReadMetadataPropertiesToken(JTokenReader reader, ref Type? objectType, ref JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, object? existingValue, out object? newValue, out string? id)
    {
        id = null;
        newValue = null;

        if (reader.TokenType == JsonToken.StartObject)
        {
            var current = (JObject)reader.CurrentToken!;

            var refProperty = current.Property(JsonTypeReflector.RefPropertyName, StringComparison.Ordinal);
            if (refProperty != null)
            {
                var refToken = refProperty.Value;
                if (refToken.Type != JTokenType.String && refToken.Type != JTokenType.Null)
                {
                    throw JsonSerializationException.Create(refToken, refToken.Path, string.Format("JSON reference {0} property must have a string or null value.", JsonTypeReflector.RefPropertyName), null);
                }

                var reference = (string?)refProperty;

                if (reference != null)
                {
                    var additionalContent = refProperty.Next ?? refProperty.Previous;
                    if (additionalContent != null)
                    {
                        throw JsonSerializationException.Create(additionalContent, additionalContent.Path, string.Format("Additional content found in JSON reference object. A JSON reference object should only have a {0} property.", JsonTypeReflector.RefPropertyName), null);
                    }

                    newValue = Serializer.GetReferenceResolver().ResolveReference(this, reference);

                    if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
                    {
                        TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader, reader.Path,string.Format( "Resolved object reference '{0}' to {1}.", reference, newValue.GetType())), null);
                    }

                    reader.Skip();
                    return true;
                }
            }
            var typeToken = current[JsonTypeReflector.TypePropertyName];
            if (typeToken != null)
            {
                var qualifiedTypeName = (string?)typeToken;
                var typeTokenReader = typeToken.CreateReader();
                typeTokenReader.ReadAndAssert();
                ResolveTypeName(typeTokenReader, ref objectType, ref contract, member, containerContract, containerMember, qualifiedTypeName!);

                var valueToken = current[JsonTypeReflector.ValuePropertyName];
                if (valueToken != null)
                {
                    while (true)
                    {
                        reader.ReadAndAssert();
                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            if ((string)reader.Value! == JsonTypeReflector.ValuePropertyName)
                            {
                                return false;
                            }
                        }

                        reader.ReadAndAssert();
                        reader.Skip();
                    }
                }
            }
            var idToken = current[JsonTypeReflector.IdPropertyName];
            if (idToken != null)
            {
                id = (string?)idToken;
            }
            var valuesToken = current[JsonTypeReflector.ArrayValuesPropertyName];
            if (valuesToken != null)
            {
                var listReader = valuesToken.CreateReader();
                listReader.ReadAndAssert();
                newValue = CreateList(listReader, objectType, contract, member, existingValue, id);

                reader.Skip();
                return true;
            }
        }

        reader.ReadAndAssert();
        return false;
    }

    bool ReadMetadataProperties(JsonReader reader, ref Type? objectType, ref JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, object? existingValue, out object? newValue, out string? id)
    {
        id = null;
        newValue = null;

        if (reader.TokenType == JsonToken.PropertyName)
        {
            var propertyName = reader.Value!.ToString();

            if (propertyName.Length > 0 && propertyName[0] == '$')
            {
                // read metadata properties
                // $type, $id, $ref, etc
                bool metadataProperty;

                do
                {
                    propertyName = reader.Value!.ToString();

                    if (string.Equals(propertyName, JsonTypeReflector.RefPropertyName, StringComparison.Ordinal))
                    {
                        reader.ReadAndAssert();
                        if (reader.TokenType != JsonToken.String && reader.TokenType != JsonToken.Null)
                        {
                            throw JsonSerializationException.Create(reader,string.Format( "JSON reference {0} property must have a string or null value.", JsonTypeReflector.RefPropertyName));
                        }

                        var reference = reader.Value?.ToString();

                        reader.ReadAndAssert();

                        if (reference != null)
                        {
                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                throw JsonSerializationException.Create(reader, string.Format("Additional content found in JSON reference object. A JSON reference object should only have a {0} property.", JsonTypeReflector.RefPropertyName));
                            }

                            newValue = Serializer.GetReferenceResolver().ResolveReference(this, reference);

                            if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
                            {
                                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("Resolved object reference '{0}' to {1}.", reference, newValue!.GetType())), null);
                            }

                            return true;
                        }
                        else
                        {
                            metadataProperty = true;
                        }
                    }
                    else if (string.Equals(propertyName, JsonTypeReflector.TypePropertyName, StringComparison.Ordinal))
                    {
                        reader.ReadAndAssert();
                        var qualifiedTypeName = reader.Value!.ToString();

                        ResolveTypeName(reader, ref objectType, ref contract, member, containerContract, containerMember, qualifiedTypeName);

                        reader.ReadAndAssert();

                        metadataProperty = true;
                    }
                    else if (string.Equals(propertyName, JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
                    {
                        reader.ReadAndAssert();

                        id = reader.Value?.ToString();

                        reader.ReadAndAssert();
                        metadataProperty = true;
                    }
                    else if (string.Equals(propertyName, JsonTypeReflector.ArrayValuesPropertyName, StringComparison.Ordinal))
                    {
                        reader.ReadAndAssert();
                        var list = CreateList(reader, objectType, contract, member, existingValue, id);
                        reader.ReadAndAssert();
                        newValue = list;
                        return true;
                    }
                    else
                    {
                        metadataProperty = false;
                    }
                } while (metadataProperty && reader.TokenType == JsonToken.PropertyName);
            }
        }
        return false;
    }

    void ResolveTypeName(JsonReader reader, ref Type? objectType, ref JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, string qualifiedTypeName)
    {
        var resolvedTypeNameHandling =
            member?.TypeNameHandling
            ?? containerContract?.ItemTypeNameHandling
            ?? containerMember?.ItemTypeNameHandling
            ?? Serializer._typeNameHandling;

        if (resolvedTypeNameHandling != TypeNameHandling.None)
        {
            var typeNameKey = ReflectionUtils.SplitFullyQualifiedTypeName(qualifiedTypeName);

            Type specifiedType;
            try
            {
                specifiedType = Serializer._serializationBinder.BindToType(typeNameKey.Value1, typeNameKey.Value2);
            }
            catch (Exception ex)
            {
                throw JsonSerializationException.Create(reader, string.Format("Error resolving type specified in JSON '{0}'.", qualifiedTypeName), ex);
            }

            if (specifiedType == null)
            {
                throw JsonSerializationException.Create(reader, string.Format("Type specified in JSON '{0}' was not resolved.", qualifiedTypeName));
            }

            if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
            {
                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("Resolved type '{0}' to {1}.", qualifiedTypeName, specifiedType)), null);
            }

            if (objectType != null
                && objectType != typeof(IDynamicMetaObjectProvider)
                && !objectType.IsAssignableFrom(specifiedType))
            {
                throw JsonSerializationException.Create(reader,string.Format( "Type specified in JSON '{0}' is not compatible with '{1}'.", specifiedType.AssemblyQualifiedName, objectType.AssemblyQualifiedName));
            }

            objectType = specifiedType;
            contract = GetContract(specifiedType);
        }
    }

    JsonArrayContract EnsureArrayContract(JsonReader reader, Type objectType, JsonContract contract)
    {
        if (contract == null)
        {
            throw JsonSerializationException.Create(reader,string.Format( "Could not resolve type '{0}' to a JsonContract.", objectType));
        }

        if (!(contract is JsonArrayContract arrayContract))
        {
            var message = @"Cannot deserialize the current JSON array (e.g. [1,2,3]) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine +
                          @"To fix this error either change the JSON to a {1} or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array." + Environment.NewLine;
            message = string.Format(message, objectType, GetExpectedDescription(contract));

            throw JsonSerializationException.Create(reader, message);
        }

        return arrayContract;
    }

    object? CreateList(JsonReader reader, Type? objectType, JsonContract? contract, JsonProperty? member, object? existingValue, string? id)
    {
        object? value;

        if (HasNoDefinedType(contract))
        {
            return CreateJToken(reader, contract);
        }

        MiscellaneousUtils.Assert(objectType != null);
        MiscellaneousUtils.Assert(contract != null);

        var arrayContract = EnsureArrayContract(reader, objectType, contract);

        if (existingValue == null)
        {
            var list = CreateNewList(reader, arrayContract, out var createdFromNonDefaultCreator);

            if (createdFromNonDefaultCreator)
            {
                if (id != null)
                {
                    throw JsonSerializationException.Create(reader, string.Format("Cannot preserve reference to array or readonly list, or list created from a non-default constructor: {0}.", contract.UnderlyingType));
                }

                if (contract.OnSerializingCallbacks.Count > 0)
                {
                    throw JsonSerializationException.Create(reader, string.Format("Cannot call OnSerializing on an array or readonly list, or list created from a non-default constructor: {0}.", contract.UnderlyingType));
                }

                if (contract.OnErrorCallbacks.Count > 0)
                {
                    throw JsonSerializationException.Create(reader, string.Format("Cannot call OnError on an array or readonly list, or list created from a non-default constructor: {0}.", contract.UnderlyingType));
                }

                if (!arrayContract.HasParameterizedCreatorInternal && !arrayContract.IsArray)
                {
                    throw JsonSerializationException.Create(reader, string.Format("Cannot deserialize readonly or fixed size list: {0}.", contract.UnderlyingType));
                }
            }

            if (!arrayContract.IsMultidimensionalArray)
            {
                PopulateList(list, reader, arrayContract, member, id);
            }
            else
            {
                PopulateMultidimensionalArray(list, reader, arrayContract, member, id);
            }

            if (createdFromNonDefaultCreator)
            {
                if (arrayContract.IsMultidimensionalArray)
                {
                    list = CollectionUtils.ToMultidimensionalArray(list, arrayContract.CollectionItemType!, contract.CreatedType.GetArrayRank());
                }
                else if (arrayContract.IsArray)
                {
                    var a = Array.CreateInstance(arrayContract.CollectionItemType, list.Count);
                    list.CopyTo(a, 0);
                    list = a;
                }
                else
                {
                    var creator = (arrayContract.OverrideCreator ?? arrayContract.ParameterizedCreator)!;

                    return creator(list);
                }
            }
            else if (list is IWrappedCollection wrappedCollection)
            {
                return wrappedCollection.UnderlyingCollection;
            }

            value = list;
        }
        else
        {
            if (!arrayContract.CanDeserialize)
            {
                throw JsonSerializationException.Create(reader, string.Format("Cannot populate list type {0}.", contract.CreatedType));
            }

            value = PopulateList(arrayContract.ShouldCreateWrapper || !(existingValue is IList list) ? arrayContract.CreateWrapper(existingValue) : list, reader, arrayContract, member, id);
        }

        return value;
    }

    bool HasNoDefinedType(JsonContract? contract)
    {
        return contract == null || contract.UnderlyingType == typeof(object) || contract.ContractType == JsonContractType.Linq
               || contract.UnderlyingType == typeof(IDynamicMetaObjectProvider);
    }

    object? EnsureType(JsonReader reader, object? value, CultureInfo culture, JsonContract? contract, Type? targetType)
    {
        if (targetType == null)
        {
            return value;
        }

        MiscellaneousUtils.Assert(contract != null);
        var valueType = ReflectionUtils.GetObjectType(value);

        // type of value and type of target don't match
        // attempt to convert value's type to target's type
        if (valueType != targetType)
        {
            if (value == null && contract.IsNullable)
            {
                return null;
            }

            try
            {
                if (contract.IsConvertable)
                {
                    var primitiveContract = (JsonPrimitiveContract)contract;

                    if (contract.IsEnum)
                    {
                        if (value is string s)
                        {
                            return EnumUtils.ParseEnum(
                                contract.NonNullableUnderlyingType,
                                null,
                                s,
                                false);
                        }
                        if (ConvertUtils.IsInteger(primitiveContract.TypeCode))
                        {
                            return Enum.ToObject(contract.NonNullableUnderlyingType, value);
                        }
                    }
                    else if (contract.NonNullableUnderlyingType == typeof(DateTime))
                    {
                        // use DateTimeUtils because Convert.ChangeType does not set DateTime.Kind correctly
                        if (value is string s && DateTimeUtils.TryParseDateTime(s, reader.DateTimeZoneHandling, reader.DateFormatString, reader.Culture, out var dt))
                        {
                            return DateTimeUtils.EnsureDateTime(dt, reader.DateTimeZoneHandling);
                        }
                    }

                    if (value is BigInteger integer)
                    {
                        return ConvertUtils.FromBigInteger(integer, contract.NonNullableUnderlyingType);
                    }

                    // this won't work when converting to a custom IConvertible
                    return Convert.ChangeType(value, contract.NonNullableUnderlyingType, culture);
                }

                return ConvertUtils.ConvertOrCast(value, culture, contract.NonNullableUnderlyingType);
            }
            catch (Exception ex)
            {
                throw JsonSerializationException.Create(reader,string.Format( "Error converting value {0} to type '{1}'.", MiscellaneousUtils.ToString(value), targetType), ex);
            }
        }

        return value;
    }

    bool SetPropertyValue(JsonProperty property, JsonConverter? propertyConverter, JsonContainerContract? containerContract, JsonProperty? containerProperty, JsonReader reader, object target)
    {
        var skipSettingProperty = CalculatePropertyDetails(
            property,
            ref propertyConverter,
            containerContract,
            containerProperty,
            reader,
            target,
            out var useExistingValue,
            out var currentValue,
            out var propertyContract,
            out var gottenCurrentValue,
            out var ignoredValue);

        if (skipSettingProperty)
        {
            // Don't set extension data if the value was ignored
            // e.g. a null with NullValueHandling should not go in ExtensionData
            if (ignoredValue)
            {
                return true;
            }

            return false;
        }

        object? value;

        if (propertyConverter is {CanRead: true})
        {
            if (!gottenCurrentValue && property.Readable)
            {
                currentValue = property.ValueProvider!.GetValue(target);
            }

            value = DeserializeConvertable(propertyConverter, reader, property.PropertyType!, currentValue);
        }
        else
        {
            value = CreateValueInternal(reader, property.PropertyType, propertyContract, property, containerContract, containerProperty, useExistingValue ? currentValue : null);
        }

        // always set the value if useExistingValue is false,
        // otherwise also set it if CreateValue returns a new value compared to the currentValue
        // this could happen because of a JsonConverter against the type
        if ((!useExistingValue || value != currentValue)
            && ShouldSetPropertyValue(property, containerContract as JsonObjectContract, value))
        {
            property.ValueProvider!.SetValue(target, value);

            if (property.SetIsSpecified != null)
            {
                if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
                {
                    TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("IsSpecified for property '{0}' on {1} set to true.", property.PropertyName, property.DeclaringType)), null);
                }

                property.SetIsSpecified(target, true);
            }

            return true;
        }

        // the value wasn't set be JSON was populated onto the existing value
        return useExistingValue;
    }

    bool CalculatePropertyDetails(
        JsonProperty property,
        ref JsonConverter? propertyConverter,
        JsonContainerContract? containerContract,
        JsonProperty? containerProperty,
        JsonReader reader,
        object target,
        out bool useExistingValue,
        out object? currentValue,
        out JsonContract? propertyContract,
        out bool gottenCurrentValue,
        out bool ignoredValue)
    {
        currentValue = null;
        useExistingValue = false;
        propertyContract = null;
        gottenCurrentValue = false;
        ignoredValue = false;

        if (property.Ignored)
        {
            return true;
        }

        var tokenType = reader.TokenType;

        if (property.PropertyContract == null)
        {
            property.PropertyContract = GetContractSafe(property.PropertyType);
        }

        var objectCreationHandling =
            property.ObjectCreationHandling.GetValueOrDefault(Serializer._objectCreationHandling);

        if (objectCreationHandling != ObjectCreationHandling.Replace
            && (tokenType is JsonToken.StartArray or JsonToken.StartObject || propertyConverter != null)
            && property.Readable
            && property.PropertyContract?.ContractType != JsonContractType.Linq)
        {
            currentValue = property.ValueProvider!.GetValue(target);
            gottenCurrentValue = true;

            if (currentValue != null)
            {
                propertyContract = GetContract(currentValue.GetType());

                useExistingValue = !propertyContract.IsReadOnlyOrFixedSize && !propertyContract.UnderlyingType.IsValueType;
            }
        }

        if (!property.Writable && !useExistingValue)
        {
            if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
            {
                TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path,string.Format( "Unable to deserialize value to non-writable property '{0}' on {1}.", property.PropertyName, property.DeclaringType)), null);
            }

            return true;
        }

        // test tokenType here because null might not be convertible to some types, e.g. ignoring null when applied to DateTime
        if (tokenType == JsonToken.Null && ResolvedNullValueHandling(containerContract as JsonObjectContract, property) == NullValueHandling.Ignore)
        {
            ignoredValue = true;
            return true;
        }

        // test tokenType here because default value might not be convertible to actual type, e.g. default of "" for DateTime
        if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Ignore)
            && !HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate)
            && JsonTokenUtils.IsPrimitiveToken(tokenType)
            && MiscellaneousUtils.ValueEquals(reader.Value, property.GetResolvedDefaultValue()))
        {
            ignoredValue = true;
            return true;
        }

        if (currentValue == null)
        {
            propertyContract = property.PropertyContract;
        }
        else
        {
            propertyContract = GetContract(currentValue.GetType());

            if (propertyContract != property.PropertyContract)
            {
                propertyConverter = GetConverter(propertyContract, property.Converter, containerContract, containerProperty);
            }
        }

        return false;
    }

    void AddReference(JsonReader reader, string id, object value)
    {
        try
        {
            if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
            {
                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("Read object reference Id '{0}' for {1}.", id, value.GetType())), null);
            }

            Serializer.GetReferenceResolver().AddReference(this, id, value);
        }
        catch (Exception ex)
        {
            throw JsonSerializationException.Create(reader, string.Format("Error reading object reference '{0}'.", id), ex);
        }
    }

    bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
    {
        return (value & flag) == flag;
    }

    bool ShouldSetPropertyValue(JsonProperty property, JsonObjectContract? contract, object? value)
    {
        if (value == null && ResolvedNullValueHandling(contract, property) == NullValueHandling.Ignore)
        {
            return false;
        }

        if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Ignore)
            && !HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate)
            && MiscellaneousUtils.ValueEquals(value, property.GetResolvedDefaultValue()))
        {
            return false;
        }

        if (!property.Writable)
        {
            return false;
        }

        return true;
    }

    IList CreateNewList(JsonReader reader, JsonArrayContract contract, out bool createdFromNonDefaultCreator)
    {
        // some types like non-generic IEnumerable can be serialized but not deserialized
        if (!contract.CanDeserialize)
        {
            throw JsonSerializationException.Create(reader, string.Format("Cannot create and populate list type {0}.", contract.CreatedType));
        }

        if (contract.OverrideCreator != null)
        {
            if (contract.HasParameterizedCreator)
            {
                createdFromNonDefaultCreator = true;
                return contract.CreateTemporaryCollection();
            }
            else
            {
                var list = contract.OverrideCreator();

                if (contract.ShouldCreateWrapper)
                {
                    list = contract.CreateWrapper(list);
                }

                createdFromNonDefaultCreator = false;
                return (IList)list;
            }
        }
        else if (contract.IsReadOnlyOrFixedSize)
        {
            createdFromNonDefaultCreator = true;
            var list = contract.CreateTemporaryCollection();

            if (contract.ShouldCreateWrapper)
            {
                list = contract.CreateWrapper(list);
            }

            return list;
        }
        else if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
        {
            var list = contract.DefaultCreator();

            if (contract.ShouldCreateWrapper)
            {
                list = contract.CreateWrapper(list);
            }

            createdFromNonDefaultCreator = false;
            return (IList)list;
        }
        else if (contract.HasParameterizedCreatorInternal)
        {
            createdFromNonDefaultCreator = true;
            return contract.CreateTemporaryCollection();
        }
        else
        {
            if (!contract.IsInstantiable)
            {
                throw JsonSerializationException.Create(reader, string.Format("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.", contract.UnderlyingType));
            }

            throw JsonSerializationException.Create(reader, string.Format("Unable to find a constructor to use for type {0}.", contract.UnderlyingType));
        }
    }

    IDictionary CreateNewDictionary(JsonReader reader, JsonDictionaryContract contract, out bool createdFromNonDefaultCreator)
    {
        if (contract.OverrideCreator != null)
        {
            if (contract.HasParameterizedCreator)
            {
                createdFromNonDefaultCreator = true;
                return contract.CreateTemporaryDictionary();
            }
            else
            {
                createdFromNonDefaultCreator = false;
                return (IDictionary)contract.OverrideCreator();
            }
        }
        else if (contract.IsReadOnlyOrFixedSize)
        {
            createdFromNonDefaultCreator = true;
            return contract.CreateTemporaryDictionary();
        }
        else if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
        {
            var dictionary = contract.DefaultCreator();

            if (contract.ShouldCreateWrapper)
            {
                dictionary = contract.CreateWrapper(dictionary);
            }

            createdFromNonDefaultCreator = false;
            return (IDictionary)dictionary;
        }
        else if (contract.HasParameterizedCreatorInternal)
        {
            createdFromNonDefaultCreator = true;
            return contract.CreateTemporaryDictionary();
        }
        else
        {
            if (!contract.IsInstantiable)
            {
                throw JsonSerializationException.Create(reader, string.Format("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.", contract.UnderlyingType));
            }

            throw JsonSerializationException.Create(reader,string.Format( "Unable to find a default constructor to use for type {0}.", contract.UnderlyingType));
        }
    }

    void OnDeserializing(JsonReader reader, JsonContract contract, object value)
    {
        if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
        {
            TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("Started deserializing {0}", contract.UnderlyingType)), null);
        }

        contract.InvokeOnDeserializing(value, Serializer._context);
    }

    void OnDeserialized(JsonReader reader, JsonContract contract, object value)
    {
        if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
        {
            TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path,string.Format( "Finished deserializing {0}", contract.UnderlyingType)), null);
        }

        contract.InvokeOnDeserialized(value, Serializer._context);
    }

    object PopulateDictionary(IDictionary dictionary, JsonReader reader, JsonDictionaryContract contract, JsonProperty? containerProperty, string? id)
    {
        var underlyingDictionary = dictionary is IWrappedDictionary wrappedDictionary ? wrappedDictionary.UnderlyingDictionary : dictionary;

        if (id != null)
        {
            AddReference(reader, id, underlyingDictionary);
        }

        OnDeserializing(reader, contract, underlyingDictionary);

        var initialDepth = reader.Depth;

        if (contract.KeyContract == null)
        {
            contract.KeyContract = GetContractSafe(contract.DictionaryKeyType);
        }

        if (contract.ItemContract == null)
        {
            contract.ItemContract = GetContractSafe(contract.DictionaryValueType);
        }

        var dictionaryValueConverter = contract.ItemConverter ?? GetConverter(contract.ItemContract, null, contract, containerProperty);
        var keyTypeCode = contract.KeyContract is JsonPrimitiveContract keyContract ? keyContract.TypeCode : PrimitiveTypeCode.Empty;

        var finished = false;
        do
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    var keyValue = reader.Value!;
                    if (CheckPropertyName(reader, keyValue.ToString()))
                    {
                        continue;
                    }

                    try
                    {
                        try
                        {
                            // this is for correctly reading ISO and MS formatted dictionary keys
                            switch (keyTypeCode)
                            {
                                case PrimitiveTypeCode.DateTime:
                                case PrimitiveTypeCode.DateTimeNullable:
                                {
                                    keyValue = DateTimeUtils.TryParseDateTime(keyValue.ToString(), reader.DateTimeZoneHandling, reader.DateFormatString, reader.Culture, out var dt)
                                        ? dt
                                        : EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType)!;
                                    break;
                                }
                                case PrimitiveTypeCode.DateTimeOffset:
                                case PrimitiveTypeCode.DateTimeOffsetNullable:
                                {
                                    keyValue = DateTimeUtils.TryParseDateTimeOffset(keyValue.ToString(), reader.DateFormatString, reader.Culture, out var dt)
                                        ? dt
                                        : EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType)!;
                                    break;
                                }
                                default:
                                    keyValue = contract.KeyContract is {IsEnum: true}
                                        ? EnumUtils.ParseEnum(contract.KeyContract.NonNullableUnderlyingType, (Serializer._contractResolver as DefaultContractResolver)?.NamingStrategy, keyValue.ToString(), false)
                                        : EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType)!;
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw JsonSerializationException.Create(reader,string.Format( "Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.", reader.Value, contract.DictionaryKeyType), ex);
                        }

                        if (!reader.ReadForType(contract.ItemContract, dictionaryValueConverter != null))
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
                        }

                        object? itemValue;
                        if (dictionaryValueConverter is {CanRead: true})
                        {
                            itemValue = DeserializeConvertable(dictionaryValueConverter, reader, contract.DictionaryValueType!, null);
                        }
                        else
                        {
                            itemValue = CreateValueInternal(reader, contract.DictionaryValueType, contract.ItemContract, null, contract, containerProperty, null);
                        }

                        dictionary[keyValue] = itemValue;
                    }
                    catch (Exception ex)
                    {
                        if (IsErrorHandled(underlyingDictionary, contract, keyValue, reader as IJsonLineInfo, reader.Path, ex))
                        {
                            HandleError(reader, true, initialDepth);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    break;
                case JsonToken.Comment:
                    break;
                case JsonToken.EndObject:
                    finished = true;
                    break;
                default:
                    throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
            }
        } while (!finished && reader.Read());

        if (!finished)
        {
            ThrowUnexpectedEndException(reader, contract, underlyingDictionary, "Unexpected end when deserializing object.");
        }

        OnDeserialized(reader, contract, underlyingDictionary);
        return underlyingDictionary;
    }

    object PopulateMultidimensionalArray(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty? containerProperty, string? id)
    {
        var rank = contract.UnderlyingType.GetArrayRank();

        if (id != null)
        {
            AddReference(reader, id, list);
        }

        OnDeserializing(reader, contract, list);

        var collectionItemContract = GetContractSafe(contract.CollectionItemType);
        var collectionItemConverter = GetConverter(collectionItemContract, null, contract, containerProperty);

        int? previousErrorIndex = null;
        var listStack = new Stack<IList>();
        listStack.Push(list);
        var currentList = list;

        var finished = false;
        do
        {
            var initialDepth = reader.Depth;

            if (listStack.Count == rank)
            {
                try
                {
                    if (reader.ReadForType(collectionItemContract, collectionItemConverter != null))
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.EndArray:
                                listStack.Pop();
                                currentList = listStack.Peek();
                                previousErrorIndex = null;
                                break;
                            case JsonToken.Comment:
                                break;
                            default:
                                object? value;

                                if (collectionItemConverter is {CanRead: true})
                                {
                                    value = DeserializeConvertable(collectionItemConverter, reader, contract.CollectionItemType!, null);
                                }
                                else
                                {
                                    value = CreateValueInternal(reader, contract.CollectionItemType, collectionItemContract, null, contract, containerProperty, null);
                                }

                                currentList.Add(value);
                                break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    var errorPosition = reader.GetPosition(initialDepth);

                    if (IsErrorHandled(list, contract, errorPosition.Position, reader as IJsonLineInfo, reader.Path, ex))
                    {
                        HandleError(reader, true, initialDepth + 1);

                        if (previousErrorIndex != null && previousErrorIndex == errorPosition.Position)
                        {
                            // reader index has not moved since previous error handling
                            // break out of reading array to prevent infinite loop
                            throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
                        }
                        else
                        {
                            previousErrorIndex = errorPosition.Position;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                if (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.StartArray:
                            IList newList = new List<object>();
                            currentList.Add(newList);
                            listStack.Push(newList);
                            currentList = newList;
                            break;
                        case JsonToken.EndArray:
                            listStack.Pop();

                            if (listStack.Count > 0)
                            {
                                currentList = listStack.Peek();
                            }
                            else
                            {
                                finished = true;
                            }
                            break;
                        case JsonToken.Comment:
                            break;
                        default:
                            throw JsonSerializationException.Create(reader, "Unexpected token when deserializing multidimensional array: " + reader.TokenType);
                    }
                }
                else
                {
                    break;
                }
            }
        } while (!finished);

        if (!finished)
        {
            ThrowUnexpectedEndException(reader, contract, list, "Unexpected end when deserializing array.");
        }

        OnDeserialized(reader, contract, list);
        return list;
    }

    void ThrowUnexpectedEndException(JsonReader reader, JsonContract contract, object? currentObject, string message)
    {
        try
        {
            throw JsonSerializationException.Create(reader, message);
        }
        catch (Exception ex)
        {
            if (IsErrorHandled(currentObject, contract, null, reader as IJsonLineInfo, reader.Path, ex))
            {
                HandleError(reader, false, 0);
            }
            else
            {
                throw;
            }
        }
    }

    object PopulateList(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty? containerProperty, string? id)
    {
#pragma warning disable CS8600, CS8602, CS8603, CS8604
        var underlyingList = list is IWrappedCollection wrappedCollection ? wrappedCollection.UnderlyingCollection : list;

        if (id != null)
        {
            AddReference(reader, id, underlyingList);
        }

        // can't populate an existing array
        if (list.IsFixedSize)
        {
            reader.Skip();
            return underlyingList;
        }

        OnDeserializing(reader, contract, underlyingList);

        var initialDepth = reader.Depth;

        if (contract.ItemContract == null)
        {
            contract.ItemContract = GetContractSafe(contract.CollectionItemType);
        }

        var collectionItemConverter = GetConverter(contract.ItemContract, null, contract, containerProperty);

        int? previousErrorIndex = null;

        var finished = false;
        do
        {
            try
            {
                if (reader.ReadForType(contract.ItemContract, collectionItemConverter != null))
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.EndArray:
                            finished = true;
                            break;
                        case JsonToken.Comment:
                            break;
                        default:
                            object? value;

                            if (collectionItemConverter is {CanRead: true})
                            {
                                value = DeserializeConvertable(collectionItemConverter, reader, contract.CollectionItemType, null);
                            }
                            else
                            {
                                value = CreateValueInternal(reader, contract.CollectionItemType, contract.ItemContract, null, contract, containerProperty, null);
                            }

                            list.Add(value);
                            break;
                    }
                }
                else
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                var errorPosition = reader.GetPosition(initialDepth);

                if (IsErrorHandled(underlyingList, contract, errorPosition.Position, reader as IJsonLineInfo, reader.Path, ex))
                {
                    HandleError(reader, true, initialDepth + 1);

                    if (previousErrorIndex != null && previousErrorIndex == errorPosition.Position)
                    {
                        // reader index has not moved since previous error handling
                        // break out of reading array to prevent infinite loop
                        throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
                    }
                    else
                    {
                        previousErrorIndex = errorPosition.Position;
                    }
                }
                else
                {
                    throw;
                }
            }
        } while (!finished);

        if (!finished)
        {
            ThrowUnexpectedEndException(reader, contract, underlyingList, "Unexpected end when deserializing array.");
        }

        OnDeserialized(reader, contract, underlyingList);
        return underlyingList;
#pragma warning restore CS8600, CS8602, CS8603, CS8604
    }

    object CreateISerializable(JsonReader reader, JsonISerializableContract contract, JsonProperty? member, string? id)
    {
        var objectType = contract.UnderlyingType;

        if (!JsonTypeReflector.FullyTrusted)
        {
            var message = @"Type '{0}' implements ISerializable but cannot be deserialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine +
                          @"To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine;
            message = string.Format(message, objectType);

            throw JsonSerializationException.Create(reader, message);
        }

        if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
        {
            TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("Deserializing {0} using ISerializable constructor.", contract.UnderlyingType)), null);
        }

        var serializationInfo = new SerializationInfo(contract.UnderlyingType, new JsonFormatterConverter(this, contract, member));

        var finished = false;
        do
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    var memberName = reader.Value!.ToString();
                    if (!reader.Read())
                    {
                        throw JsonSerializationException.Create(reader, string.Format("Unexpected end when setting {0}'s value.", memberName));
                    }
                    serializationInfo.AddValue(memberName, JToken.ReadFrom(reader));
                    break;
                case JsonToken.Comment:
                    break;
                case JsonToken.EndObject:
                    finished = true;
                    break;
                default:
                    throw JsonSerializationException.Create(reader, $"Unexpected token when deserializing object: {reader.TokenType}");
            }
        } while (!finished && reader.Read());

        if (!finished)
        {
            ThrowUnexpectedEndException(reader, contract, serializationInfo, "Unexpected end when deserializing object.");
        }

        if (!contract.IsInstantiable)
        {
            throw JsonSerializationException.Create(reader, string.Format("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.", contract.UnderlyingType));
        }

        if (contract.ISerializableCreator == null)
        {
            throw JsonSerializationException.Create(reader, string.Format("ISerializable type '{0}' does not have a valid constructor. To correctly implement ISerializable a constructor that takes SerializationInfo and StreamingContext parameters should be present.", objectType));
        }

        var createdObject = contract.ISerializableCreator(serializationInfo, Serializer._context);

        if (id != null)
        {
            AddReference(reader, id, createdObject);
        }

        // these are together because OnDeserializing takes an object but for an ISerializable the object is fully created in the constructor
        OnDeserializing(reader, contract, createdObject);
        OnDeserialized(reader, contract, createdObject);

        return createdObject;
    }

    internal object? CreateISerializableItem(JToken token, Type type, JsonISerializableContract contract, JsonProperty? member)
    {
        var itemContract = GetContractSafe(type);
        var itemConverter = GetConverter(itemContract, null, contract, member);

        var tokenReader = token.CreateReader();
        tokenReader.ReadAndAssert(); // Move to first token

        object? result;
        if (itemConverter is {CanRead: true})
        {
            result = DeserializeConvertable(itemConverter, tokenReader, type, null);
        }
        else
        {
            result = CreateValueInternal(tokenReader, type, itemContract, null, contract, member, null);
        }

        return result;
    }

    object CreateDynamic(JsonReader reader, JsonDynamicContract contract, JsonProperty? member, string? id)
    {
        IDynamicMetaObjectProvider newObject;

        if (!contract.IsInstantiable)
        {
            throw JsonSerializationException.Create(reader, string.Format("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.", contract.UnderlyingType));
        }

        if (contract.DefaultCreator != null &&
            (!contract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
        {
            newObject = (IDynamicMetaObjectProvider)contract.DefaultCreator();
        }
        else
        {
            throw JsonSerializationException.Create(reader, string.Format("Unable to find a default constructor to use for type {0}.", contract.UnderlyingType));
        }

        if (id != null)
        {
            AddReference(reader, id, newObject);
        }

        OnDeserializing(reader, contract, newObject);

        var initialDepth = reader.Depth;

        var finished = false;
        do
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    var memberName = reader.Value!.ToString();

                    try
                    {
                        if (!reader.Read())
                        {
                            throw JsonSerializationException.Create(reader, string.Format("Unexpected end when setting {0}'s value.", memberName));
                        }

                        // first attempt to find a settable property, otherwise fall back to a dynamic set without type
                        var property = contract.Properties.GetClosestMatchProperty(memberName);

                        if (property is {Writable: true, Ignored: false})
                        {
                            if (property.PropertyContract == null)
                            {
                                property.PropertyContract = GetContractSafe(property.PropertyType);
                            }

                            var propertyConverter = GetConverter(property.PropertyContract, property.Converter, null, null);

                            if (!SetPropertyValue(property, propertyConverter, null, member, reader, newObject))
                            {
                                reader.Skip();
                            }
                        }
                        else
                        {
                            var t = JsonTokenUtils.IsPrimitiveToken(reader.TokenType) ? reader.ValueType! : typeof(IDynamicMetaObjectProvider);

                            var dynamicMemberContract = GetContractSafe(t);
                            var dynamicMemberConverter = GetConverter(dynamicMemberContract, null, null, member);

                            object? value;
                            if (dynamicMemberConverter is {CanRead: true})
                            {
                                value = DeserializeConvertable(dynamicMemberConverter!, reader, t, null);
                            }
                            else
                            {
                                value = CreateValueInternal(reader, t, dynamicMemberContract, null, null, member, null);
                            }

                            contract.TrySetMember(newObject, memberName, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (IsErrorHandled(newObject, contract, memberName, reader as IJsonLineInfo, reader.Path, ex))
                        {
                            HandleError(reader, true, initialDepth);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    break;
                case JsonToken.EndObject:
                    finished = true;
                    break;
                default:
                    throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
            }
        } while (!finished && reader.Read());

        if (!finished)
        {
            ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");
        }

        OnDeserialized(reader, contract, newObject);

        return newObject;
    }

    internal class CreatorPropertyContext
    {
        public readonly string Name;
        public JsonProperty? Property;
        public JsonProperty? ConstructorProperty;
        public PropertyPresence? Presence;
        public object? Value;
        public bool Used;

        public CreatorPropertyContext(string name)
        {
            Name = name;
        }
    }

    object CreateObjectUsingCreatorWithParameters(JsonReader reader, JsonObjectContract contract, JsonProperty? containerProperty, ObjectConstructor<object> creator, string? id)
    {
        ValidationUtils.ArgumentNotNull(creator, nameof(creator));

        // only need to keep a track of properties' presence if they are required or a value should be defaulted if missing
        var trackPresence = contract.HasRequiredOrDefaultValueProperties || HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Populate);

        var objectType = contract.UnderlyingType;

        if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
        {
            var parameters = string.Join(", ", contract.CreatorParameters.Select(p => p.PropertyName));
            TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("Deserializing {0} using creator with parameters: {1}.", contract.UnderlyingType, parameters)), null);
        }

        var propertyContexts = ResolvePropertyAndCreatorValues(contract, containerProperty, reader, objectType);
        if (trackPresence)
        {
            foreach (var property in contract.Properties)
            {
                if (!property.Ignored)
                {
                    if (propertyContexts.All(p => p.Property != property))
                    {
                        propertyContexts.Add(new CreatorPropertyContext(property.PropertyName!)
                        {
                            Property = property,
                            Presence = PropertyPresence.None
                        });
                    }
                }
            }
        }

        var creatorParameterValues = new object?[contract.CreatorParameters.Count];

        foreach (var context in propertyContexts)
        {
            // set presence of read values
            if (trackPresence)
            {
                if (context.Property != null && context.Presence == null)
                {
                    var v = context.Value;
                    PropertyPresence propertyPresence;
                    if (v == null)
                    {
                        propertyPresence = PropertyPresence.Null;
                    }
                    else if (v is string s)
                    {
                        propertyPresence = CoerceEmptyStringToNull(context.Property.PropertyType, context.Property.PropertyContract, s)
                            ? PropertyPresence.Null
                            : PropertyPresence.Value;
                    }
                    else
                    {
                        propertyPresence = PropertyPresence.Value;
                    }

                    context.Presence = propertyPresence;
                }
            }

            var constructorProperty = context.ConstructorProperty;
            if (constructorProperty == null && context.Property != null)
            {
                constructorProperty = contract.CreatorParameters.ForgivingCaseSensitiveFind(p => p.PropertyName!, context.Property.UnderlyingName!);
            }

            if (constructorProperty is {Ignored: false})
            {
                // handle giving default values to creator parameters
                // this needs to happen before the call to creator
                if (trackPresence)
                {
                    if (context.Presence is PropertyPresence.None or PropertyPresence.Null)
                    {
                        if (constructorProperty.PropertyContract == null)
                        {
                            constructorProperty.PropertyContract = GetContractSafe(constructorProperty.PropertyType);
                        }

                        if (HasFlag(constructorProperty.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate))
                        {
                            context.Value = EnsureType(
                                reader,
                                constructorProperty.GetResolvedDefaultValue(),
                                CultureInfo.InvariantCulture,
                                constructorProperty.PropertyContract!,
                                constructorProperty.PropertyType);
                        }
                    }
                }

                var i = contract.CreatorParameters.IndexOf(constructorProperty);
                creatorParameterValues[i] = context.Value;

                context.Used = true;
            }
        }

        var createdObject = creator(creatorParameterValues);

        if (id != null)
        {
            AddReference(reader, id, createdObject);
        }

        OnDeserializing(reader, contract, createdObject);

        // go through unused values and set the newly created object's properties
        foreach (var context in propertyContexts)
        {
            if (context.Used ||
                context.Property == null ||
                context.Property.Ignored ||
                context.Presence == PropertyPresence.None)
            {
                continue;
            }

            var property = context.Property;
            var value = context.Value;

            if (ShouldSetPropertyValue(property, contract, value))
            {
                property.ValueProvider!.SetValue(createdObject, value);
                context.Used = true;
            }
            else if (!property.Writable && value != null)
            {
                // handle readonly collection/dictionary properties
                var propertyContract = Serializer._contractResolver.ResolveContract(property.PropertyType!);

                if (propertyContract.ContractType == JsonContractType.Array)
                {
                    var propertyArrayContract = (JsonArrayContract)propertyContract;

                    if (propertyArrayContract.CanDeserialize && !propertyArrayContract.IsReadOnlyOrFixedSize)
                    {
                        var createdObjectCollection = property.ValueProvider!.GetValue(createdObject);
                        if (createdObjectCollection != null)
                        {
                            propertyArrayContract = (JsonArrayContract)GetContract(createdObjectCollection.GetType());

                            var createdObjectCollectionWrapper = propertyArrayContract.ShouldCreateWrapper ? propertyArrayContract.CreateWrapper(createdObjectCollection) : (IList)createdObjectCollection;

                            // Don't attempt to populate array/read-only list
                            if (!createdObjectCollectionWrapper.IsFixedSize)
                            {
                                var newValues = propertyArrayContract.ShouldCreateWrapper ? propertyArrayContract.CreateWrapper(value) : (IList)value;

                                foreach (var newValue in newValues)
                                {
                                    createdObjectCollectionWrapper.Add(newValue);
                                }
                            }
                        }
                    }
                }
                else if (propertyContract.ContractType == JsonContractType.Dictionary)
                {
                    var dictionaryContract = (JsonDictionaryContract)propertyContract;

                    if (!dictionaryContract.IsReadOnlyOrFixedSize)
                    {
                        var createdObjectDictionary = property.ValueProvider!.GetValue(createdObject);
                        if (createdObjectDictionary != null)
                        {
                            var targetDictionary = dictionaryContract.ShouldCreateWrapper ? dictionaryContract.CreateWrapper(createdObjectDictionary) : (IDictionary)createdObjectDictionary;
                            var newValues = dictionaryContract.ShouldCreateWrapper ? dictionaryContract.CreateWrapper(value) : (IDictionary)value;

                            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
                            var e = newValues.GetEnumerator();
                            try
                            {
                                while (e.MoveNext())
                                {
                                    var entry = e.Entry;
                                    targetDictionary[entry.Key] = entry.Value;
                                }
                            }
                            finally
                            {
                                (e as IDisposable)?.Dispose();
                            }
                        }
                    }
                }

                context.Used = true;
            }
        }

        if (contract.ExtensionDataSetter != null)
        {
            foreach (var propertyValue in propertyContexts)
            {
                if (!propertyValue.Used && propertyValue.Presence != PropertyPresence.None)
                {
                    contract.ExtensionDataSetter(createdObject, propertyValue.Name, propertyValue.Value);
                }
            }
        }

        if (trackPresence)
        {
            foreach (var context in propertyContexts)
            {
                if (context.Property == null)
                {
                    continue;
                }

                EndProcessProperty(
                    createdObject,
                    reader,
                    contract,
                    reader.Depth,
                    context.Property,
                    context.Presence.GetValueOrDefault(),
                    !context.Used);
            }
        }

        OnDeserialized(reader, contract, createdObject);
        return createdObject;
    }

    object? DeserializeConvertable(JsonConverter converter, JsonReader reader, Type objectType, object? existingValue)
    {
        if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
        {
            TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("Started deserializing {0} with converter {1}.", objectType, converter.GetType())), null);
        }

        var value = converter.ReadJson(reader, objectType, existingValue, GetInternalSerializer());

        if (TraceWriter is {LevelFilter: >= TraceLevel.Info})
        {
            TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("Finished deserializing {0} with converter {1}.", objectType, converter.GetType())), null);
        }

        return value;
    }

    List<CreatorPropertyContext> ResolvePropertyAndCreatorValues(JsonObjectContract contract, JsonProperty? containerProperty, JsonReader reader, Type objectType)
    {
        var propertyValues = new List<CreatorPropertyContext>();
        var exit = false;
        do
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    var memberName = reader.Value!.ToString();

                    var creatorPropertyContext = new CreatorPropertyContext(memberName)
                    {
                        ConstructorProperty = contract.CreatorParameters.GetClosestMatchProperty(memberName),
                        Property = contract.Properties.GetClosestMatchProperty(memberName)
                    };
                    propertyValues.Add(creatorPropertyContext);

                    var property = creatorPropertyContext.ConstructorProperty ?? creatorPropertyContext.Property;
                    if (property != null)
                    {
                        if (!property.Ignored)
                        {
                            if (property.PropertyContract == null)
                            {
                                property.PropertyContract = GetContractSafe(property.PropertyType);
                            }

                            var propertyConverter = GetConverter(property.PropertyContract, property.Converter, contract, containerProperty);

                            if (!reader.ReadForType(property.PropertyContract, propertyConverter != null))
                            {
                                throw JsonSerializationException.Create(reader, string.Format("Unexpected end when setting {0}'s value.", memberName));
                            }

                            if (propertyConverter is {CanRead: true})
                            {
                                creatorPropertyContext.Value = DeserializeConvertable(propertyConverter, reader, property.PropertyType!, null);
                            }
                            else
                            {
                                creatorPropertyContext.Value = CreateValueInternal(reader, property.PropertyType, property.PropertyContract, property, contract, containerProperty, null);
                            }

                            continue;
                        }
                    }
                    else
                    {
                        if (!reader.Read())
                        {
                            throw JsonSerializationException.Create(reader, string.Format("Unexpected end when setting {0}'s value.", memberName));
                        }

                        if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
                        {
                            TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("Could not find member '{0}' on {1}.", memberName, contract.UnderlyingType)), null);
                        }

                        if ((contract.MissingMemberHandling ?? Serializer._missingMemberHandling) == MissingMemberHandling.Error)
                        {
                            throw JsonSerializationException.Create(reader, string.Format("Could not find member '{0}' on object of type '{1}'", memberName, objectType.Name));
                        }
                    }

                    if (contract.ExtensionDataSetter != null)
                    {
                        creatorPropertyContext.Value = ReadExtensionDataValue(contract, containerProperty, reader);
                    }
                    else
                    {
                        reader.Skip();
                    }
                    break;
                case JsonToken.Comment:
                    break;
                case JsonToken.EndObject:
                    exit = true;
                    break;
                default:
                    throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
            }
        } while (!exit && reader.Read());

        if (!exit)
        {
            ThrowUnexpectedEndException(reader, contract, null, "Unexpected end when deserializing object.");
        }

        return propertyValues;
    }

    public object CreateNewObject(JsonReader reader, JsonObjectContract objectContract, JsonProperty? containerMember, JsonProperty? containerProperty, string? id, out bool createdFromNonDefaultCreator)
    {
        object? newObject = null;

        if (objectContract.OverrideCreator != null)
        {
            if (objectContract.CreatorParameters.Count > 0)
            {
                createdFromNonDefaultCreator = true;
                return CreateObjectUsingCreatorWithParameters(reader, objectContract, containerMember, objectContract.OverrideCreator, id);
            }

            newObject = objectContract.OverrideCreator(CollectionUtils.ArrayEmpty<object>());
        }
        else if (objectContract.DefaultCreator != null &&
                 (!objectContract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor || objectContract.ParameterizedCreator == null))
        {
            // use the default constructor if it is...
            // public
            // non-public and the user has change constructor handling settings
            // non-public and there is no other creator
            newObject = objectContract.DefaultCreator();
        }
        else if (objectContract.ParameterizedCreator != null)
        {
            createdFromNonDefaultCreator = true;
            return CreateObjectUsingCreatorWithParameters(reader, objectContract, containerMember, objectContract.ParameterizedCreator, id);
        }

        if (newObject == null)
        {
            if (!objectContract.IsInstantiable)
            {
                throw JsonSerializationException.Create(reader, string.Format("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.", objectContract.UnderlyingType));
            }

            throw JsonSerializationException.Create(reader, string.Format("Unable to find a constructor to use for type {0}. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.", objectContract.UnderlyingType));
        }

        createdFromNonDefaultCreator = false;
        return newObject;
    }

    object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, JsonProperty? member, string? id)
    {
        OnDeserializing(reader, contract, newObject);

        // only need to keep a track of properties' presence if they are required or a value should be defaulted if missing
        var propertiesPresence = contract.HasRequiredOrDefaultValueProperties || HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Populate)
            ? contract.Properties.ToDictionary(m => m, _ => PropertyPresence.None)
            : null;

        if (id != null)
        {
            AddReference(reader, id, newObject);
        }

        var initialDepth = reader.Depth;

        var finished = false;
        do
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                {
                    var propertyName = reader.Value!.ToString();

                    if (CheckPropertyName(reader, propertyName))
                    {
                        continue;
                    }

                    try
                    {
                        // attempt exact case match first
                        // then try match ignoring case
                        var property = contract.Properties.GetClosestMatchProperty(propertyName);

                        if (property == null)
                        {
                            if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
                            {
                                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, string.Format("Could not find member '{0}' on {1}", propertyName, contract.UnderlyingType)), null);
                            }

                            if ((contract.MissingMemberHandling ?? Serializer._missingMemberHandling) == MissingMemberHandling.Error)
                            {
                                throw JsonSerializationException.Create(reader, string.Format("Could not find member '{0}' on object of type '{1}'", propertyName, contract.UnderlyingType.Name));
                            }

                            if (!reader.Read())
                            {
                                break;
                            }

                            SetExtensionData(contract, member, reader, propertyName, newObject);
                            continue;
                        }

                        if (property.Ignored || !ShouldDeserialize(reader, property, newObject))
                        {
                            if (!reader.Read())
                            {
                                break;
                            }

                            SetPropertyPresence(reader, property, propertiesPresence);
                            SetExtensionData(contract, member, reader, propertyName, newObject);
                        }
                        else
                        {
                            if (property.PropertyContract == null)
                            {
                                property.PropertyContract = GetContractSafe(property.PropertyType);
                            }

                            var propertyConverter = GetConverter(property.PropertyContract, property.Converter, contract, member);

                            if (!reader.ReadForType(property.PropertyContract, propertyConverter != null))
                            {
                                throw JsonSerializationException.Create(reader, string.Format("Unexpected end when setting {0}'s value.", propertyName));
                            }

                            SetPropertyPresence(reader, property, propertiesPresence);

                            // set extension data if property is ignored or readonly
                            if (!SetPropertyValue(property, propertyConverter, contract, member, reader, newObject))
                            {
                                SetExtensionData(contract, member, reader, propertyName, newObject);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (IsErrorHandled(newObject, contract, propertyName, reader as IJsonLineInfo, reader.Path, ex))
                        {
                            HandleError(reader, true, initialDepth);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    break;
                }
                case JsonToken.EndObject:
                    finished = true;
                    break;
                case JsonToken.Comment:
                    // ignore
                    break;
                default:
                    throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
            }
        } while (!finished && reader.Read());

        if (!finished)
        {
            ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");
        }

        if (propertiesPresence != null)
        {
            foreach (var propertyPresence in propertiesPresence)
            {
                var property = propertyPresence.Key;
                var presence = propertyPresence.Value;

                EndProcessProperty(newObject, reader, contract, initialDepth, property, presence, true);
            }
        }

        OnDeserialized(reader, contract, newObject);
        return newObject;
    }

    bool ShouldDeserialize(JsonReader reader, JsonProperty property, object target)
    {
        if (property.ShouldDeserialize == null)
        {
            return true;
        }

        var shouldDeserialize = property.ShouldDeserialize(target);

        if (TraceWriter is {LevelFilter: >= TraceLevel.Verbose})
        {
            TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, reader.Path,string.Format( "ShouldDeserialize result for property '{0}' on {1}: {2}", property.PropertyName, property.DeclaringType, shouldDeserialize)), null);
        }

        return shouldDeserialize;
    }

    bool CheckPropertyName(JsonReader reader, string memberName)
    {
        if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
        {
            switch (memberName)
            {
                case JsonTypeReflector.IdPropertyName:
                case JsonTypeReflector.RefPropertyName:
                case JsonTypeReflector.TypePropertyName:
                case JsonTypeReflector.ArrayValuesPropertyName:
                    reader.Skip();
                    return true;
            }
        }
        return false;
    }

    void SetExtensionData(JsonObjectContract contract, JsonProperty? member, JsonReader reader, string memberName, object o)
    {
        if (contract.ExtensionDataSetter != null)
        {
            try
            {
                var value = ReadExtensionDataValue(contract, member, reader);

                contract.ExtensionDataSetter(o, memberName, value);
            }
            catch (Exception ex)
            {
                throw JsonSerializationException.Create(reader,string.Format( "Error setting value in extension data for type '{0}'.", contract.UnderlyingType), ex);
            }
        }
        else
        {
            reader.Skip();
        }
    }

    object? ReadExtensionDataValue(JsonObjectContract contract, JsonProperty? member, JsonReader reader)
    {
        object? value;
        if (contract.ExtensionDataIsJToken)
        {
            value = JToken.ReadFrom(reader);
        }
        else
        {
            value = CreateValueInternal(reader, null, null, null, contract, member, null);
        }
        return value;
    }

    void EndProcessProperty(object newObject, JsonReader reader, JsonObjectContract contract, int initialDepth, JsonProperty property, PropertyPresence presence, bool setDefaultValue)
    {
        if (presence is PropertyPresence.None or PropertyPresence.Null)
        {
            try
            {
                var resolvedRequired = property.Ignored ? Required.Default : property._required ?? contract.ItemRequired ?? Required.Default;

                switch (presence)
                {
                    case PropertyPresence.None:
                        if (resolvedRequired is Required.AllowNull or Required.Always)
                        {
                            throw JsonSerializationException.Create(reader, string.Format("Required property '{0}' not found in JSON.", property.PropertyName));
                        }

                        if (setDefaultValue && !property.Ignored)
                        {
                            if (property.PropertyContract == null)
                            {
                                property.PropertyContract = GetContractSafe(property.PropertyType);
                            }

                            if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate) && property.Writable)
                            {
                                property.ValueProvider!.SetValue(newObject, EnsureType(reader, property.GetResolvedDefaultValue(), CultureInfo.InvariantCulture, property.PropertyContract!, property.PropertyType));
                            }
                        }
                        break;
                    case PropertyPresence.Null:
                        if (resolvedRequired == Required.Always)
                        {
                            throw JsonSerializationException.Create(reader,string.Format( "Required property '{0}' expects a value but got null.", property.PropertyName));
                        }
                        if (resolvedRequired == Required.DisallowNull)
                        {
                            throw JsonSerializationException.Create(reader,string.Format( "Required property '{0}' expects a non-null value.", property.PropertyName));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (IsErrorHandled(newObject, contract, property.PropertyName, reader as IJsonLineInfo, reader.Path, ex))
                {
                    HandleError(reader, true, initialDepth);
                }
                else
                {
                    throw;
                }
            }
        }
    }

    void SetPropertyPresence(JsonReader reader, JsonProperty property, Dictionary<JsonProperty, PropertyPresence>? requiredProperties)
    {
        if (property != null && requiredProperties != null)
        {
            PropertyPresence propertyPresence;
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    propertyPresence = CoerceEmptyStringToNull(property.PropertyType, property.PropertyContract, (string)reader.Value!)
                        ? PropertyPresence.Null
                        : PropertyPresence.Value;
                    break;
                case JsonToken.Null:
                case JsonToken.Undefined:
                    propertyPresence = PropertyPresence.Null;
                    break;
                default:
                    propertyPresence = PropertyPresence.Value;
                    break;
            }

            requiredProperties[property] = propertyPresence;
        }
    }

    void HandleError(JsonReader reader, bool readPastError, int initialDepth)
    {
        ClearErrorContext();

        if (readPastError)
        {
            reader.Skip();

            while (reader.Depth > initialDepth)
            {
                if (!reader.Read())
                {
                    break;
                }
            }
        }
    }
}