// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
/// </summary>
public partial class JsonTextWriter : JsonWriter
{
    const int indentCharBufferSize = 12;
    readonly TextWriter writer;
    Base64Encoder? base64Encoder;
    char indentChar;
    int indentation;
    char quoteChar;
    bool[]? charEscapeFlags;
    char[]? writeBuffer;
    IArrayPool<char>? arrayPool;
    char[]? indentChars;

    Base64Encoder Base64Encoder => base64Encoder ??= new Base64Encoder(writer);

    /// <summary>
    /// Gets or sets the writer's character array pool.
    /// </summary>
    public IArrayPool<char>? ArrayPool
    {
        get => arrayPool;
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            arrayPool = value;
        }
    }

    /// <summary>
    /// Gets or sets how many <see cref="JsonTextWriter.IndentChar"/>s to write for each level in the hierarchy when <see cref="JsonWriter.Formatting"/> is set to <see cref="Formatting.Indented"/>.
    /// </summary>
    public int Indentation
    {
        get => indentation;
        set
        {
            if (value < 0)
            {
                throw new ArgumentException("Indentation value must be greater than 0.");
            }

            indentation = value;
        }
    }

    /// <summary>
    /// Gets or sets which character to use to quote attribute values.
    /// </summary>
    public char QuoteChar
    {
        get => quoteChar;
        set
        {
            if (value != '"' && value != '\'')
            {
                throw new ArgumentException(@"Invalid JavaScript string quote character. Valid quote characters are ' and "".");
            }

            quoteChar = value;
            UpdateCharEscapeFlags();
        }
    }

    /// <summary>
    /// Gets or sets which character to use for indenting when <see cref="JsonWriter.Formatting"/> is set to <see cref="Formatting.Indented"/>.
    /// </summary>
    public char IndentChar
    {
        get => indentChar;
        set
        {
            if (value != indentChar)
            {
                indentChar = value;
                indentChars = null;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether object names will be surrounded with quotes.
    /// </summary>
    public bool QuoteName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonTextWriter"/> class using the specified <see cref="TextWriter"/>.
    /// </summary>
    public JsonTextWriter(TextWriter textWriter)
    {
        writer = textWriter;
        quoteChar = '"';
        QuoteName = true;
        indentChar = ' ';
        indentation = 2;

        UpdateCharEscapeFlags();

        safeAsync = GetType() == typeof(JsonTextWriter);
    }

    /// <summary>
    /// Flushes whatever is in the buffer to the underlying <see cref="TextWriter"/> and also flushes the underlying <see cref="TextWriter"/>.
    /// </summary>
    public override void Flush()
    {
        writer.Flush();
    }

    /// <summary>
    /// Closes this writer.
    /// If <see cref="JsonWriter.CloseOutput"/> is set to <c>true</c>, the underlying <see cref="TextWriter"/> is also closed.
    /// If <see cref="JsonWriter.AutoCompleteOnClose"/> is set to <c>true</c>, the JSON is auto-completed.
    /// </summary>
    public override void Close()
    {
        base.Close();

        CloseBufferAndWriter();
    }

    void CloseBufferAndWriter()
    {
        if (writeBuffer != null)
        {
            BufferUtils.ReturnBuffer(arrayPool, writeBuffer);
            writeBuffer = null;
        }

        if (CloseOutput)
        {
            writer.Close();
        }
    }

    /// <summary>
    /// Writes the beginning of a JSON object.
    /// </summary>
    public override void WriteStartObject()
    {
        InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);

        writer.Write('{');
    }

    /// <summary>
    /// Writes the beginning of a JSON array.
    /// </summary>
    public override void WriteStartArray()
    {
        InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);

        writer.Write('[');
    }

    /// <summary>
    /// Writes the start of a constructor with the given name.
    /// </summary>
    public override void WriteStartConstructor(string name)
    {
        InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);

        writer.Write("new ");
        writer.Write(name);
        writer.Write('(');
    }

    /// <summary>
    /// Writes the specified end token.
    /// </summary>
    protected override void WriteEnd(JsonToken token)
    {
        switch (token)
        {
            case JsonToken.EndObject:
                writer.Write('}');
                break;
            case JsonToken.EndArray:
                writer.Write(']');
                break;
            case JsonToken.EndConstructor:
                writer.Write(')');
                break;
            default:
                throw JsonWriterException.Create(this, $"Invalid JsonToken: {token}", null);
        }
    }

    /// <summary>
    /// Writes the property name of a name/value pair on a JSON object.
    /// </summary>
    public override void WritePropertyName(string name)
    {
        InternalWritePropertyName(name);

        WriteEscapedString(name, QuoteName);

        writer.Write(':');
    }

    /// <summary>
    /// Writes the property name of a name/value pair on a JSON object.
    /// </summary>
    /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
    public override void WritePropertyName(string name, bool escape)
    {
        InternalWritePropertyName(name);

        if (escape)
        {
            WriteEscapedString(name, QuoteName);
        }
        else
        {
            if (QuoteName)
            {
                writer.Write(quoteChar);
            }

            writer.Write(name);

            if (QuoteName)
            {
                writer.Write(quoteChar);
            }
        }

        writer.Write(':');
    }

    internal override void OnStringEscapeHandlingChanged()
    {
        UpdateCharEscapeFlags();
    }

    void UpdateCharEscapeFlags()
    {
        charEscapeFlags = JavaScriptUtils.GetCharEscapeFlags(StringEscapeHandling, quoteChar);
    }

    /// <summary>
    /// Writes indent characters.
    /// </summary>
    protected override void WriteIndent()
    {
        // levels of indentation multiplied by the indent count
        var currentIndentCount = Top * indentation;

        var newLineLen = SetIndentChars();

        writer.Write(indentChars!, 0, newLineLen + Math.Min(currentIndentCount, indentCharBufferSize));

        while ((currentIndentCount -= indentCharBufferSize) > 0)
        {
            writer.Write(indentChars!, newLineLen, Math.Min(currentIndentCount, indentCharBufferSize));
        }
    }

    int SetIndentChars()
    {
        // Set _indentChars to be a newline followed by IndentCharBufferSize indent characters.
        var writerNewLine = writer.NewLine;
        var newLineLen = writerNewLine.Length;
        var match = indentChars != null && indentChars.Length == indentCharBufferSize + newLineLen;
        if (match)
        {
            for (var i = 0; i != newLineLen; ++i)
            {
                if (writerNewLine[i] != indentChars![i])
                {
                    match = false;
                    break;
                }
            }
        }

        if (!match)
        {
            // If we're here, either _indentChars hasn't been set yet, or _writer.NewLine
            // has been changed, or _indentChar has been changed.
            indentChars = (writerNewLine + new string(indentChar, indentCharBufferSize)).ToCharArray();
        }

        return newLineLen;
    }

    /// <summary>
    /// Writes the JSON value delimiter.
    /// </summary>
    protected override void WriteValueDelimiter()
    {
        writer.Write(',');
    }

    /// <summary>
    /// Writes an indent space.
    /// </summary>
    protected override void WriteIndentSpace()
    {
        writer.Write(' ');
    }

    void WriteValueInternal(string value, JsonToken token)
    {
        writer.Write(value);
    }

    #region WriteValue methods
    /// <summary>
    /// Writes a <see cref="Object"/> value.
    /// An error will raised if the value cannot be written as a single JSON token.
    /// </summary>
    public override void WriteValue(object? value)
    {
        if (value is BigInteger i)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteValueInternal(i.ToString(CultureInfo.InvariantCulture), JsonToken.String);
        }
        else
        {
            base.WriteValue(value);
        }
    }

    /// <summary>
    /// Writes a null value.
    /// </summary>
    public override void WriteNull()
    {
        InternalWriteValue(JsonToken.Null);
        WriteValueInternal(JsonConvert.Null, JsonToken.Null);
    }

    /// <summary>
    /// Writes an undefined value.
    /// </summary>
    public override void WriteUndefined()
    {
        InternalWriteValue(JsonToken.Undefined);
        WriteValueInternal(JsonConvert.Undefined, JsonToken.Undefined);
    }

    /// <summary>
    /// Writes raw JSON.
    /// </summary>
    public override void WriteRaw(string? json)
    {
        InternalWriteRaw();

        writer.Write(json);
    }

    /// <summary>
    /// Writes a <see cref="String"/> value.
    /// </summary>
    public override void WriteValue(string? value)
    {
        InternalWriteValue(JsonToken.String);

        if (value == null)
        {
            WriteValueInternal(JsonConvert.Null, JsonToken.Null);
        }
        else
        {
            WriteEscapedString(value, true);
        }
    }

    void WriteEscapedString(string value, bool quote)
    {
        EnsureWriteBuffer();
        JavaScriptUtils.WriteEscapedJavaScriptString(writer, value, quoteChar, quote, charEscapeFlags!, StringEscapeHandling, arrayPool, ref writeBuffer);
    }

    /// <summary>
    /// Writes a <see cref="Int32"/> value.
    /// </summary>
    public override void WriteValue(int value)
    {
        InternalWriteValue(JsonToken.Integer);
        WriteIntegerValue(value);
    }

    /// <summary>
    /// Writes a <see cref="UInt32"/> value.
    /// </summary>
    public override void WriteValue(uint value)
    {
        InternalWriteValue(JsonToken.Integer);
        WriteIntegerValue(value);
    }

    /// <summary>
    /// Writes a <see cref="Int64"/> value.
    /// </summary>
    public override void WriteValue(long value)
    {
        InternalWriteValue(JsonToken.Integer);
        WriteIntegerValue(value);
    }

    /// <summary>
    /// Writes a <see cref="UInt64"/> value.
    /// </summary>
    public override void WriteValue(ulong value)
    {
        InternalWriteValue(JsonToken.Integer);
        WriteIntegerValue(value, false);
    }

    /// <summary>
    /// Writes a <see cref="Single"/> value.
    /// </summary>
    public override void WriteValue(float value)
    {
        InternalWriteValue(JsonToken.Float);
        WriteValueInternal(JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, false), JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}"/> of <see cref="Single"/> value.
    /// </summary>
    public override void WriteValue(float? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(JsonConvert.ToString(value.GetValueOrDefault(), FloatFormatHandling, QuoteChar, true), JsonToken.Float);
        }
    }

    /// <summary>
    /// Writes a <see cref="Double"/> value.
    /// </summary>
    public override void WriteValue(double value)
    {
        InternalWriteValue(JsonToken.Float);
        WriteValueInternal(JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, false), JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}"/> of <see cref="Double"/> value.
    /// </summary>
    public override void WriteValue(double? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(JsonConvert.ToString(value.GetValueOrDefault(), FloatFormatHandling, QuoteChar, true), JsonToken.Float);
        }
    }

    /// <summary>
    /// Writes a <see cref="Boolean"/> value.
    /// </summary>
    public override void WriteValue(bool value)
    {
        InternalWriteValue(JsonToken.Boolean);
        WriteValueInternal(JsonConvert.ToString(value), JsonToken.Boolean);
    }

    /// <summary>
    /// Writes a <see cref="Int16"/> value.
    /// </summary>
    public override void WriteValue(short value)
    {
        InternalWriteValue(JsonToken.Integer);
        WriteIntegerValue(value);
    }

    /// <summary>
    /// Writes a <see cref="UInt16"/> value.
    /// </summary>
    public override void WriteValue(ushort value)
    {
        InternalWriteValue(JsonToken.Integer);
        WriteIntegerValue(value);
    }

    /// <summary>
    /// Writes a <see cref="Char"/> value.
    /// </summary>
    public override void WriteValue(char value)
    {
        InternalWriteValue(JsonToken.String);
        WriteValueInternal(JsonConvert.ToString(value), JsonToken.String);
    }

    /// <summary>
    /// Writes a <see cref="Byte"/> value.
    /// </summary>
    public override void WriteValue(byte value)
    {
        InternalWriteValue(JsonToken.Integer);
        WriteIntegerValue(value);
    }

    /// <summary>
    /// Writes a <see cref="SByte"/> value.
    /// </summary>
    public override void WriteValue(sbyte value)
    {
        InternalWriteValue(JsonToken.Integer);
        WriteIntegerValue(value);
    }

    /// <summary>
    /// Writes a <see cref="Decimal"/> value.
    /// </summary>
    public override void WriteValue(decimal value)
    {
        InternalWriteValue(JsonToken.Float);
        WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> value.
    /// </summary>
    public override void WriteValue(DateTime value)
    {
        InternalWriteValue(JsonToken.Date);
        value = DateTimeUtils.EnsureDateTime(value, DateTimeZoneHandling);

        if (StringUtils.IsNullOrEmpty(DateFormatString))
        {
            var length = WriteValueToBuffer(value);

            writer.Write(writeBuffer!, 0, length);
        }
        else
        {
            writer.Write(quoteChar);
            writer.Write(value.ToString(DateFormatString, Culture));
            writer.Write(quoteChar);
        }
    }

    int WriteValueToBuffer(DateTime value)
    {
        EnsureWriteBuffer();
        MiscellaneousUtils.Assert(writeBuffer != null);

        var pos = 0;
        writeBuffer[pos++] = quoteChar;
        pos = DateTimeUtils.WriteDateTimeString(writeBuffer, pos, value, null, value.Kind);
        writeBuffer[pos++] = quoteChar;
        return pos;
    }

    /// <summary>
    /// Writes a <see cref="Byte"/>[] value.
    /// </summary>
    public override void WriteValue(byte[]? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            InternalWriteValue(JsonToken.Bytes);
            writer.Write(quoteChar);
            Base64Encoder.Encode(value, 0, value.Length);
            Base64Encoder.Flush();
            writer.Write(quoteChar);
        }
    }

    /// <summary>
    /// Writes a <see cref="DateTimeOffset"/> value.
    /// </summary>
    public override void WriteValue(DateTimeOffset value)
    {
        InternalWriteValue(JsonToken.Date);

        if (StringUtils.IsNullOrEmpty(DateFormatString))
        {
            var length = WriteValueToBuffer(value);

            writer.Write(writeBuffer!, 0, length);
        }
        else
        {
            writer.Write(quoteChar);
            writer.Write(value.ToString(DateFormatString, Culture));
            writer.Write(quoteChar);
        }
    }

    int WriteValueToBuffer(DateTimeOffset value)
    {
        EnsureWriteBuffer();
        MiscellaneousUtils.Assert(writeBuffer != null);

        var pos = 0;
        writeBuffer[pos++] = quoteChar;
        pos = DateTimeUtils.WriteDateTimeString(writeBuffer, pos, value.DateTime, value.Offset, DateTimeKind.Local);
        writeBuffer[pos++] = quoteChar;
        return pos;
    }

    /// <summary>
    /// Writes a <see cref="Guid"/> value.
    /// </summary>
    public override void WriteValue(Guid value)
    {
        InternalWriteValue(JsonToken.String);

        var text = value.ToString("D", CultureInfo.InvariantCulture);

        writer.Write(quoteChar);
        writer.Write(text);
        writer.Write(quoteChar);
    }

    /// <summary>
    /// Writes a <see cref="TimeSpan"/> value.
    /// </summary>
    public override void WriteValue(TimeSpan value)
    {
        InternalWriteValue(JsonToken.String);

        var text = value.ToString(null, CultureInfo.InvariantCulture);

        writer.Write(quoteChar);
        writer.Write(text);
        writer.Write(quoteChar);
    }

    /// <summary>
    /// Writes a <see cref="Uri"/> value.
    /// </summary>
    public override void WriteValue(Uri? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            InternalWriteValue(JsonToken.String);
            WriteEscapedString(value.OriginalString, true);
        }
    }
    #endregion

    /// <summary>
    /// Writes a comment <c>/*...*/</c> containing the specified text.
    /// </summary>
    public override void WriteComment(string? text)
    {
        InternalWriteComment();

        writer.Write("/*");
        writer.Write(text);
        writer.Write("*/");
    }

    /// <summary>
    /// Writes the given white space.
    /// </summary>
    public override void WriteWhitespace(string ws)
    {
        InternalWriteWhitespace(ws);

        writer.Write(ws);
    }

    void EnsureWriteBuffer()
    {
        // maximum buffer sized used when writing iso date
        writeBuffer ??= BufferUtils.RentBuffer(arrayPool, 35);
    }

    void WriteIntegerValue(long value)
    {
        if (value is >= 0 and <= 9)
        {
            writer.Write((char)('0' + value));
        }
        else
        {
            var negative = value < 0;
            WriteIntegerValue(negative ? (ulong)-value : (ulong)value, negative);
        }
    }

    void WriteIntegerValue(ulong value, bool negative)
    {
        if (!negative & value <= 9)
        {
            writer.Write((char)('0' + value));
        }
        else
        {
            var length = WriteNumberToBuffer(value, negative);
            writer.Write(writeBuffer!, 0, length);
        }
    }

    int WriteNumberToBuffer(ulong value, bool negative)
    {
        if (value <= uint.MaxValue)
        {
            // avoid the 64 bit division if possible
            return WriteNumberToBuffer((uint)value, negative);
        }

        EnsureWriteBuffer();
        MiscellaneousUtils.Assert(writeBuffer != null);

        var totalLength = MathUtils.IntLength(value);

        if (negative)
        {
            totalLength++;
            writeBuffer[0] = '-';
        }

        var index = totalLength;

        do
        {
            var quotient = value / 10;
            var digit = value - quotient * 10;
            writeBuffer[--index] = (char)('0' + digit);
            value = quotient;
        } while (value != 0);

        return totalLength;
    }

    void WriteIntegerValue(int value)
    {
        if (value is >= 0 and <= 9)
        {
            writer.Write((char)('0' + value));
        }
        else
        {
            var negative = value < 0;
            WriteIntegerValue(negative ? (uint)-value : (uint)value, negative);
        }
    }

    void WriteIntegerValue(uint value, bool negative)
    {
        if (!negative & value <= 9)
        {
            writer.Write((char)('0' + value));
        }
        else
        {
            var length = WriteNumberToBuffer(value, negative);
            writer.Write(writeBuffer!, 0, length);
        }
    }

    int WriteNumberToBuffer(uint value, bool negative)
    {
        EnsureWriteBuffer();
        MiscellaneousUtils.Assert(writeBuffer != null);

        var totalLength = MathUtils.IntLength(value);

        if (negative)
        {
            totalLength++;
            writeBuffer[0] = '-';
        }

        var index = totalLength;

        do
        {
            var quotient = value / 10;
            var digit = value - quotient * 10;
            writeBuffer[--index] = (char)('0' + digit);
            value = quotient;
        } while (value != 0);

        return totalLength;
    }
}