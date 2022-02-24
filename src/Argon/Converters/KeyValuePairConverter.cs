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
            if (!ReflectionUtils.IsNullableType(type))
            {
                throw JsonSerializationException.Create(reader, "Cannot convert null value to KeyValuePair.");
            }

            return null;
        }

        object? key = null;
        object? value = null;

        reader.ReadAndAssert();

        var t = ReflectionUtils.IsNullableType(type)
            ? Nullable.GetUnderlyingType(type)
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
        var t = ReflectionUtils.IsNullableType(type)
            ? Nullable.GetUnderlyingType(type)
            : type;

        if (t.IsValueType && t.IsGenericType)
        {
            return t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
        }

        return false;
    }
}