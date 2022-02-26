// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

class TraceJsonWriter : JsonWriter
{
    readonly JsonWriter innerWriter;
    readonly JsonTextWriter textWriter;
    readonly StringWriter stringWriter;

    public TraceJsonWriter(JsonWriter innerWriter)
    {
        this.innerWriter = innerWriter;

        stringWriter = new StringWriter(CultureInfo.InvariantCulture);
        // prefix the message in the stringwriter to avoid concat with a potentially large JSON string
        stringWriter.Write($"Serialized JSON: {Environment.NewLine}");

        textWriter = new JsonTextWriter(stringWriter);
        textWriter.Formatting = Formatting.Indented;
        textWriter.Culture = innerWriter.Culture;
        textWriter.DateFormatHandling = innerWriter.DateFormatHandling;
        textWriter.DateFormatString = innerWriter.DateFormatString;
        textWriter.DateTimeZoneHandling = innerWriter.DateTimeZoneHandling;
        textWriter.FloatFormatHandling = innerWriter.FloatFormatHandling;
    }

    public string GetSerializedJsonMessage()
    {
        return stringWriter.ToString();
    }

    public override void WriteValue(decimal value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(decimal? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(bool value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(bool? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(byte value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(byte? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(char value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(char? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(byte[]? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value == null)
        {
            base.WriteUndefined();
        }
        else
        {
            base.WriteValue(value);
        }
    }

    public override void WriteValue(DateTime value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(DateTime? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(DateTimeOffset value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(DateTimeOffset? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(double value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(double? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteUndefined()
    {
        textWriter.WriteUndefined();
        innerWriter.WriteUndefined();
        base.WriteUndefined();
    }

    public override void WriteNull()
    {
        textWriter.WriteNull();
        innerWriter.WriteNull();
        base.WriteUndefined();
    }

    public override void WriteValue(float value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(float? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(Guid value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(Guid? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(int value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(int? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(long value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(long? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(object? value)
    {
        if (value is BigInteger)
        {
            textWriter.WriteValue(value);
            innerWriter.WriteValue(value);
            InternalWriteValue(JsonToken.Integer);
        }
        else
        {
            textWriter.WriteValue(value);
            innerWriter.WriteValue(value);
            if (value == null)
            {
                base.WriteUndefined();
            }
            else
            {
                // base.WriteValue(value) will error
                InternalWriteValue(JsonToken.String);
            }
        }
    }

    public override void WriteValue(sbyte value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(sbyte? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(short value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(short? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(string? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(TimeSpan value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(TimeSpan? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(uint value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(uint? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(ulong value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(ulong? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteValue(Uri? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value == null)
        {
            base.WriteUndefined();
        }
        else
        {
            base.WriteValue(value);
        }
    }

    public override void WriteValue(ushort value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        base.WriteValue(value);
    }

    public override void WriteValue(ushort? value)
    {
        textWriter.WriteValue(value);
        innerWriter.WriteValue(value);
        if (value.HasValue)
        {
            base.WriteValue(value.GetValueOrDefault());
        }
        else
        {
            base.WriteUndefined();
        }
    }

    public override void WriteWhitespace(string ws)
    {
        textWriter.WriteWhitespace(ws);
        innerWriter.WriteWhitespace(ws);
        base.WriteWhitespace(ws);
    }

    public override void WriteComment(string? text)
    {
        textWriter.WriteComment(text);
        innerWriter.WriteComment(text);
        base.WriteComment(text);
    }

    public override void WriteStartArray()
    {
        textWriter.WriteStartArray();
        innerWriter.WriteStartArray();
        base.WriteStartArray();
    }

    public override void WriteEndArray()
    {
        textWriter.WriteEndArray();
        innerWriter.WriteEndArray();
        base.WriteEndArray();
    }

    public override void WriteStartConstructor(string name)
    {
        textWriter.WriteStartConstructor(name);
        innerWriter.WriteStartConstructor(name);
        base.WriteStartConstructor(name);
    }

    public override void WriteEndConstructor()
    {
        textWriter.WriteEndConstructor();
        innerWriter.WriteEndConstructor();
        base.WriteEndConstructor();
    }

    public override void WritePropertyName(string name)
    {
        textWriter.WritePropertyName(name);
        innerWriter.WritePropertyName(name);
        base.WritePropertyName(name);
    }

    public override void WritePropertyName(string name, bool escape)
    {
        textWriter.WritePropertyName(name, escape);
        innerWriter.WritePropertyName(name, escape);

        // method with escape will error
        base.WritePropertyName(name);
    }

    public override void WriteStartObject()
    {
        textWriter.WriteStartObject();
        innerWriter.WriteStartObject();
        base.WriteStartObject();
    }

    public override void WriteEndObject()
    {
        textWriter.WriteEndObject();
        innerWriter.WriteEndObject();
        base.WriteEndObject();
    }

    public override void WriteRawValue(string? json)
    {
        textWriter.WriteRawValue(json);
        innerWriter.WriteRawValue(json);

        // calling base method will write json twice
        InternalWriteValue(JsonToken.Undefined);
    }

    public override void WriteRaw(string? json)
    {
        textWriter.WriteRaw(json);
        innerWriter.WriteRaw(json);
        base.WriteRaw(json);
    }

    public override void Close()
    {
        textWriter.Close();
        innerWriter.Close();
        base.Close();
    }

    public override void Flush()
    {
        textWriter.Flush();
        innerWriter.Flush();
    }
}