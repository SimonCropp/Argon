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

enum ReadType
{
    Read,
    ReadAsInt32,
    ReadAsInt64,
    ReadAsBytes,
    ReadAsString,
    ReadAsDecimal,
    ReadAsDateTime,
    ReadAsDateTimeOffset,
    ReadAsDouble,
    ReadAsBoolean
}

/// <summary>
/// Represents a reader that provides fast, non-cached, forward-only access to JSON text data.
/// </summary>
public partial class JsonTextReader : JsonReader, IJsonLineInfo
{
    const char unicodeReplacementChar = '\uFFFD';
    const int maximumJavascriptIntegerCharacterLength = 380;
#if DEBUG
    internal int LargeBufferLength { get; set; } = int.MaxValue / 2;
#else
    const int LargeBufferLength = int.MaxValue / 2;
#endif

    readonly TextReader reader;
    int charsUsed;
    int lineStartPos;
    int lineNumber;
    bool isEndOfFile;
    StringBuffer stringBuffer;
    StringReference stringReference;
    IArrayPool<char>? arrayPool;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonTextReader"/> class with the specified <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> containing the JSON data to read.</param>
    public JsonTextReader(TextReader reader)
    {
        this.reader = reader;
        lineNumber = 1;

        safeAsync = GetType() == typeof(JsonTextReader);
    }

    internal char[]? CharBuffer { get; set; }

    internal int CharPos { get; private set; }

    /// <summary>
    /// Gets or sets the reader's property name table.
    /// </summary>
    public JsonNameTable? PropertyNameTable { get; set; }

    /// <summary>
    /// Gets or sets the reader's character buffer pool.
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

    void EnsureBufferNotEmpty()
    {
        if (stringBuffer.IsEmpty)
        {
            stringBuffer = new StringBuffer(arrayPool, 1024);
        }
    }

    void SetNewLine(bool hasNextChar)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (hasNextChar && CharBuffer[CharPos] == StringUtils.LineFeed)
        {
            CharPos++;
        }

