// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Data;

namespace Argon.DataSetConverters;

/// <summary>
/// Converts a <see cref="DataSet"/> to and from JSON.
/// </summary>
public class DataSetConverter : JsonConverter
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dataSet = (DataSet)value;
        var resolver = serializer.ContractResolver as DefaultContractResolver;

        var converter = new DataTableConverter();

        writer.WriteStartObject();

        foreach (DataTable table in dataSet.Tables)
        {
            writer.WritePropertyName(resolver != null ? resolver.GetResolvedPropertyName(table.TableName) : table.TableName);

            converter.WriteJson(writer, table, serializer);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var set = GetDataSet(type);

        var converter = new DataTableConverter();

        reader.ReadAndAssert();

        while (reader.TokenType == JsonToken.PropertyName)
        {
            var table = set.Tables[(string)reader.Value!];
            var exists = table != null;

            table = (DataTable)converter.ReadJson(reader, typeof(DataTable), table, serializer)!;

            if (!exists)
            {
                set.Tables.Add(table);
            }

            reader.ReadAndAssert();
        }

        return set;
    }

    static DataSet GetDataSet(Type type)
    {
        // handle typed datasets
        if (type == typeof(DataSet))
        {
            return new();
        }

        return (DataSet) Activator.CreateInstance(type)!;
    }

    /// <summary>
    /// Determines whether this instance can convert the specified value type.
    /// </summary>
    /// <param name="valueType">Type of the value.</param>
    /// <returns>
    ///   <c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type valueType)
    {
        return typeof(DataSet).IsAssignableFrom(valueType);
    }
}