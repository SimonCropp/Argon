// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Data;

namespace Argon.DataSetConverters;

/// <summary>
/// Converts a <see cref="DataTable" /> to and from JSON.
/// </summary>
public class DataTableConverter : JsonConverter
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var table = (DataTable) value;
        var resolver = serializer.ContractResolver as DefaultContractResolver;

        writer.WriteStartArray();

        foreach (DataRow row in table.Rows)
        {
            writer.WriteStartObject();
            foreach (DataColumn column in row.Table.Columns)
            {
                var columnValue = row[column];

                if (serializer.NullValueHandling == NullValueHandling.Ignore && (columnValue == null || columnValue == DBNull.Value))
                {
                    continue;
                }

                writer.WritePropertyName(resolver != null ? resolver.GetResolvedPropertyName(column.ColumnName) : column.ColumnName);
                serializer.Serialize(writer, columnValue);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
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

        if (existingValue is not DataTable table)
        {
            // handle typed datasets
            if (type == typeof(DataTable))
            {
                table = new();
            }
            else
            {
                table = (DataTable) Activator.CreateInstance(type)!;
            }
        }

        // DataTable is inside a DataSet
        // populate the name from the property name
        if (reader.TokenType == JsonToken.PropertyName)
        {
            table.TableName = reader.StringValue;

            reader.ReadAndAssert();

            if (reader.TokenType == JsonToken.Null)
            {
                return table;
            }
        }

        if (reader.TokenType != JsonToken.StartArray)
        {
            throw JsonSerializationException.Create(reader, $"Unexpected JSON token when reading DataTable. Expected StartArray, got {reader.TokenType}.");
        }

        reader.ReadAndAssert();

        while (reader.TokenType != JsonToken.EndArray)
        {
            CreateRow(reader, table, serializer);

            reader.ReadAndAssert();
        }

        return table;
    }

    static void CreateRow(JsonReader reader, DataTable table, JsonSerializer serializer)
    {
        var row = table.NewRow();
        reader.ReadAndAssert();

        while (reader.TokenType == JsonToken.PropertyName)
        {
            var columnName = reader.StringValue;

            reader.ReadAndAssert();

            var column = GetColumn(reader, table, columnName);

            if (column.DataType == typeof(DataTable))
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    reader.ReadAndAssert();
                }

                var nestedTable = new DataTable();

                while (reader.TokenType != JsonToken.EndArray)
                {
                    CreateRow(reader, nestedTable, serializer);

                    reader.ReadAndAssert();
                }

                row[columnName] = nestedTable;
            }
            else if (column.DataType.IsArray &&
                     column.DataType != typeof(byte[]))
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    reader.ReadAndAssert();
                }

                var list = new List<object?>();

                while (reader.TokenType != JsonToken.EndArray)
                {
                    list.Add(reader.Value);
                    reader.ReadAndAssert();
                }

                var destinationArray = Array.CreateInstance(column.DataType.GetElementType()!, list.Count);
                ((IList) list).CopyTo(destinationArray, 0);

                row[columnName] = destinationArray;
            }
            else
            {
                var columnValue = GetColumnValue(reader, serializer, column);

                row[columnName] = columnValue;
            }

            reader.ReadAndAssert();
        }

        row.EndEdit();
        table.Rows.Add(row);
    }

    static object GetColumnValue(JsonReader reader, JsonSerializer serializer, DataColumn column)
    {
        if (reader.Value == null)
        {
            return DBNull.Value;
        }

        return serializer.Deserialize(reader, column.DataType) ?? DBNull.Value;
    }

    static DataColumn GetColumn(JsonReader reader, DataTable table, string name)
    {
        var column = table.Columns[name];
        if (column != null)
        {
            return column;
        }

        var columnType = GetColumnDataType(reader);
        column = new(name, columnType);
        table.Columns.Add(column);

        return column;
    }

    static Type GetColumnDataType(JsonReader reader)
    {
        var tokenType = reader.TokenType;

        switch (tokenType)
        {
            case JsonToken.Integer:
            case JsonToken.Boolean:
            case JsonToken.Float:
            case JsonToken.String:
            case JsonToken.Date:
            case JsonToken.Bytes:
                return reader.ValueType!;
            case JsonToken.Null:
            case JsonToken.Undefined:
            case JsonToken.EndArray:
                return typeof(string);
            case JsonToken.StartArray:
                reader.ReadAndAssert();
                if (reader.TokenType == JsonToken.StartObject)
                {
                    // nested datatable
                    return typeof(DataTable);
                }

                var arrayType = GetColumnDataType(reader);
                return arrayType.MakeArrayType();
            default:
                throw JsonSerializationException.Create(reader, $"Unexpected JSON token when reading DataTable: {tokenType}");
        }
    }

    /// <summary>
    /// Determines whether this instance can convert the specified value type.
    /// </summary>
    /// <param name="valueType">Type of the value.</param>
    /// <returns>
    /// <c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type valueType)
    {
        return typeof(DataTable).IsAssignableFrom(valueType);
    }
}