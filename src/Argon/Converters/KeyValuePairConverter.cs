// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Converts a <see cref="KeyValuePair{TKey,TValue}"/> to and from JSON.
/// </summary>
public class KeyValuePairConverter : JsonConverter
{
    const string keyName = "Key";
    const string valueName = "Value";

    static readonly ThreadSafeStore<Type, ReflectionObject> reflectionObjectPerType = new(InitializeReflectionObject);

    static ReflectionObject InitializeReflectionObject(Type type)
    {
        var genericArguments = type.GetGenericArguments();
        var keyType = genericArguments[0];
        var valueType = genericArguments[1];

        return ReflectionObject.Create(type, type.GetConstructor(new[] { keyType, valueType }), keyName, valueName);
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var reflectionObject = reflectionObjectPerType.Get(value.GetType());

        var resolver = serializer.ContractResolver as DefaultContractResolver;

        writer.WriteStartObject();
        writer.WritePropertyName(resolver != null ? resolver.GetResolvedPropertyName(keyName) : keyName);
        serializer.Serialize(writer, reflectionObject.GetValue(value, keyName), reflectionObject.GetType(keyName));
        writer.WritePropertyName(resolver != null ? resolver.GetResolvedPropertyName(valueName) : valueName);
        serializer.Serialize(writer, reflectionObject.GetValue(value, valueName), reflectionObject.GetType(valueName));
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            if (!type.IsNullableType())
            {
                throw JsonSerializationException.Create(reader, "Cannot convert null value to KeyValuePair.");
            }

            return null;
        }

        object? key = null;
        object? value = null;

        reader.ReadAndAssert();

        var t = type.IsNullableType()
            ? Nullable.GetUnderlyingType(type)!
            : type;

        var reflectionObject = reflectionObjectPerType.Get(t);
        var keyContract = serializer.ResolveContract(reflectionObject.GetType(keyName));
        var valueContract = serializer.ResolveContract(reflectionObject.GetType(valueName));

        while (reader.TokenType == JsonToken.PropertyName)
        {
            var propertyName = reader.Value!.ToString();
            if (string.Equals(propertyName, keyName, StringComparison.OrdinalIgnoreCase))
            {
                reader.ReadForTypeAndAssert(keyContract, false);

                key = serializer.Deserialize(reader, keyContract.UnderlyingType);
            }
            else if (string.Equals(propertyName, valueName, StringComparison.OrdinalIgnoreCase))
            {
                reader.ReadForTypeAndAssert(valueContract, false);

                value = serializer.Deserialize(reader, valueContract.UnderlyingType);
            }
            else
            {
                reader.Skip();
            }

            reader.ReadAndAssert();
        }

        return reflectionObject.Creator!(key, value);
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type type)
    {
        var t = type.IsNullableType()
            ? Nullable.GetUnderlyingType(type)!
            : type;

        if (t.IsValueType && t.IsGenericType)
        {
            return t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
        }

        return false;
    }
}