        OnNewLine(CharPos);
    }

    void OnNewLine(int pos)
    {
        lineNumber++;
        lineStartPos = pos;
    }

    void ParseString(char quote, ReadType readType)
    {
        CharPos++;

        ShiftBufferIfNeeded();
        ReadStringIntoBuffer(quote);
        ParseReadString(quote, readType);
    }

    void ParseReadString(char quote, ReadType readType)
    {
        SetPostValueState(true);

        switch (readType)
        {
            case ReadType.ReadAsBytes:
                byte[] data;
                if (stringReference.Length == 0)
                {
                    data = Array.Empty<byte>();
                }
                else if (stringReference.Length == 36 && ConvertUtils.TryConvertGuid(stringReference.ToString(), out var g))
                {
                    data = g.ToByteArray();
                }
                else
                {
                    data = Convert.FromBase64CharArray(stringReference.Chars, stringReference.StartIndex, stringReference.Length);
                }

                SetToken(JsonToken.Bytes, data, false);
                break;
            case ReadType.ReadAsString:
                var text = stringReference.ToString();

                SetToken(JsonToken.String, text, false);
                quoteChar = quote;
                break;
            case ReadType.ReadAsInt32:
            case ReadType.ReadAsDecimal:
            case ReadType.ReadAsBoolean:
                // caller will convert result
                break;
            default:
                if (DateParseHandling != DateParseHandling.None)
                {
                    DateParseHandling dateParseHandling;
                    if (readType == ReadType.ReadAsDateTime)
                    {
                        dateParseHandling = DateParseHandling.DateTime;
                    }
                    else if (readType == ReadType.ReadAsDateTimeOffset)
                    {
                        dateParseHandling = DateParseHandling.DateTimeOffset;
                    }
                    else
                    {
                        dateParseHandling = base.DateParseHandling;
                    }

                    if (dateParseHandling == DateParseHandling.DateTime)
                    {
                        if (DateTimeUtils.TryParseDateTime(stringReference, DateTimeZoneHandling, DateFormatString, Culture, out var dt))
                        {
                            SetToken(JsonToken.Date, dt, false);
                            return;
                        }
                    }
                    else
                    {
                        if (DateTimeUtils.TryParseDateTimeOffset(stringReference, DateFormatString, Culture, out var dt))
                        {
                            SetToken(JsonToken.Date, dt, false);
                            return;
                        }
                    }
                }

                SetToken(JsonToken.String, stringReference.ToString(), false);
                quoteChar = quote;
                break;
        }
    }

    static void BlockCopyChars(char[] src, int srcOffset, char[] dst, int dstOffset, int count)
    {
        const int charByteCount = 2;

        Buffer.BlockCopy(src, srcOffset * charByteCount, dst, dstOffset * charByteCount, count * charByteCount);
    }

    void ShiftBufferIfNeeded()
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        // once in the last 10% of the buffer, or buffer is already very large then
        // shift the remaining content to the start to avoid unnecessarily increasing
        // the buffer size when reading numbers/strings
        var length = CharBuffer.Length;
        if (length - CharPos <= length * 0.1 || length >= LargeBufferLength)
        {
            var count = charsUsed - CharPos;
            if (count > 0)
            {
                BlockCopyChars(CharBuffer, CharPos, CharBuffer, 0, count);
            }

            lineStartPos -= CharPos;
            CharPos = 0;
            charsUsed = count;
            CharBuffer[charsUsed] = '\0';
        }
    }

    int ReadData(bool append)
    {
        return ReadData(append, 0);
    }

    void PrepareBufferForReadData(bool append, int charsRequired)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        // char buffer is full
        if (charsUsed + charsRequired >= CharBuffer.Length - 1)
        {
            if (append)
            {
                var doubledArrayLength = CharBuffer.Length * 2;

                // copy to new array either double the size of the current or big enough to fit required content
                var newArrayLength = Math.Max(
                    doubledArrayLength < 0 ? int.MaxValue : doubledArrayLength, // handle overflow
                    charsUsed + charsRequired + 1);

                // increase the size of the buffer
                var dst = BufferUtils.RentBuffer(arrayPool, newArrayLength);

                BlockCopyChars(CharBuffer, 0, dst, 0, CharBuffer.Length);

                BufferUtils.ReturnBuffer(arrayPool, CharBuffer);

                CharBuffer = dst;
            }
            else
            {
                var remainingCharCount = charsUsed - CharPos;

                if (remainingCharCount + charsRequired + 1 >= CharBuffer.Length)
                {
                    // the remaining count plus the required is bigger than the current buffer size
                    var dst = BufferUtils.RentBuffer(arrayPool, remainingCharCount + charsRequired + 1);

                    if (remainingCharCount > 0)
                    {
                        BlockCopyChars(CharBuffer, CharPos, dst, 0, remainingCharCount);
                    }

                    BufferUtils.ReturnBuffer(arrayPool, CharBuffer);

                    CharBuffer = dst;
                }
                else
                {
                    // copy any remaining data to the beginning of the buffer if needed and reset positions
                    if (remainingCharCount > 0)
                    {
                        BlockCopyChars(CharBuffer, CharPos, CharBuffer, 0, remainingCharCount);
                    }
                }

                lineStartPos -= CharPos;
                CharPos = 0;
                charsUsed = remainingCharCount;
            }
        }
    }

    int ReadData(bool append, int charsRequired)
    {
        if (isEndOfFile)
        {
            return 0;
        }

        PrepareBufferForReadData(append, charsRequired);
        MiscellaneousUtils.Assert(CharBuffer != null);

        var attemptCharReadCount = CharBuffer.Length - charsUsed - 1;

        var charsRead = reader.Read(CharBuffer, charsUsed, attemptCharReadCount);

        charsUsed += charsRead;

        if (charsRead == 0)
        {
            isEndOfFile = true;
        }

        CharBuffer[charsUsed] = '\0';
        return charsRead;
    }

    bool EnsureChars(int relativePosition, bool append)
    {
        return CharPos + relativePosition < charsUsed ||
               ReadChars(relativePosition, append);
    }

    bool ReadChars(int relativePosition, bool append)
    {
        if (isEndOfFile)
        {
            return false;
        }

        var charsRequired = CharPos + relativePosition - charsUsed + 1;

        var totalCharsRead = 0;

        // it is possible that the TextReader doesn't return all data at once
        // repeat read until the required text is returned or the reader is out of content
        do
        {
            var charsRead = ReadData(append, charsRequired - totalCharsRead);

            // no more content
            if (charsRead == 0)
            {
                break;
            }

            totalCharsRead += charsRead;
        } while (totalCharsRead < charsRequired);

        return totalCharsRead >= charsRequired;
    }

    /// <summary>
    /// Reads the next JSON token from the underlying <see cref="TextReader"/>.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
    /// </returns>
    public override bool Read()
    {
        EnsureBuffer();
        MiscellaneousUtils.Assert(CharBuffer != null);

        while (true)
        {
            switch (currentState)
            {
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
                    return ParseValue();
                case State.Object:
                case State.ObjectStart:
                    return ParseObject();
                case State.PostValue:
                    // returns true if it hits
                    // end of object or array
                    if (ParsePostValue(false))
                    {
                        return true;
                    }
                    break;
                case State.Finished:
                    if (EnsureChars(0, false))
                    {
                        EatWhitespace();
                        if (isEndOfFile)
                        {
                            SetToken(JsonToken.None);
                            return false;
                        }
                        if (CharBuffer[CharPos] == '/')
                        {
                            ParseComment(true);
                            return true;
                        }

                        throw JsonReaderException.Create(this, $"Additional text encountered after finished reading JSON content: {CharBuffer[CharPos]}.");
                    }
                    SetToken(JsonToken.None);
                    return false;
                default:
                    throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
            }
        }
    }

    /// <summary>
    /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="Int32"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}"/> of <see cref="Int32"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override int? ReadAsInt32()
    {
        return (int?)ReadNumberValue(ReadType.ReadAsInt32);
    }

    /// <summary>
    /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="DateTime"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}"/> of <see cref="DateTime"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override DateTime? ReadAsDateTime()
    {
        return (DateTime?)ReadStringValue(ReadType.ReadAsDateTime);
    }

    /// <summary>
    /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="String"/>.
    /// </summary>
    /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override string? ReadAsString()
    {
        return (string?)ReadStringValue(ReadType.ReadAsString);
    }

    /// <summary>
    /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Byte"/>[].
    /// </summary>
    /// <returns>A <see cref="Byte"/>[] or <c>null</c> if the next JSON token is null. This method will return <c>null</c> at the end of an array.</returns>
    public override byte[]? ReadAsBytes()
    {
        EnsureBuffer();
        MiscellaneousUtils.Assert(CharBuffer != null);

        var isWrapped = false;

        switch (currentState)
        {
            case State.PostValue:
                if (ParsePostValue(true))
                {
                    return null;
                }
                goto case State.Start;
            case State.Start:
            case State.Property:
            case State.Array:
            case State.ArrayStart:
            case State.Constructor:
            case State.ConstructorStart:
                while (true)
                {
                    var currentChar = CharBuffer[CharPos];

                    switch (currentChar)
                    {
                        case '\0':
                            if (ReadNullChar())
                            {
                                SetToken(JsonToken.None, null, false);
                                return null;
                            }
                            break;
                        case '"':
                        case '\'':
                            ParseString(currentChar, ReadType.ReadAsBytes);
                            var data = (byte[]?)Value;
                            if (isWrapped)
                            {
                                ReaderReadAndAssert();
                                if (TokenType != JsonToken.EndObject)
                                {
                                    throw JsonReaderException.Create(this, $"Error reading bytes. Unexpected token: {TokenType}.");
                                }
                                SetToken(JsonToken.Bytes, data, false);
                            }
                            return data;
                        case '{':
                            CharPos++;
                            SetToken(JsonToken.StartObject);
                            ReadIntoWrappedTypeObject();
                            isWrapped = true;
                            break;
                        case '[':
                            CharPos++;
                            SetToken(JsonToken.StartArray);
                            return ReadArrayIntoByteArray();
                        case 'n':
                            HandleNull();
                            return null;
                        case '/':
                            ParseComment(false);
                            break;
                        case ',':
                            ProcessValueComma();
                            break;
                        case ']':
                            CharPos++;
                            if (currentState is State.Array or State.ArrayStart or State.PostValue)
                            {
                                SetToken(JsonToken.EndArray);
                                return null;
                            }
                            throw CreateUnexpectedCharacterException(currentChar);
                        case StringUtils.CarriageReturn:
                            ProcessCarriageReturn(false);
                            break;
                        case StringUtils.LineFeed:
                            ProcessLineFeed();
                            break;
                        case ' ':
                        case StringUtils.Tab:
                            // eat
                            CharPos++;
                            break;
                        default:
                            CharPos++;

                            if (!char.IsWhiteSpace(currentChar))
                            {
                                throw CreateUnexpectedCharacterException(currentChar);
                            }

                            // eat
                            break;
                    }
                }
            case State.Finished:
                ReadFinished();
                return null;
            default:
                throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
        }
    }

    object? ReadStringValue(ReadType readType)
    {
        EnsureBuffer();
        MiscellaneousUtils.Assert(CharBuffer != null);

        switch (currentState)
        {
            case State.PostValue:
                if (ParsePostValue(true))
                {
                    return null;
                }
                goto case State.Start;
            case State.Start:
            case State.Property:
            case State.Array:
            case State.ArrayStart:
            case State.Constructor:
            case State.ConstructorStart:
                while (true)
                {
                    var currentChar = CharBuffer[CharPos];

                    switch (currentChar)
                    {
                        case '\0':
                            if (ReadNullChar())
                            {
                                SetToken(JsonToken.None, null, false);
                                return null;
                            }
                            break;
                        case '"':
                        case '\'':
                            ParseString(currentChar, readType);
                            return FinishReadQuotedStringValue(readType);
                        case '-':
                            if (EnsureChars(1, true) && CharBuffer[CharPos + 1] == 'I')
                            {
                                return ParseNumberNegativeInfinity(readType);
                            }

                            ParseNumber(readType);
                            return Value;
                        case '.':
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            if (readType != ReadType.ReadAsString)
                            {
                                CharPos++;
                                throw CreateUnexpectedCharacterException(currentChar);
                            }
                            ParseNumber(ReadType.ReadAsString);
                            return Value;
                        case 't':
                        case 'f':
                            if (readType != ReadType.ReadAsString)
                            {
                                CharPos++;
                                throw CreateUnexpectedCharacterException(currentChar);
                            }
                            var expected = currentChar == 't' ? JsonConvert.True : JsonConvert.False;
                            if (!MatchValueWithTrailingSeparator(expected))
                            {
                                throw CreateUnexpectedCharacterException(CharBuffer[CharPos]);
                            }
                            SetToken(JsonToken.String, expected);
                            return expected;
                        case 'I':
                            return ParseNumberPositiveInfinity(readType);
                        case 'N':
                            return ParseNumberNaN(readType);
                        case 'n':
                            HandleNull();
                            return null;
                        case '/':
                            ParseComment(false);
                            break;
                        case ',':
                            ProcessValueComma();
                            break;
                        case ']':
                            CharPos++;
                            if (currentState is State.Array or State.ArrayStart or State.PostValue)
                            {
                                SetToken(JsonToken.EndArray);
                                return null;
                            }
                            throw CreateUnexpectedCharacterException(currentChar);
                        case StringUtils.CarriageReturn:
                            ProcessCarriageReturn(false);
                            break;
                        case StringUtils.LineFeed:
                            ProcessLineFeed();
                            break;
                        case ' ':
                        case StringUtils.Tab:
                            // eat
                            CharPos++;
                            break;
                        default:
                            CharPos++;

                            if (!char.IsWhiteSpace(currentChar))
                            {
                                throw CreateUnexpectedCharacterException(currentChar);
                            }

                            // eat
                            break;
                    }
                }
            case State.Finished:
                ReadFinished();
                return null;
            default:
                throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
        }
    }

    object? FinishReadQuotedStringValue(ReadType readType)
    {
        switch (readType)
        {
            case ReadType.ReadAsBytes:
            case ReadType.ReadAsString:
                return Value;
            case ReadType.ReadAsDateTime:
                if (Value is DateTime time)
                {
                    return time;
                }

                return ReadDateTimeString((string?)Value);
            case ReadType.ReadAsDateTimeOffset:
                if (Value is DateTimeOffset offset)
                {
                    return offset;
                }

                return ReadDateTimeOffsetString((string?)Value);
            default:
                throw new ArgumentOutOfRangeException(nameof(readType));
        }
    }

    JsonReaderException CreateUnexpectedCharacterException(char c)
    {
        return JsonReaderException.Create(this, $"Unexpected character encountered while parsing value: {c}.");
    }

    /// <summary>
    /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="Boolean"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}"/> of <see cref="Boolean"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override bool? ReadAsBoolean()
    {
        EnsureBuffer();
        MiscellaneousUtils.Assert(CharBuffer != null);

        switch (currentState)
        {
            case State.PostValue:
                if (ParsePostValue(true))
                {
                    return null;
                }
                goto case State.Start;
            case State.Start:
            case State.Property:
            case State.Array:
            case State.ArrayStart:
            case State.Constructor:
            case State.ConstructorStart:
                while (true)
                {
                    var currentChar = CharBuffer[CharPos];

                    switch (currentChar)
                    {
                        case '\0':
                            if (ReadNullChar())
                            {
                                SetToken(JsonToken.None, null, false);
                                return null;
                            }
                            break;
                        case '"':
                        case '\'':
                            ParseString(currentChar, ReadType.Read);
                            return ReadBooleanString(stringReference.ToString());
                        case 'n':
                            HandleNull();
                            return null;
                        case '-':
                        case '.':
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            ParseNumber(ReadType.Read);
                            bool b;
                            if (Value is BigInteger integer)
                            {
                                b = integer != 0;
                            }
                            else
                            {
                                b = Convert.ToBoolean(Value, CultureInfo.InvariantCulture);
                            }
                            SetToken(JsonToken.Boolean, b, false);
                            return b;
                        case 't':
                        case 'f':
                            var isTrue = currentChar == 't';
                            var expected = isTrue ? JsonConvert.True : JsonConvert.False;

                            if (!MatchValueWithTrailingSeparator(expected))
                            {
                                throw CreateUnexpectedCharacterException(CharBuffer[CharPos]);
                            }
                            SetToken(JsonToken.Boolean, isTrue);
                            return isTrue;
                        case '/':
                            ParseComment(false);
                            break;
                        case ',':
                            ProcessValueComma();
                            break;
                        case ']':
                            CharPos++;
                            if (currentState is State.Array or State.ArrayStart or State.PostValue)
                            {
                                SetToken(JsonToken.EndArray);
                                return null;
                            }
                            throw CreateUnexpectedCharacterException(currentChar);
                        case StringUtils.CarriageReturn:
                            ProcessCarriageReturn(false);
                            break;
                        case StringUtils.LineFeed:
                            ProcessLineFeed();
                            break;
                        case ' ':
                        case StringUtils.Tab:
                            // eat
                            CharPos++;
                            break;
                        default:
                            CharPos++;

                            if (!char.IsWhiteSpace(currentChar))
                            {
                                throw CreateUnexpectedCharacterException(currentChar);
                            }

                            // eat
                            break;
                    }
                }
            case State.Finished:
                ReadFinished();
                return null;
            default:
                throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
        }
    }

    void ProcessValueComma()
    {
        CharPos++;

        if (currentState != State.PostValue)
        {
            SetToken(JsonToken.Undefined);
            var ex = CreateUnexpectedCharacterException(',');
            // so the comma will be parsed again
            CharPos--;

            throw ex;
        }

        SetStateBasedOnCurrent();
    }

    object? ReadNumberValue(ReadType readType)
    {
        EnsureBuffer();
        MiscellaneousUtils.Assert(CharBuffer != null);

        switch (currentState)
        {
            case State.PostValue:
                if (ParsePostValue(true))
                {
                    return null;
                }
                goto case State.Start;
            case State.Start:
            case State.Property:
            case State.Array:
            case State.ArrayStart:
            case State.Constructor:
            case State.ConstructorStart:
                while (true)
                {
                    var currentChar = CharBuffer[CharPos];

                    switch (currentChar)
                    {
                        case '\0':
                            if (ReadNullChar())
                            {
                                SetToken(JsonToken.None, null, false);
                                return null;
                            }
                            break;
                        case '"':
                        case '\'':
                            ParseString(currentChar, readType);
                            return FinishReadQuotedNumber(readType);
                        case 'n':
                            HandleNull();
                            return null;
                        case 'N':
                            return ParseNumberNaN(readType);
                        case 'I':
                            return ParseNumberPositiveInfinity(readType);
                        case '-':
                            if (EnsureChars(1, true) && CharBuffer[CharPos + 1] == 'I')
                            {
                                return ParseNumberNegativeInfinity(readType);
                            }

                            ParseNumber(readType);
                            return Value;
                        case '.':
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            ParseNumber(readType);
                            return Value;
                        case '/':
                            ParseComment(false);
                            break;
                        case ',':
                            ProcessValueComma();
                            break;
                        case ']':
                            CharPos++;
                            if (currentState is State.Array or State.ArrayStart or State.PostValue)
                            {
                                SetToken(JsonToken.EndArray);
                                return null;
                            }
                            throw CreateUnexpectedCharacterException(currentChar);
                        case StringUtils.CarriageReturn:
                            ProcessCarriageReturn(false);
                            break;
                        case StringUtils.LineFeed:
                            ProcessLineFeed();
                            break;
                        case ' ':
                        case StringUtils.Tab:
                            // eat
                            CharPos++;
                            break;
                        default:
                            CharPos++;

                            if (!char.IsWhiteSpace(currentChar))
                            {
                                throw CreateUnexpectedCharacterException(currentChar);
                            }

                            // eat
                            break;
                    }
                }
            case State.Finished:
                ReadFinished();
                return null;
            default:
                throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
        }
    }

    object? FinishReadQuotedNumber(ReadType readType)
    {
        switch (readType)
        {
            case ReadType.ReadAsInt32:
                return ReadInt32String(stringReference.ToString());
            case ReadType.ReadAsDecimal:
                return ReadDecimalString(stringReference.ToString());
            case ReadType.ReadAsDouble:
                return ReadDoubleString(stringReference.ToString());
            default:
                throw new ArgumentOutOfRangeException(nameof(readType));
        }
    }

    /// <summary>
    /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override DateTimeOffset? ReadAsDateTimeOffset()
    {
        return (DateTimeOffset?)ReadStringValue(ReadType.ReadAsDateTimeOffset);
    }

    /// <summary>
    /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="Decimal"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}"/> of <see cref="Decimal"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override decimal? ReadAsDecimal()
    {
        return (decimal?)ReadNumberValue(ReadType.ReadAsDecimal);
    }

    /// <summary>
    /// Reads the next JSON token from the underlying <see cref="TextReader"/> as a <see cref="Nullable{T}"/> of <see cref="Double"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}"/> of <see cref="Double"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override double? ReadAsDouble()
    {
        return (double?)ReadNumberValue(ReadType.ReadAsDouble);
    }

    void HandleNull()
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (EnsureChars(1, true))
        {
            var next = CharBuffer[CharPos + 1];

            if (next == 'u')
            {
                ParseNull();
                return;
            }

            CharPos += 2;
            throw CreateUnexpectedCharacterException(CharBuffer[CharPos - 1]);
        }

        CharPos = charsUsed;
        throw CreateUnexpectedEndException();
    }

    void ReadFinished()
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (EnsureChars(0, false))
        {
            EatWhitespace();
            if (isEndOfFile)
            {
                return;
            }
            if (CharBuffer[CharPos] == '/')
            {
                ParseComment(false);
            }
            else
            {
                throw JsonReaderException.Create(this, $"Additional text encountered after finished reading JSON content: {CharBuffer[CharPos]}.");
            }
        }

        SetToken(JsonToken.None);
    }

    bool ReadNullChar()
    {
        if (charsUsed == CharPos)
        {
            if (ReadData(false) == 0)
            {
                isEndOfFile = true;
                return true;
            }
        }
        else
        {
            CharPos++;
        }

        return false;
    }

    void EnsureBuffer()
    {
        if (CharBuffer == null)
        {
            CharBuffer = BufferUtils.RentBuffer(arrayPool, 1024);
            CharBuffer[0] = '\0';
        }
    }

    void ReadStringIntoBuffer(char quote)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        var charPos = CharPos;
        var initialPosition = CharPos;
        var lastWritePosition = CharPos;
        stringBuffer.Position = 0;

        while (true)
        {
            switch (CharBuffer[charPos++])
            {
                case '\0':
                    if (charsUsed == charPos - 1)
                    {
                        charPos--;

                        if (ReadData(true) == 0)
                        {
                            CharPos = charPos;
                            throw JsonReaderException.Create(this, $"Unterminated string. Expected delimiter: {quote}.");
                        }
                    }
                    break;
                case '\\':
                    CharPos = charPos;
                    if (!EnsureChars(0, true))
                    {
                        throw JsonReaderException.Create(this, $"Unterminated string. Expected delimiter: {quote}.");
                    }

                    // start of escape sequence
                    var escapeStartPos = charPos - 1;

                    var currentChar = CharBuffer[charPos];
                    charPos++;

                    char writeChar;

                    switch (currentChar)
                    {
                        case 'b':
                            writeChar = '\b';
                            break;
                        case 't':
                            writeChar = '\t';
                            break;
                        case 'n':
                            writeChar = '\n';
                            break;
                        case 'f':
                            writeChar = '\f';
                            break;
                        case 'r':
                            writeChar = '\r';
                            break;
                        case '\\':
                            writeChar = '\\';
                            break;
                        case '"':
                        case '\'':
                        case '/':
                            writeChar = currentChar;
                            break;
                        case 'u':
                            CharPos = charPos;
                            writeChar = ParseUnicode();

                            if (StringUtils.IsLowSurrogate(writeChar))
                            {
                                // low surrogate with no preceding high surrogate; this char is replaced
                                writeChar = unicodeReplacementChar;
                            }
                            else if (StringUtils.IsHighSurrogate(writeChar))
                            {
                                bool anotherHighSurrogate;

                                // loop for handling situations where there are multiple consecutive high surrogates
                                do
                                {
                                    anotherHighSurrogate = false;

                                    // potential start of a surrogate pair
                                    if (EnsureChars(2, true) && CharBuffer[CharPos] == '\\' && CharBuffer[CharPos + 1] == 'u')
                                    {
                                        var highSurrogate = writeChar;

                                        CharPos += 2;
                                        writeChar = ParseUnicode();

                                        if (StringUtils.IsLowSurrogate(writeChar))
                                        {
                                            // a valid surrogate pair!
                                        }
                                        else if (StringUtils.IsHighSurrogate(writeChar))
                                        {
                                            // another high surrogate; replace current and start check over
                                            highSurrogate = unicodeReplacementChar;
                                            anotherHighSurrogate = true;
                                        }
                                        else
                                        {
                                            // high surrogate not followed by low surrogate; original char is replaced
                                            highSurrogate = unicodeReplacementChar;
                                        }

                                        EnsureBufferNotEmpty();

                                        WriteCharToBuffer(highSurrogate, lastWritePosition, escapeStartPos);
                                        lastWritePosition = CharPos;
                                    }
                                    else
                                    {
                                        // there are not enough remaining chars for the low surrogate or is not follow by unicode sequence
                                        // replace high surrogate and continue on as usual
                                        writeChar = unicodeReplacementChar;
                                    }
                                } while (anotherHighSurrogate);
                            }

                            charPos = CharPos;
                            break;
                        default:
                            CharPos = charPos;
                            throw JsonReaderException.Create(this, $"Bad JSON escape sequence: \\{currentChar}.");
                    }

                    EnsureBufferNotEmpty();
                    WriteCharToBuffer(writeChar, lastWritePosition, escapeStartPos);

                    lastWritePosition = charPos;
                    break;
                case StringUtils.CarriageReturn:
                    CharPos = charPos - 1;
                    ProcessCarriageReturn(true);
                    charPos = CharPos;
                    break;
                case StringUtils.LineFeed:
                    CharPos = charPos - 1;
                    ProcessLineFeed();
                    charPos = CharPos;
                    break;
                case '"':
                case '\'':
                    if (CharBuffer[charPos - 1] == quote)
                    {
                        FinishReadStringIntoBuffer(charPos - 1, initialPosition, lastWritePosition);
                        return;
                    }
                    break;
            }
        }
    }

    void FinishReadStringIntoBuffer(int charPos, int initialPosition, int lastWritePosition)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (initialPosition == lastWritePosition)
        {
            stringReference = new StringReference(CharBuffer, initialPosition, charPos - initialPosition);
        }
        else
        {
            EnsureBufferNotEmpty();

            if (charPos > lastWritePosition)
            {
                stringBuffer.Append(arrayPool, CharBuffer, lastWritePosition, charPos - lastWritePosition);
            }

            stringReference = new StringReference(stringBuffer.InternalBuffer!, 0, stringBuffer.Position);
        }

        CharPos = charPos + 1;
    }

    void WriteCharToBuffer(char writeChar, int lastWritePosition, int writeToPosition)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (writeToPosition > lastWritePosition)
        {
            stringBuffer.Append(arrayPool, CharBuffer, lastWritePosition, writeToPosition - lastWritePosition);
        }

        stringBuffer.Append(arrayPool, writeChar);
    }

    char ConvertUnicode(bool enoughChars)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (enoughChars)
        {
            if (ConvertUtils.TryHexTextToInt(CharBuffer, CharPos, CharPos + 4, out var value))
            {
                var hexChar = Convert.ToChar(value);
                CharPos += 4;
                return hexChar;
            }

            throw JsonReaderException.Create(this, $@"Invalid Unicode escape sequence: \u{new string(CharBuffer, CharPos, 4)}.");
        }

        throw JsonReaderException.Create(this, "Unexpected end while parsing Unicode escape sequence.");
    }

    char ParseUnicode()
    {
        return ConvertUnicode(EnsureChars(4, true));
    }

    void ReadNumberIntoBuffer()
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        var charPos = CharPos;

        while (true)
        {
            var currentChar = CharBuffer[charPos];
            if (currentChar == '\0')
            {
                CharPos = charPos;

                if (charsUsed == charPos)
                {
                    if (ReadData(true) == 0)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else if (ReadNumberCharIntoBuffer(currentChar, charPos))
            {
                return;
            }
            else
            {
                charPos++;
            }
        }
    }

    bool ReadNumberCharIntoBuffer(char currentChar, int charPos)
    {
        switch (currentChar)
        {
            case '-':
            case '+':
            case 'a':
            case 'A':
            case 'b':
            case 'B':
            case 'c':
            case 'C':
            case 'd':
            case 'D':
            case 'e':
            case 'E':
            case 'f':
            case 'F':
            case 'x':
            case 'X':
            case '.':
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                return false;
            default:
                CharPos = charPos;

                if (char.IsWhiteSpace(currentChar) || currentChar is ',' or '}' or ']' or ')' or '/')
                {
                    return true;
                }

                throw JsonReaderException.Create(this, $"Unexpected character encountered while parsing number: {currentChar}.");
        }
    }

    void ClearRecentString()
    {
        stringBuffer.Position = 0;
        stringReference = new StringReference();
    }

    bool ParsePostValue(bool ignoreComments)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        while (true)
        {
            var currentChar = CharBuffer[CharPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == CharPos)
                    {
                        if (ReadData(false) == 0)
                        {
                            currentState = State.Finished;
                            return false;
                        }
                    }
                    else
                    {
                        CharPos++;
                    }
                    break;
                case '}':
                    CharPos++;
                    SetToken(JsonToken.EndObject);
                    return true;
                case ']':
                    CharPos++;
                    SetToken(JsonToken.EndArray);
                    return true;
                case ')':
                    CharPos++;
                    SetToken(JsonToken.EndConstructor);
                    return true;
                case '/':
                    ParseComment(!ignoreComments);
                    if (!ignoreComments)
                    {
                        return true;
                    }
                    break;
                case ',':
                    CharPos++;

                    // finished parsing
                    SetStateBasedOnCurrent();
                    return false;
                case ' ':
                case StringUtils.Tab:
                    // eat
                    CharPos++;
                    break;
                case StringUtils.CarriageReturn:
                    ProcessCarriageReturn(false);
                    break;
                case StringUtils.LineFeed:
                    ProcessLineFeed();
                    break;
                default:
                    if (char.IsWhiteSpace(currentChar))
                    {
                        // eat
                        CharPos++;
                    }
                    else
                    {
                        // handle multiple content without comma delimiter
                        if (SupportMultipleContent && Depth == 0)
                        {
                            SetStateBasedOnCurrent();
                            return false;
                        }

                        throw JsonReaderException.Create(this, $"After parsing a value an unexpected character was encountered: {currentChar}.");
                    }
                    break;
            }
        }
    }

    bool ParseObject()
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        while (true)
        {
            var currentChar = CharBuffer[CharPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == CharPos)
                    {
                        if (ReadData(false) == 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        CharPos++;
                    }
                    break;
                case '}':
                    SetToken(JsonToken.EndObject);
                    CharPos++;
                    return true;
                case '/':
                    ParseComment(true);
                    return true;
                case StringUtils.CarriageReturn:
                    ProcessCarriageReturn(false);
                    break;
                case StringUtils.LineFeed:
                    ProcessLineFeed();
                    break;
                case ' ':
                case StringUtils.Tab:
                    // eat
                    CharPos++;
                    break;
                default:
                    if (char.IsWhiteSpace(currentChar))
                    {
                        // eat
                        CharPos++;
                    }
                    else
                    {
                        return ParseProperty();
                    }
                    break;
            }
        }
    }

    bool ParseProperty()
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        var firstChar = CharBuffer[CharPos];
        char quoteChar;

        if (firstChar is '"' or '\'')
        {
            CharPos++;
            quoteChar = firstChar;
            ShiftBufferIfNeeded();
            ReadStringIntoBuffer(quoteChar);
        }
        else if (ValidIdentifierChar(firstChar))
        {
            quoteChar = '\0';
            ShiftBufferIfNeeded();
            ParseUnquotedProperty();
        }
        else
        {
            throw JsonReaderException.Create(this, $"Invalid property identifier character: {CharBuffer[CharPos]}.");
        }

        string? propertyName;

        if (PropertyNameTable == null)
        {
            propertyName = stringReference.ToString();
        }
        else
        {
            propertyName = PropertyNameTable.Get(stringReference.Chars, stringReference.StartIndex, stringReference.Length) ??
                           // no match in name table
                           stringReference.ToString();
        }

        EatWhitespace();

        if (CharBuffer[CharPos] != ':')
        {
            throw JsonReaderException.Create(this, $"Invalid character after parsing property name. Expected ':' but got: {CharBuffer[CharPos]}.");
        }

        CharPos++;

        SetToken(JsonToken.PropertyName, propertyName);
        base.quoteChar = quoteChar;
        ClearRecentString();

        return true;
    }

    static bool ValidIdentifierChar(char value)
    {
        return char.IsLetterOrDigit(value) || value is '_' or '$';
    }

    void ParseUnquotedProperty()
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        var initialPosition = CharPos;

        // parse unquoted property name until whitespace or colon
        while (true)
        {
            var currentChar = CharBuffer[CharPos];
            if (currentChar == '\0')
            {
                if (charsUsed == CharPos)
                {
                    if (ReadData(true) == 0)
                    {
                        throw JsonReaderException.Create(this, "Unexpected end while parsing unquoted property name.");
                    }

                    continue;
                }

                stringReference = new StringReference(CharBuffer, initialPosition, CharPos - initialPosition);
                return;
            }

            if (ReadUnquotedPropertyReportIfDone(currentChar, initialPosition))
            {
                return;
            }
        }
    }

    bool ReadUnquotedPropertyReportIfDone(char currentChar, int initialPosition)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (ValidIdentifierChar(currentChar))
        {
            CharPos++;
            return false;
        }

        if (char.IsWhiteSpace(currentChar) || currentChar == ':')
        {
            stringReference = new StringReference(CharBuffer, initialPosition, CharPos - initialPosition);
            return true;
        }

        throw JsonReaderException.Create(this, $"Invalid JavaScript property identifier character: {currentChar}.");
    }

    bool ParseValue()
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        while (true)
        {
            var currentChar = CharBuffer[CharPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == CharPos)
                    {
                        if (ReadData(false) == 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        CharPos++;
                    }
                    break;
                case '"':
                case '\'':
                    ParseString(currentChar, ReadType.Read);
                    return true;
                case 't':
                    ParseTrue();
                    return true;
                case 'f':
                    ParseFalse();
                    return true;
                case 'n':
                    if (EnsureChars(1, true))
                    {
                        var next = CharBuffer[CharPos + 1];

                        if (next == 'u')
                        {
                            ParseNull();
                        }
                        else if (next == 'e')
                        {
                            ParseConstructor();
                        }
                        else
                        {
                            throw CreateUnexpectedCharacterException(CharBuffer[CharPos]);
                        }
                    }
                    else
                    {
                        CharPos++;
                        throw CreateUnexpectedEndException();
                    }
                    return true;
                case 'N':
                    ParseNumberNaN(ReadType.Read);
                    return true;
                case 'I':
                    ParseNumberPositiveInfinity(ReadType.Read);
                    return true;
                case '-':
                    if (EnsureChars(1, true) && CharBuffer[CharPos + 1] == 'I')
                    {
                        ParseNumberNegativeInfinity(ReadType.Read);
                    }
                    else
                    {
                        ParseNumber(ReadType.Read);
                    }
                    return true;
                case '/':
                    ParseComment(true);
                    return true;
                case 'u':
                    ParseUndefined();
                    return true;
                case '{':
                    CharPos++;
                    SetToken(JsonToken.StartObject);
                    return true;
                case '[':
                    CharPos++;
                    SetToken(JsonToken.StartArray);
                    return true;
                case ']':
                    CharPos++;
                    SetToken(JsonToken.EndArray);
                    return true;
                case ',':
                    // don't increment position, the next call to read will handle comma
                    // this is done to handle multiple empty comma values
                    SetToken(JsonToken.Undefined);
                    return true;
                case ')':
                    CharPos++;
                    SetToken(JsonToken.EndConstructor);
                    return true;
                case StringUtils.CarriageReturn:
                    ProcessCarriageReturn(false);
                    break;
                case StringUtils.LineFeed:
                    ProcessLineFeed();
                    break;
                case ' ':
                case StringUtils.Tab:
                    // eat
                    CharPos++;
                    break;
                default:
                    if (char.IsWhiteSpace(currentChar))
                    {
                        // eat
                        CharPos++;
                        break;
                    }
                    if (char.IsNumber(currentChar) || currentChar is '-' or '.')
                    {
                        ParseNumber(ReadType.Read);
                        return true;
                    }

                    throw CreateUnexpectedCharacterException(currentChar);
            }
        }
    }

    void ProcessLineFeed()
    {
        CharPos++;
        OnNewLine(CharPos);
    }

    void ProcessCarriageReturn(bool append)
    {
        CharPos++;

        SetNewLine(EnsureChars(1, append));
    }

    void EatWhitespace()
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        while (true)
        {
            var currentChar = CharBuffer[CharPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == CharPos)
                    {
                        if (ReadData(false) == 0)
                        {
                            return;
                        }
                    }
                    else
                    {
                        CharPos++;
                    }
                    break;
                case StringUtils.CarriageReturn:
                    ProcessCarriageReturn(false);
                    break;
                case StringUtils.LineFeed:
                    ProcessLineFeed();
                    break;
                default:
                    if (currentChar == ' ' || char.IsWhiteSpace(currentChar))
                    {
                        CharPos++;
                    }
                    else
                    {
                        return;
                    }
                    break;
            }
        }
    }

    void ParseConstructor()
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (MatchValueWithTrailingSeparator("new"))
        {
            EatWhitespace();

            var initialPosition = CharPos;
            int endPosition;

            while (true)
            {
                var currentChar = CharBuffer[CharPos];
                if (currentChar == '\0')
                {
                    if (charsUsed == CharPos)
                    {
                        if (ReadData(true) == 0)
                        {
                            throw JsonReaderException.Create(this, "Unexpected end while parsing constructor.");
                        }
                    }
                    else
                    {
                        endPosition = CharPos;
                        CharPos++;
                        break;
                    }
                }
                else if (char.IsLetterOrDigit(currentChar))
                {
                    CharPos++;
                }
                else if (currentChar == StringUtils.CarriageReturn)
                {
                    endPosition = CharPos;
                    ProcessCarriageReturn(true);
                    break;
                }
                else if (currentChar == StringUtils.LineFeed)
                {
                    endPosition = CharPos;
                    ProcessLineFeed();
                    break;
                }
                else if (char.IsWhiteSpace(currentChar))
                {
                    endPosition = CharPos;
                    CharPos++;
                    break;
                }
                else if (currentChar == '(')
                {
                    endPosition = CharPos;
                    break;
                }
                else
                {
                    throw JsonReaderException.Create(this, $"Unexpected character while parsing constructor: {currentChar}.");
                }
            }

            stringReference = new StringReference(CharBuffer, initialPosition, endPosition - initialPosition);
            var constructorName = stringReference.ToString();

            EatWhitespace();

            if (CharBuffer[CharPos] != '(')
            {
                throw JsonReaderException.Create(this, $"Unexpected character while parsing constructor: {CharBuffer[CharPos]}.");
            }

            CharPos++;

            ClearRecentString();

            SetToken(JsonToken.StartConstructor, constructorName);
        }
        else
        {
            throw JsonReaderException.Create(this, "Unexpected content while parsing JSON.");
        }
    }

    void ParseNumber(ReadType readType)
    {
        ShiftBufferIfNeeded();
        MiscellaneousUtils.Assert(CharBuffer != null);

        var firstChar = CharBuffer[CharPos];
        var initialPosition = CharPos;

        ReadNumberIntoBuffer();

        ParseReadNumber(readType, firstChar, initialPosition);
    }

    void ParseReadNumber(ReadType readType, char firstChar, int initialPosition)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        // set state to PostValue now so that if there is an error parsing the number then the reader can continue
        SetPostValueState(true);

        stringReference = new StringReference(CharBuffer, initialPosition, CharPos - initialPosition);

        object numberValue;
        JsonToken numberType;

        var singleDigit = char.IsDigit(firstChar) && stringReference.Length == 1;
        var nonBase10 = firstChar == '0' && stringReference.Length > 1 && stringReference.Chars[stringReference.StartIndex + 1] != '.' && stringReference.Chars[stringReference.StartIndex + 1] != 'e' && stringReference.Chars[stringReference.StartIndex + 1] != 'E';

        switch (readType)
        {
            case ReadType.ReadAsString:
            {
                var number = stringReference.ToString();

                // validate that the string is a valid number
                if (nonBase10)
                {
                    try
                    {
                        if (number.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            Convert.ToInt64(number, 16);
                        }
                        else
                        {
                            Convert.ToInt64(number, 8);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ThrowReaderError($"Input string '{number}' is not a valid number.", ex);
                    }
                }
                else
                {
                    if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        throw ThrowReaderError($"Input string '{stringReference.ToString()}' is not a valid number.");
                    }
                }

                numberType = JsonToken.String;
                numberValue = number;
            }
                break;
            case ReadType.ReadAsInt32:
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = firstChar - 48;
                }
                else if (nonBase10)
                {
                    var number = stringReference.ToString();

                    try
                    {
                        var integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(number, 16) : Convert.ToInt32(number, 8);

                        numberValue = integer;
                    }
                    catch (Exception ex)
                    {
                        throw ThrowReaderError($"Input string '{number}' is not a valid integer.", ex);
                    }
                }
                else
                {
                    var parseResult = ConvertUtils.Int32TryParse(stringReference.Chars, stringReference.StartIndex, stringReference.Length, out var value);
                    if (parseResult == ParseResult.Success)
                    {
                        numberValue = value;
                    }
                    else if (parseResult == ParseResult.Overflow)
                    {
                        throw ThrowReaderError($"JSON integer {stringReference.ToString()} is too large or small for an Int32.");
                    }
                    else
                    {
                        throw ThrowReaderError($"Input string '{stringReference.ToString()}' is not a valid integer.");
                    }
                }

                numberType = JsonToken.Integer;
            }
                break;
            case ReadType.ReadAsDecimal:
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = (decimal)firstChar - 48;
                }
                else if (nonBase10)
                {
                    var number = stringReference.ToString();

                    try
                    {
                        // decimal.Parse doesn't support parsing hexadecimal values
                        var integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(number, 16) : Convert.ToInt64(number, 8);

                        numberValue = Convert.ToDecimal(integer);
                    }
                    catch (Exception ex)
                    {
                        throw ThrowReaderError($"Input string '{number}' is not a valid decimal.", ex);
                    }
                }
                else
                {
                    var parseResult = ConvertUtils.DecimalTryParse(stringReference.Chars, stringReference.StartIndex, stringReference.Length, out var value);
                    if (parseResult == ParseResult.Success)
                    {
                        numberValue = value;
                    }
                    else
                    {
                        throw ThrowReaderError($"Input string '{stringReference.ToString()}' is not a valid decimal.");
                    }
                }

                numberType = JsonToken.Float;
            }
                break;
            case ReadType.ReadAsDouble:
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = (double)firstChar - 48;
                }
                else if (nonBase10)
                {
                    var number = stringReference.ToString();

                    try
                    {
                        // double.Parse doesn't support parsing hexadecimal values
                        var integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(number, 16) : Convert.ToInt64(number, 8);

                        numberValue = Convert.ToDouble(integer);
                    }
                    catch (Exception ex)
                    {
                        throw ThrowReaderError($"Input string '{number}' is not a valid double.", ex);
                    }
                }
                else
                {
                    var number = stringReference.ToString();

                    if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    {
                        numberValue = value;
                    }
                    else
                    {
                        throw ThrowReaderError($"Input string '{stringReference.ToString()}' is not a valid double.");
                    }
                }

                numberType = JsonToken.Float;
            }
                break;
            case ReadType.Read:
            case ReadType.ReadAsInt64:
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = (long)firstChar - 48;
                    numberType = JsonToken.Integer;
                }
                else if (nonBase10)
                {
                    var number = stringReference.ToString();

                    try
                    {
                        numberValue = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(number, 16) : Convert.ToInt64(number, 8);
                    }
                    catch (Exception ex)
                    {
                        throw ThrowReaderError($"Input string '{number}' is not a valid number.", ex);
                    }

                    numberType = JsonToken.Integer;
                }
                else
                {
                    var parseResult = ConvertUtils.Int64TryParse(stringReference.Chars, stringReference.StartIndex, stringReference.Length, out var value);
                    if (parseResult == ParseResult.Success)
                    {
                        numberValue = value;
                        numberType = JsonToken.Integer;
                    }
                    else if (parseResult == ParseResult.Overflow)
                    {
                        var number = stringReference.ToString();

                        if (number.Length > maximumJavascriptIntegerCharacterLength)
                        {
                            throw ThrowReaderError($"JSON integer {stringReference.ToString()} is too large to parse.");
                        }

                        numberValue = BigIntegerParse(number, CultureInfo.InvariantCulture);
                        numberType = JsonToken.Integer;
                    }
                    else
                    {
                        if (FloatParseHandling == FloatParseHandling.Decimal)
                        {
                            parseResult = ConvertUtils.DecimalTryParse(stringReference.Chars, stringReference.StartIndex, stringReference.Length, out var d);
                            if (parseResult == ParseResult.Success)
                            {
                                numberValue = d;
                            }
                            else
                            {
                                throw ThrowReaderError($"Input string '{stringReference.ToString()}' is not a valid decimal.");
                            }
                        }
                        else
                        {
                            var number = stringReference.ToString();

                            if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                            {
                                numberValue = d;
                            }
                            else
                            {
                                throw ThrowReaderError($"Input string '{stringReference.ToString()}' is not a valid number.");
                            }
                        }

                        numberType = JsonToken.Float;
                    }
                }
            }
                break;
            default:
                throw JsonReaderException.Create(this, "Cannot read number value as type.");
        }

        ClearRecentString();

        // index has already been updated
        SetToken(numberType, numberValue, false);
    }

    JsonReaderException ThrowReaderError(string message, Exception? ex = null)
    {
        SetToken(JsonToken.Undefined, null, false);
        return JsonReaderException.Create(this, message, ex);
    }

    // By using the BigInteger type in a separate method,
    // the runtime can execute the ParseNumber even if
    // the System.Numerics.BigInteger.Parse method is
    // missing, which happens in some versions of Mono
    [MethodImpl(MethodImplOptions.NoInlining)]
    static object BigIntegerParse(string number, CultureInfo culture)
    {
        return BigInteger.Parse(number, culture);
    }

    void ParseComment(bool setToken)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        // should have already parsed / character before reaching this method
        CharPos++;

        if (!EnsureChars(1, false))
        {
            throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
        }

        bool singleLineComment;

        if (CharBuffer[CharPos] == '*')
        {
            singleLineComment = false;
        }
        else if (CharBuffer[CharPos] == '/')
        {
            singleLineComment = true;
        }
        else
        {
            throw JsonReaderException.Create(this, $"Error parsing comment. Expected: *, got {CharBuffer[CharPos]}.");
        }

        CharPos++;

        var initialPosition = CharPos;

        while (true)
        {
            switch (CharBuffer[CharPos])
            {
                case '\0':
                    if (charsUsed == CharPos)
                    {
                        if (ReadData(true) == 0)
                        {
                            if (!singleLineComment)
                            {
                                throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
                            }

                            EndComment(setToken, initialPosition, CharPos);
                            return;
                        }
                    }
                    else
                    {
                        CharPos++;
                    }
                    break;
                case '*':
                    CharPos++;

                    if (!singleLineComment)
                    {
                        if (EnsureChars(0, true))
                        {
                            if (CharBuffer[CharPos] == '/')
                            {
                                EndComment(setToken, initialPosition, CharPos - 1);

                                CharPos++;
                                return;
                            }
                        }
                    }
                    break;
                case StringUtils.CarriageReturn:
                    if (singleLineComment)
                    {
                        EndComment(setToken, initialPosition, CharPos);
                        return;
                    }
                    ProcessCarriageReturn(true);
                    break;
                case StringUtils.LineFeed:
                    if (singleLineComment)
                    {
                        EndComment(setToken, initialPosition, CharPos);
                        return;
                    }
                    ProcessLineFeed();
                    break;
                default:
                    CharPos++;
                    break;
            }
        }
    }

    void EndComment(bool setToken, int initialPosition, int endPosition)
    {
        if (setToken)
        {
            SetToken(JsonToken.Comment, new string(CharBuffer, initialPosition, endPosition - initialPosition));
        }
    }

    bool MatchValue(string value)
    {
        return MatchValue(EnsureChars(value.Length - 1, true), value);
    }

    bool MatchValue(bool enoughChars, string value)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (!enoughChars)
        {
            CharPos = charsUsed;
            throw CreateUnexpectedEndException();
        }

        for (var i = 0; i < value.Length; i++)
        {
            if (CharBuffer[CharPos + i] != value[i])
            {
                CharPos += i;
                return false;
            }
        }

        CharPos += value.Length;

        return true;
    }

    bool MatchValueWithTrailingSeparator(string value)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        // will match value and then move to the next character, checking that it is a separator character
        var match = MatchValue(value);

        if (!match)
        {
            return false;
        }

        if (!EnsureChars(0, false))
        {
            return true;
        }

        return IsSeparator(CharBuffer[CharPos]) || CharBuffer[CharPos] == '\0';
    }

    bool IsSeparator(char c)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        switch (c)
        {
            case '}':
            case ']':
            case ',':
                return true;
            case '/':
                // check next character to see if start of a comment
                if (!EnsureChars(1, false))
                {
                    return false;
                }

                var nextChart = CharBuffer[CharPos + 1];

                return nextChart is '*' or '/';
            case ')':
                if (CurrentState is State.Constructor or State.ConstructorStart)
                {
                    return true;
                }
                break;
            case ' ':
            case StringUtils.Tab:
            case StringUtils.LineFeed:
            case StringUtils.CarriageReturn:
                return true;
            default:
                if (char.IsWhiteSpace(c))
                {
                    return true;
                }
                break;
        }

        return false;
    }

    void ParseTrue()
    {
        // check characters equal 'true'
        // and that it is followed by either a separator character
        // or the text ends
        if (MatchValueWithTrailingSeparator(JsonConvert.True))
        {
            SetToken(JsonToken.Boolean, true);
        }
        else
        {
            throw JsonReaderException.Create(this, "Error parsing boolean value.");
        }
    }

    void ParseNull()
    {
        if (MatchValueWithTrailingSeparator(JsonConvert.Null))
        {
            SetToken(JsonToken.Null);
        }
        else
        {
            throw JsonReaderException.Create(this, "Error parsing null value.");
        }
    }

    void ParseUndefined()
    {
        if (MatchValueWithTrailingSeparator(JsonConvert.Undefined))
        {
            SetToken(JsonToken.Undefined);
        }
        else
        {
            throw JsonReaderException.Create(this, "Error parsing undefined value.");
        }
    }

    void ParseFalse()
    {
        if (MatchValueWithTrailingSeparator(JsonConvert.False))
        {
            SetToken(JsonToken.Boolean, false);
        }
        else
        {
            throw JsonReaderException.Create(this, "Error parsing boolean value.");
        }
    }

    object ParseNumberNegativeInfinity(ReadType readType)
    {
        return ParseNumberNegativeInfinity(readType, MatchValueWithTrailingSeparator(JsonConvert.NegativeInfinity));
    }

    object ParseNumberNegativeInfinity(ReadType readType, bool matched)
    {
        if (matched)
        {
            switch (readType)
            {
                case ReadType.Read:
                case ReadType.ReadAsDouble:
                    if (FloatParseHandling == FloatParseHandling.Double)
                    {
                        SetToken(JsonToken.Float, double.NegativeInfinity);
                        return double.NegativeInfinity;
                    }
                    break;
                case ReadType.ReadAsString:
                    SetToken(JsonToken.String, JsonConvert.NegativeInfinity);
                    return JsonConvert.NegativeInfinity;
            }

            throw JsonReaderException.Create(this, "Cannot read -Infinity value.");
        }

        throw JsonReaderException.Create(this, "Error parsing -Infinity value.");
    }

    object ParseNumberPositiveInfinity(ReadType readType)
    {
        return ParseNumberPositiveInfinity(readType, MatchValueWithTrailingSeparator(JsonConvert.PositiveInfinity));
    }
    object ParseNumberPositiveInfinity(ReadType readType, bool matched)
    {
        if (matched)
        {
            switch (readType)
            {
                case ReadType.Read:
                case ReadType.ReadAsDouble:
                    if (FloatParseHandling == FloatParseHandling.Double)
                    {
                        SetToken(JsonToken.Float, double.PositiveInfinity);
                        return double.PositiveInfinity;
                    }
                    break;
                case ReadType.ReadAsString:
                    SetToken(JsonToken.String, JsonConvert.PositiveInfinity);
                    return JsonConvert.PositiveInfinity;
            }

            throw JsonReaderException.Create(this, "Cannot read Infinity value.");
        }

        throw JsonReaderException.Create(this, "Error parsing Infinity value.");
    }

    object ParseNumberNaN(ReadType readType)
    {
        return ParseNumberNaN(readType, MatchValueWithTrailingSeparator(JsonConvert.NaN));
    }

    object ParseNumberNaN(ReadType readType, bool matched)
    {
        if (matched)
        {
            switch (readType)
            {
                case ReadType.Read:
                case ReadType.ReadAsDouble:
                    if (FloatParseHandling == FloatParseHandling.Double)
                    {
                        SetToken(JsonToken.Float, double.NaN);
                        return double.NaN;
                    }
                    break;
                case ReadType.ReadAsString:
                    SetToken(JsonToken.String, JsonConvert.NaN);
                    return JsonConvert.NaN;
            }

            throw JsonReaderException.Create(this, "Cannot read NaN value.");
        }

        throw JsonReaderException.Create(this, "Error parsing NaN value.");
    }

    /// <summary>
    /// Changes the reader's state to <see cref="JsonReader.State.Closed"/>.
    /// If <see cref="JsonReader.CloseInput"/> is set to <c>true</c>, the underlying <see cref="TextReader"/> is also closed.
    /// </summary>
    public override void Close()
    {
        base.Close();

        if (CharBuffer != null)
        {
            BufferUtils.ReturnBuffer(arrayPool, CharBuffer);
            CharBuffer = null;
        }

        if (CloseInput)
        {
            reader?.Close();
        }

        stringBuffer.Clear(arrayPool);
    }

    /// <summary>
    /// Gets a value indicating whether the class can return line information.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if <see cref="JsonTextReader.LineNumber"/> and <see cref="JsonTextReader.LinePosition"/> can be provided; otherwise, <c>false</c>.
    /// </returns>
    public bool HasLineInfo()
    {
        return true;
    }

    /// <summary>
    /// Gets the current line number.
    /// </summary>
    public int LineNumber
    {
        get
        {
            if (CurrentState == State.Start && LinePosition == 0 && TokenType != JsonToken.Comment)
            {
                return 0;
            }

            return lineNumber;
        }
    }

    /// <summary>
    /// Gets the current line position.
    /// </summary>
    public int LinePosition => CharPos - lineStartPos;
}