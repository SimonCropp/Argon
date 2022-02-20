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

namespace Argon.Converters;

/// <summary>
/// Converts a F# discriminated union type to and from JSON.
/// </summary>
public class DiscriminatedUnionConverter : JsonConverter
{
    #region UnionDefinition
    internal class Union
    {
        public readonly FSharpFunction TagReader;
        public readonly List<UnionCase> Cases;

        public Union(FSharpFunction tagReader, List<UnionCase> cases)
        {
            TagReader = tagReader;
            Cases = cases;
        }
    }

    internal class UnionCase
    {
        public readonly int Tag;
        public readonly string Name;
        public readonly PropertyInfo[] Fields;
        public readonly FSharpFunction FieldReader;
        public readonly FSharpFunction Constructor;

        public UnionCase(int tag, string name, PropertyInfo[] fields, FSharpFunction fieldReader, FSharpFunction constructor)
        {
            Tag = tag;
            Name = name;
            Fields = fields;
            FieldReader = fieldReader;
            Constructor = constructor;
        }
    }
    #endregion

    const string CasePropertyName = "Case";
    const string FieldsPropertyName = "Fields";

    static readonly ThreadSafeStore<Type, Union> UnionCache = new(CreateUnion);
    static readonly ThreadSafeStore<Type, Type> UnionTypeLookupCache = new(CreateUnionTypeLookup);

    static Type CreateUnionTypeLookup(Type t)
    {
        // this lookup is because cases with fields are derived from union type
        // need to get declaring type to avoid duplicate Unions in cache

        // hacky but I can't find an API to get the declaring type without GetUnionCases
        var cases = (object[])FSharpUtils.Instance.GetUnionCases(null, t, null)!;

        var caseInfo = cases.First();

        var unionType = (Type)FSharpUtils.Instance.GetUnionCaseInfoDeclaringType(caseInfo)!;
        return unionType;
    }

    static Union CreateUnion(Type t)
    {
        var u = new Union((FSharpFunction)FSharpUtils.Instance.PreComputeUnionTagReader(null, t, null), new List<UnionCase>());

        var cases = (object[])FSharpUtils.Instance.GetUnionCases(null, t, null)!;

        foreach (var unionCaseInfo in cases)
        {
            var unionCase = new UnionCase(
                (int)FSharpUtils.Instance.GetUnionCaseInfoTag(unionCaseInfo),
                (string)FSharpUtils.Instance.GetUnionCaseInfoName(unionCaseInfo),
                (PropertyInfo[])FSharpUtils.Instance.GetUnionCaseInfoFields(unionCaseInfo)!,
                (FSharpFunction)FSharpUtils.Instance.PreComputeUnionReader(null, unionCaseInfo, null),
                (FSharpFunction)FSharpUtils.Instance.PreComputeUnionConstructor(null, unionCaseInfo, null));

            u.Cases.Add(unionCase);
        }

        return u;
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var resolver = serializer.ContractResolver as DefaultContractResolver;

        var unionType = UnionTypeLookupCache.Get(value.GetType());
        var union = UnionCache.Get(unionType);

        var tag = (int)union.TagReader.Invoke(value);
        var caseInfo = union.Cases.Single(c => c.Tag == tag);

        writer.WriteStartObject();
        writer.WritePropertyName(resolver != null ? resolver.GetResolvedPropertyName(CasePropertyName) : CasePropertyName);
        writer.WriteValue(caseInfo.Name);
        if (caseInfo.Fields is {Length: > 0})
        {
            var fields = (object[])caseInfo.FieldReader.Invoke(value)!;

            writer.WritePropertyName(resolver != null ? resolver.GetResolvedPropertyName(FieldsPropertyName) : FieldsPropertyName);
            writer.WriteStartArray();
            foreach (var field in fields)
            {
                serializer.Serialize(writer, field);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        UnionCase? caseInfo = null;
        string? caseName = null;
        JArray? fields = null;

        // start object
        reader.ReadAndAssert();

        while (reader.TokenType == JsonToken.PropertyName)
        {
            var propertyName = reader.Value!.ToString();
            if (string.Equals(propertyName, CasePropertyName, StringComparison.OrdinalIgnoreCase))
            {
                reader.ReadAndAssert();

                var union = UnionCache.Get(objectType);

                caseName = reader.Value!.ToString();

                caseInfo = union.Cases.SingleOrDefault(c => c.Name == caseName);

                if (caseInfo == null)
                {
                    throw JsonSerializationException.Create(reader, $"No union type found with the name '{caseName}'.");
                }
            }
            else if (string.Equals(propertyName, FieldsPropertyName, StringComparison.OrdinalIgnoreCase))
            {
                reader.ReadAndAssert();
                if (reader.TokenType != JsonToken.StartArray)
                {
                    throw JsonSerializationException.Create(reader, "Union fields must been an array.");
                }

                fields = (JArray)JToken.ReadFrom(reader);
            }
            else
            {
                throw JsonSerializationException.Create(reader, $"Unexpected property '{propertyName}' found when reading union.");
            }

            reader.ReadAndAssert();
        }

        if (caseInfo == null)
        {
            throw JsonSerializationException.Create(reader, $"No '{CasePropertyName}' property with union name found.");
        }

        var typedFieldValues = new object?[caseInfo.Fields.Length];

        if (caseInfo.Fields.Length > 0 && fields == null)
        {
            throw JsonSerializationException.Create(reader, $"No '{FieldsPropertyName}' property with union fields found.");
        }

        if (fields != null)
        {
            if (caseInfo.Fields.Length != fields.Count)
            {
                throw JsonSerializationException.Create(reader, $"The number of field values does not match the number of properties defined by union '{caseName}'.");
            }

            for (var i = 0; i < fields.Count; i++)
            {
                var t = fields[i];
                var fieldProperty = caseInfo.Fields[i];

                typedFieldValues[i] = t.ToObject(fieldProperty.PropertyType, serializer);
            }
        }

        object[] args = { typedFieldValues };

        return caseInfo.Constructor.Invoke(args);
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type objectType)
    {
        if (typeof(IEnumerable).IsAssignableFrom(objectType))
        {
            return false;
        }

        // all fsharp objects have CompilationMappingAttribute
        // get the fsharp assembly from the attribute and initialize latebound methods
        var attributes = objectType.GetCustomAttributes(true);

        var isFSharpType = false;
        foreach (var attribute in attributes)
        {
            var attributeType = attribute.GetType();
            if (attributeType.FullName == "Microsoft.FSharp.Core.CompilationMappingAttribute")
            {
                FSharpUtils.EnsureInitialized(attributeType.Assembly);

                isFSharpType = true;
                break;
            }
        }

        if (isFSharpType)
        {
            return (bool) FSharpUtils.Instance.IsUnion(null, objectType, null);
        }
        
        return false;
    }
}