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

class TraceJsonReader : JsonReader, IJsonLineInfo
{
    readonly JsonReader innerReader;
    readonly JsonTextWriter textWriter;
    readonly StringWriter stringWriter;

    public TraceJsonReader(JsonReader innerReader)
    {
        this.innerReader = innerReader;

        stringWriter = new StringWriter(CultureInfo.InvariantCulture);
        // prefix the message in the stringwriter to avoid concat with a potentially large JSON string
        stringWriter.Write($"Deserialized JSON: {Environment.NewLine}");

        textWriter = new JsonTextWriter(stringWriter);
        textWriter.Formatting = Formatting.Indented;
    }

    public string GetDeserializedJsonMessage()
    {
        return stringWriter.ToString();
    }

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

    public void WriteCurrentToken()
    {
        textWriter.WriteToken(innerReader, false, false, true);
    }

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

    public override void Close()
    {
        innerReader.Close();
    }

    bool IJsonLineInfo.HasLineInfo()
    {
        return innerReader is IJsonLineInfo lineInfo && lineInfo.HasLineInfo();
    }

    int IJsonLineInfo.LineNumber => innerReader is IJsonLineInfo lineInfo ? lineInfo.LineNumber : 0;

    int IJsonLineInfo.LinePosition => innerReader is IJsonLineInfo lineInfo ? lineInfo.LinePosition : 0;
}