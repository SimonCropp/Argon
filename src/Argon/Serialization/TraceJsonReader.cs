// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

class TraceJsonReader : JsonReader, IJsonLineInfo
{
    readonly JsonReader innerReader;
    readonly JsonTextWriter textWriter;
    readonly StringWriter stringWriter;

    public TraceJsonReader(JsonReader innerReader)
    {
        this.innerReader = innerReader;

        stringWriter = new(CultureInfo.InvariantCulture);
        // prefix the message in the stringwriter to avoid concat with a potentially large JSON string
        stringWriter.Write($"Deserialized JSON: {Environment.NewLine}");

        textWriter = new(stringWriter);
        textWriter.Formatting = Formatting.Indented;
    }

    public string GetDeserializedJsonMessage() =>
        stringWriter.ToString();

    public override bool Read()
    {
        var value = innerReader.Read();
        WriteCurrentToken();
        return value;
    }

    public override int? ReadAsInt32()
    {
        var value = innerReader.ReadAsInt32();
        WriteCurrentToken();
        return value;
    }

    public override string? ReadAsString()
    {
        var value = innerReader.ReadAsString();
        WriteCurrentToken();
        return value;
    }

    public override byte[]? ReadAsBytes()
    {
        var value = innerReader.ReadAsBytes();
        WriteCurrentToken();
        return value;
    }

    public override decimal? ReadAsDecimal()
    {
        var value = innerReader.ReadAsDecimal();
        WriteCurrentToken();
        return value;
    }

    public override double? ReadAsDouble()
    {
        var value = innerReader.ReadAsDouble();
        WriteCurrentToken();
        return value;
    }

    public override bool? ReadAsBoolean()
    {
        var value = innerReader.ReadAsBoolean();
        WriteCurrentToken();
        return value;
    }

    public override DateTime? ReadAsDateTime()
    {
        var value = innerReader.ReadAsDateTime();
        WriteCurrentToken();
        return value;
    }

    public override DateTimeOffset? ReadAsDateTimeOffset()
    {
        var value = innerReader.ReadAsDateTimeOffset();
        WriteCurrentToken();
        return value;
    }

    public void WriteCurrentToken() =>
        textWriter.WriteToken(innerReader, false, false, true);

    public override int Depth => innerReader.Depth;

    public override string Path => innerReader.Path;

    public override char QuoteChar
    {
        get => innerReader.QuoteChar;
        protected internal set => innerReader.QuoteChar = value;
    }

    public override JsonToken TokenType => innerReader.TokenType;

    public override object? Value => innerReader.Value;

    public override Type? ValueType => innerReader.ValueType;

    public override void Close() =>
        innerReader.Close();

    bool IJsonLineInfo.HasLineInfo() =>
        innerReader is IJsonLineInfo lineInfo && lineInfo.HasLineInfo();

    int IJsonLineInfo.LineNumber => innerReader is IJsonLineInfo lineInfo ? lineInfo.LineNumber : 0;

    int IJsonLineInfo.LinePosition => innerReader is IJsonLineInfo lineInfo ? lineInfo.LinePosition : 0;
}