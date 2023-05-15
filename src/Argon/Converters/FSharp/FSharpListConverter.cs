using Microsoft.FSharp.Collections;

namespace Argon;

/// <summary>
/// Converts a F# discriminated union type to and from JSON.
/// </summary>
public class FSharpListConverter : JsonConverter
{
    MethodInfo toFSharpList = typeof(FSharpListConverter).GetMethod("ToFSharpList")!;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        foreach (var item in (IEnumerable)value)
        {
            writer.WriteValue(item);
        }
        writer.WriteEndArray();
    }

    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        var genericArgument = type.GetGenericArguments()[0];
        return toFSharpList.MakeGenericMethod(genericArgument)
            .Invoke(
                null,
                new object[]
                {
                    reader,
                    serializer
                });
    }

    public static FSharpList<T> ToFSharpList<T>(JsonReader reader, JsonSerializer serializer)
    {
        var list = new List<T>();

        reader.Read();
        while (reader.TokenType != JsonToken.EndArray)
        {
            var item = serializer.Deserialize<T>(reader);

            list.Add(item);

            reader.Read();
        }

        return ListModule.OfSeq(list);
    }

    public override bool CanConvert(Type type) =>
        type.IsGenericType &&
        type.GetGenericTypeDefinition() == typeof(FSharpList<>);
}