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

public partial class JsonTextReader
{
    // It's not safe to perform the async methods here in a derived class as if the synchronous equivalent
    // has been overriden then the asychronous method will no longer be doing the same operation
    readonly bool safeAsync;

    /// <summary>
    /// Asynchronously reads the next JSON token from the source.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
    /// property returns <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task<bool> ReadAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoReadAsync(cancellation) : base.ReadAsync(cancellation);
    }

    Task<bool> DoReadAsync(CancellationToken cancellation)
    {
        EnsureBuffer();

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
                    return ParseValueAsync(cancellation);
                case State.Object:
                case State.ObjectStart:
                    return ParseObjectAsync(cancellation);
                case State.PostValue:
                    var task = ParsePostValueAsync(false, cancellation);
                    if (task.IsCompletedSucessfully())
                    {
                        if (task.Result)
                        {
                            return AsyncUtils.True;
                        }
                    }
                    else
                    {
                        return DoReadAsync(task, cancellation);
                    }
                    break;
                case State.Finished:
                    return ReadFromFinishedAsync(cancellation);
                default:
                    throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
            }
        }
    }

    async Task<bool> DoReadAsync(Task<bool> task, CancellationToken cancellation)
    {
        var result = await task.ConfigureAwait(false);
        if (result)
        {
            return true;
        }
        return await DoReadAsync(cancellation).ConfigureAwait(false);
    }

    async Task<bool> ParsePostValueAsync(bool ignoreComments, CancellationToken cancellation)
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
                        if (await ReadDataAsync(false, cancellation).ConfigureAwait(false) == 0)
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
                    await ParseCommentAsync(!ignoreComments, cancellation).ConfigureAwait(false);
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
                    await ProcessCarriageReturnAsync(false, cancellation).ConfigureAwait(false);
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

    async Task<bool> ReadFromFinishedAsync(CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (await EnsureCharsAsync(0, false, cancellation).ConfigureAwait(false))
        {
            await EatWhitespaceAsync(cancellation).ConfigureAwait(false);
            if (isEndOfFile)
            {
                SetToken(JsonToken.None);
                return false;
            }

            if (CharBuffer[CharPos] == '/')
            {
                await ParseCommentAsync(true, cancellation).ConfigureAwait(false);
                return true;
            }

            throw JsonReaderException.Create(this, $"Additional text encountered after finished reading JSON content: {CharBuffer[CharPos]}.");
        }

        SetToken(JsonToken.None);
        return false;
    }

    Task<int> ReadDataAsync(bool append, CancellationToken cancellation)
    {
        return ReadDataAsync(append, 0, cancellation);
    }

    async Task<int> ReadDataAsync(bool append, int charsRequired, CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (isEndOfFile)
        {
            return 0;
        }

        PrepareBufferForReadData(append, charsRequired);

        var charsRead = await reader.ReadAsync(CharBuffer, charsUsed, CharBuffer.Length - charsUsed - 1, cancellation).ConfigureAwait(false);

        charsUsed += charsRead;

        if (charsRead == 0)
        {
            isEndOfFile = true;
        }

        CharBuffer[charsUsed] = '\0';
        return charsRead;
    }

    async Task<bool> ParseValueAsync(CancellationToken cancellation)
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
                        if (await ReadDataAsync(false, cancellation).ConfigureAwait(false) == 0)
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
                    await ParseStringAsync(currentChar, ReadType.Read, cancellation).ConfigureAwait(false);
                    return true;
                case 't':
                    await ParseTrueAsync(cancellation).ConfigureAwait(false);
                    return true;
                case 'f':
                    await ParseFalseAsync(cancellation).ConfigureAwait(false);
                    return true;
                case 'n':
                    if (await EnsureCharsAsync(1, true, cancellation).ConfigureAwait(false))
                    {
                        switch (CharBuffer[CharPos + 1])
                        {
                            case 'u':
                                await ParseNullAsync(cancellation).ConfigureAwait(false);
                                break;
                            case 'e':
                                await ParseConstructorAsync(cancellation).ConfigureAwait(false);
                                break;
                            default:
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
                    await ParseNumberNaNAsync(ReadType.Read, cancellation).ConfigureAwait(false);
                    return true;
                case 'I':
                    await ParseNumberPositiveInfinityAsync(ReadType.Read, cancellation).ConfigureAwait(false);
                    return true;
                case '-':
                    if (await EnsureCharsAsync(1, true, cancellation).ConfigureAwait(false) && CharBuffer[CharPos + 1] == 'I')
                    {
                        await ParseNumberNegativeInfinityAsync(ReadType.Read, cancellation).ConfigureAwait(false);
                    }
                    else
                    {
                        await ParseNumberAsync(ReadType.Read, cancellation).ConfigureAwait(false);
                    }
                    return true;
                case '/':
                    await ParseCommentAsync(true, cancellation).ConfigureAwait(false);
                    return true;
                case 'u':
                    await ParseUndefinedAsync(cancellation).ConfigureAwait(false);
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
                    await ProcessCarriageReturnAsync(false, cancellation).ConfigureAwait(false);
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
                        await ParseNumberAsync(ReadType.Read, cancellation).ConfigureAwait(false);
                        return true;
                    }

                    throw CreateUnexpectedCharacterException(currentChar);
            }
        }
    }

    async Task ReadStringIntoBufferAsync(char quote, CancellationToken cancellation)
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

                        if (await ReadDataAsync(true, cancellation).ConfigureAwait(false) == 0)
                        {
                            CharPos = charPos;
                            throw JsonReaderException.Create(this, $"Unterminated string. Expected delimiter: {quote}.");
                        }
                    }

                    break;
                case '\\':
                    CharPos = charPos;
                    if (!await EnsureCharsAsync(0, true, cancellation).ConfigureAwait(false))
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
                            writeChar = await ParseUnicodeAsync(cancellation).ConfigureAwait(false);

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
                                    if (await EnsureCharsAsync(2, true, cancellation).ConfigureAwait(false) && CharBuffer[CharPos] == '\\' && CharBuffer[CharPos + 1] == 'u')
                                    {
                                        var highSurrogate = writeChar;

                                        CharPos += 2;
                                        writeChar = await ParseUnicodeAsync(cancellation).ConfigureAwait(false);

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
                    await ProcessCarriageReturnAsync(true, cancellation).ConfigureAwait(false);
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

    Task ProcessCarriageReturnAsync(bool append, CancellationToken cancellation)
    {
        CharPos++;

        var task = EnsureCharsAsync(1, append, cancellation);
        if (task.IsCompletedSucessfully())
        {
            SetNewLine(task.Result);
            return AsyncUtils.CompletedTask;
        }

        return ProcessCarriageReturnAsync(task);
    }

    async Task ProcessCarriageReturnAsync(Task<bool> task)
    {
        SetNewLine(await task.ConfigureAwait(false));
    }

    async Task<char> ParseUnicodeAsync(CancellationToken cancellation)
    {
        return ConvertUnicode(await EnsureCharsAsync(4, true, cancellation).ConfigureAwait(false));
    }

    Task<bool> EnsureCharsAsync(int relativePosition, bool append, CancellationToken cancellation)
    {
        if (CharPos + relativePosition < charsUsed)
        {
            return AsyncUtils.True;
        }

        if (isEndOfFile)
        {
            return AsyncUtils.False;
        }

        return ReadCharsAsync(relativePosition, append, cancellation);
    }

    async Task<bool> ReadCharsAsync(int relativePosition, bool append, CancellationToken cancellation)
    {
        var charsRequired = CharPos + relativePosition - charsUsed + 1;

        // it is possible that the TextReader doesn't return all data at once
        // repeat read until the required text is returned or the reader is out of content
        do
        {
            var charsRead = await ReadDataAsync(append, charsRequired, cancellation).ConfigureAwait(false);

            // no more content
            if (charsRead == 0)
            {
                return false;
            }

            charsRequired -= charsRead;
        } while (charsRequired > 0);

        return true;
    }

    async Task<bool> ParseObjectAsync(CancellationToken cancellation)
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
                        if (await ReadDataAsync(false, cancellation).ConfigureAwait(false) == 0)
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
                    await ParseCommentAsync(true, cancellation).ConfigureAwait(false);
                    return true;
                case StringUtils.CarriageReturn:
                    await ProcessCarriageReturnAsync(false, cancellation).ConfigureAwait(false);
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
                        return await ParsePropertyAsync(cancellation).ConfigureAwait(false);
                    }

                    break;
            }
        }
    }

    async Task ParseCommentAsync(bool setToken, CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        // should have already parsed / character before reaching this method
        CharPos++;

        if (!await EnsureCharsAsync(1, false, cancellation).ConfigureAwait(false))
        {
            throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
        }

        bool singlelineComment;

        if (CharBuffer[CharPos] == '*')
        {
            singlelineComment = false;
        }
        else if (CharBuffer[CharPos] == '/')
        {
            singlelineComment = true;
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
                        if (await ReadDataAsync(true, cancellation).ConfigureAwait(false) == 0)
                        {
                            if (!singlelineComment)
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

                    if (!singlelineComment)
                    {
                        if (await EnsureCharsAsync(0, true, cancellation).ConfigureAwait(false))
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
                    if (singlelineComment)
                    {
                        EndComment(setToken, initialPosition, CharPos);
                        return;
                    }

                    await ProcessCarriageReturnAsync(true, cancellation).ConfigureAwait(false);
                    break;
                case StringUtils.LineFeed:
                    if (singlelineComment)
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

    async Task EatWhitespaceAsync(CancellationToken cancellation)
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
                        if (await ReadDataAsync(false, cancellation).ConfigureAwait(false) == 0)
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
                    await ProcessCarriageReturnAsync(false, cancellation).ConfigureAwait(false);
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

    async Task ParseStringAsync(char quote, ReadType readType, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        CharPos++;

        ShiftBufferIfNeeded();
        await ReadStringIntoBufferAsync(quote, cancellation).ConfigureAwait(false);
        ParseReadString(quote, readType);
    }

    async Task<bool> MatchValueAsync(string value, CancellationToken cancellation)
    {
        return MatchValue(await EnsureCharsAsync(value.Length - 1, true, cancellation).ConfigureAwait(false), value);
    }

    async Task<bool> MatchValueWithTrailingSeparatorAsync(string value, CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        // will match value and then move to the next character, checking that it is a separator character
        if (!await MatchValueAsync(value, cancellation).ConfigureAwait(false))
        {
            return false;
        }

        if (!await EnsureCharsAsync(0, false, cancellation).ConfigureAwait(false))
        {
            return true;
        }

        return IsSeparator(CharBuffer[CharPos]) || CharBuffer[CharPos] == '\0';
    }

    async Task MatchAndSetAsync(string value, JsonToken newToken, object? tokenValue, CancellationToken cancellation)
    {
        if (await MatchValueWithTrailingSeparatorAsync(value, cancellation).ConfigureAwait(false))
        {
            SetToken(newToken, tokenValue);
        }
        else
        {
            throw JsonReaderException.Create(this, $"Error parsing {newToken.ToString().ToLowerInvariant()} value.");
        }
    }

    Task ParseTrueAsync(CancellationToken cancellation)
    {
        return MatchAndSetAsync(JsonConvert.True, JsonToken.Boolean, true, cancellation);
    }

    Task ParseFalseAsync(CancellationToken cancellation)
    {
        return MatchAndSetAsync(JsonConvert.False, JsonToken.Boolean, false, cancellation);
    }

    Task ParseNullAsync(CancellationToken cancellation)
    {
        return MatchAndSetAsync(JsonConvert.Null, JsonToken.Null, null, cancellation);
    }

    async Task ParseConstructorAsync(CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (await MatchValueWithTrailingSeparatorAsync("new", cancellation).ConfigureAwait(false))
        {
            await EatWhitespaceAsync(cancellation).ConfigureAwait(false);

            var initialPosition = CharPos;
            int endPosition;

            while (true)
            {
                var currentChar = CharBuffer[CharPos];
                if (currentChar == '\0')
                {
                    if (charsUsed == CharPos)
                    {
                        if (await ReadDataAsync(true, cancellation).ConfigureAwait(false) == 0)
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
                    await ProcessCarriageReturnAsync(true, cancellation).ConfigureAwait(false);
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

            await EatWhitespaceAsync(cancellation).ConfigureAwait(false);

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

    async Task<object> ParseNumberNaNAsync(ReadType readType, CancellationToken cancellation)
    {
        return ParseNumberNaN(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.NaN, cancellation).ConfigureAwait(false));
    }

    async Task<object> ParseNumberPositiveInfinityAsync(ReadType readType, CancellationToken cancellation)
    {
        return ParseNumberPositiveInfinity(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.PositiveInfinity, cancellation).ConfigureAwait(false));
    }

    async Task<object> ParseNumberNegativeInfinityAsync(ReadType readType, CancellationToken cancellation)
    {
        return ParseNumberNegativeInfinity(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.NegativeInfinity, cancellation).ConfigureAwait(false));
    }

    async Task ParseNumberAsync(ReadType readType, CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        ShiftBufferIfNeeded();

        var firstChar = CharBuffer[CharPos];
        var initialPosition = CharPos;

        await ReadNumberIntoBufferAsync(cancellation).ConfigureAwait(false);

        ParseReadNumber(readType, firstChar, initialPosition);
    }

    Task ParseUndefinedAsync(CancellationToken cancellation)
    {
        return MatchAndSetAsync(JsonConvert.Undefined, JsonToken.Undefined, null, cancellation);
    }

    async Task<bool> ParsePropertyAsync(CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        var firstChar = CharBuffer[CharPos];
        char quoteChar;

        if (firstChar is '"' or '\'')
        {
            CharPos++;
            quoteChar = firstChar;
            ShiftBufferIfNeeded();
            await ReadStringIntoBufferAsync(quoteChar, cancellation).ConfigureAwait(false);
        }
        else if (ValidIdentifierChar(firstChar))
        {
            quoteChar = '\0';
            ShiftBufferIfNeeded();
            await ParseUnquotedPropertyAsync(cancellation).ConfigureAwait(false);
        }
        else
        {
            throw JsonReaderException.Create(this, $"Invalid property identifier character: {CharBuffer[CharPos]}.");
        }

        string propertyName;

        if (PropertyNameTable != null)
        {
            propertyName = PropertyNameTable.Get(stringReference.Chars, stringReference.StartIndex, stringReference.Length)
                           // no match in name table
                           ?? stringReference.ToString();
        }
        else
        {
            propertyName = stringReference.ToString();
        }

        await EatWhitespaceAsync(cancellation).ConfigureAwait(false);

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

    async Task ReadNumberIntoBufferAsync(CancellationToken cancellation)
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
                    if (await ReadDataAsync(true, cancellation).ConfigureAwait(false) == 0)
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

    async Task ParseUnquotedPropertyAsync(CancellationToken cancellation)
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
                    if (await ReadDataAsync(true, cancellation).ConfigureAwait(false) == 0)
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

    async Task<bool> ReadNullCharAsync(CancellationToken cancellation)
    {
        if (charsUsed == CharPos)
        {
            if (await ReadDataAsync(false, cancellation).ConfigureAwait(false) == 0)
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

    async Task HandleNullAsync(CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (await EnsureCharsAsync(1, true, cancellation).ConfigureAwait(false))
        {
            if (CharBuffer[CharPos + 1] == 'u')
            {
                await ParseNullAsync(cancellation).ConfigureAwait(false);
                return;
            }

            CharPos += 2;
            throw CreateUnexpectedCharacterException(CharBuffer[CharPos - 1]);
        }

        CharPos = charsUsed;
        throw CreateUnexpectedEndException();
    }

    async Task ReadFinishedAsync(CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(CharBuffer != null);

        if (await EnsureCharsAsync(0, false, cancellation).ConfigureAwait(false))
        {
            await EatWhitespaceAsync(cancellation).ConfigureAwait(false);
            if (isEndOfFile)
            {
                SetToken(JsonToken.None);
                return;
            }

            if (CharBuffer[CharPos] == '/')
            {
                await ParseCommentAsync(false, cancellation).ConfigureAwait(false);
            }
            else
            {
                throw JsonReaderException.Create(this, $"Additional text encountered after finished reading JSON content: {CharBuffer[CharPos]}.");
            }
        }

        SetToken(JsonToken.None);
    }

    async Task<object?> ReadStringValueAsync(ReadType readType, CancellationToken cancellation)
    {
        EnsureBuffer();
        MiscellaneousUtils.Assert(CharBuffer != null);

        switch (currentState)
        {
            case State.PostValue:
                if (await ParsePostValueAsync(true, cancellation).ConfigureAwait(false))
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
                            if (await ReadNullCharAsync(cancellation).ConfigureAwait(false))
                            {
                                SetToken(JsonToken.None, null, false);
                                return null;
                            }

                            break;
                        case '"':
                        case '\'':
                            await ParseStringAsync(currentChar, readType, cancellation).ConfigureAwait(false);
                            return FinishReadQuotedStringValue(readType);
                        case '-':
                            if (await EnsureCharsAsync(1, true, cancellation).ConfigureAwait(false) && CharBuffer[CharPos + 1] == 'I')
                            {
                                return ParseNumberNegativeInfinity(readType);
                            }

                            await ParseNumberAsync(readType, cancellation).ConfigureAwait(false);
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

                            await ParseNumberAsync(ReadType.ReadAsString, cancellation).ConfigureAwait(false);
                            return Value;
                        case 't':
                        case 'f':
                            if (readType != ReadType.ReadAsString)
                            {
                                CharPos++;
                                throw CreateUnexpectedCharacterException(currentChar);
                            }

                            var expected = currentChar == 't' ? JsonConvert.True : JsonConvert.False;
                            if (!await MatchValueWithTrailingSeparatorAsync(expected, cancellation).ConfigureAwait(false))
                            {
                                throw CreateUnexpectedCharacterException(CharBuffer[CharPos]);
                            }

                            SetToken(JsonToken.String, expected);
                            return expected;
                        case 'I':
                            return await ParseNumberPositiveInfinityAsync(readType, cancellation).ConfigureAwait(false);
                        case 'N':
                            return await ParseNumberNaNAsync(readType, cancellation).ConfigureAwait(false);
                        case 'n':
                            await HandleNullAsync(cancellation).ConfigureAwait(false);
                            return null;
                        case '/':
                            await ParseCommentAsync(false, cancellation).ConfigureAwait(false);
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
                            await ProcessCarriageReturnAsync(false, cancellation).ConfigureAwait(false);
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
                await ReadFinishedAsync(cancellation).ConfigureAwait(false);
                return null;
            default:
                throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
        }
    }

    async Task<object?> ReadNumberValueAsync(ReadType readType, CancellationToken cancellation)
    {
        EnsureBuffer();
        MiscellaneousUtils.Assert(CharBuffer != null);

        switch (currentState)
        {
            case State.PostValue:
                if (await ParsePostValueAsync(true, cancellation).ConfigureAwait(false))
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
                            if (await ReadNullCharAsync(cancellation).ConfigureAwait(false))
                            {
                                SetToken(JsonToken.None, null, false);
                                return null;
                            }

                            break;
                        case '"':
                        case '\'':
                            await ParseStringAsync(currentChar, readType, cancellation).ConfigureAwait(false);
                            return FinishReadQuotedNumber(readType);
                        case 'n':
                            await HandleNullAsync(cancellation).ConfigureAwait(false);
                            return null;
                        case 'N':
                            return await ParseNumberNaNAsync(readType, cancellation).ConfigureAwait(false);
                        case 'I':
                            return await ParseNumberPositiveInfinityAsync(readType, cancellation).ConfigureAwait(false);
                        case '-':
                            if (await EnsureCharsAsync(1, true, cancellation).ConfigureAwait(false) && CharBuffer[CharPos + 1] == 'I')
                            {
                                return await ParseNumberNegativeInfinityAsync(readType, cancellation).ConfigureAwait(false);
                            }

                            await ParseNumberAsync(readType, cancellation).ConfigureAwait(false);
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
                            await ParseNumberAsync(readType, cancellation).ConfigureAwait(false);
                            return Value;
                        case '/':
                            await ParseCommentAsync(false, cancellation).ConfigureAwait(false);
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
                            await ProcessCarriageReturnAsync(false, cancellation).ConfigureAwait(false);
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
                await ReadFinishedAsync(cancellation).ConfigureAwait(false);
                return null;
            default:
                throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
        }
    }

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="bool"/>.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
    /// property returns the <see cref="Nullable{T}"/> of <see cref="bool"/>. This result will be <c>null</c> at the end of an array.</returns>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task<bool?> ReadAsBooleanAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoReadAsBooleanAsync(cancellation) : base.ReadAsBooleanAsync(cancellation);
    }

    internal async Task<bool?> DoReadAsBooleanAsync(CancellationToken cancellation)
    {
        EnsureBuffer();
        MiscellaneousUtils.Assert(CharBuffer != null);

        switch (currentState)
        {
            case State.PostValue:
                if (await ParsePostValueAsync(true, cancellation).ConfigureAwait(false))
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
                            if (await ReadNullCharAsync(cancellation).ConfigureAwait(false))
                            {
                                SetToken(JsonToken.None, null, false);
                                return null;
                            }

                            break;
                        case '"':
                        case '\'':
                            await ParseStringAsync(currentChar, ReadType.Read, cancellation).ConfigureAwait(false);
                            return ReadBooleanString(stringReference.ToString());
                        case 'n':
                            await HandleNullAsync(cancellation).ConfigureAwait(false);
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
                            await ParseNumberAsync(ReadType.Read, cancellation).ConfigureAwait(false);
                            bool b;
                            if (Value is BigInteger i)
                            {
                                b = i != 0;
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
                            if (!await MatchValueWithTrailingSeparatorAsync(isTrue ? JsonConvert.True : JsonConvert.False, cancellation).ConfigureAwait(false))
                            {
                                throw CreateUnexpectedCharacterException(CharBuffer[CharPos]);
                            }

                            SetToken(JsonToken.Boolean, isTrue);
                            return isTrue;
                        case '/':
                            await ParseCommentAsync(false, cancellation).ConfigureAwait(false);
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
                            await ProcessCarriageReturnAsync(false, cancellation).ConfigureAwait(false);
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
                await ReadFinishedAsync(cancellation).ConfigureAwait(false);
                return null;
            default:
                throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
        }
    }

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="byte"/>[].
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
    /// property returns the <see cref="byte"/>[]. This result will be <c>null</c> at the end of an array.</returns>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task<byte[]?> ReadAsBytesAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoReadAsBytesAsync(cancellation) : base.ReadAsBytesAsync(cancellation);
    }

    internal async Task<byte[]?> DoReadAsBytesAsync(CancellationToken cancellation)
    {
        EnsureBuffer();
        MiscellaneousUtils.Assert(CharBuffer != null);

        var isWrapped = false;

        switch (currentState)
        {
            case State.PostValue:
                if (await ParsePostValueAsync(true, cancellation).ConfigureAwait(false))
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
                            if (await ReadNullCharAsync(cancellation).ConfigureAwait(false))
                            {
                                SetToken(JsonToken.None, null, false);
                                return null;
                            }

                            break;
                        case '"':
                        case '\'':
                            await ParseStringAsync(currentChar, ReadType.ReadAsBytes, cancellation).ConfigureAwait(false);
                            var data = (byte[]?)Value;
                            if (isWrapped)
                            {
                                await ReaderReadAndAssertAsync(cancellation).ConfigureAwait(false);
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
                            await ReadIntoWrappedTypeObjectAsync(cancellation).ConfigureAwait(false);
                            isWrapped = true;
                            break;
                        case '[':
                            CharPos++;
                            SetToken(JsonToken.StartArray);
                            return await ReadArrayIntoByteArrayAsync(cancellation).ConfigureAwait(false);
                        case 'n':
                            await HandleNullAsync(cancellation).ConfigureAwait(false);
                            return null;
                        case '/':
                            await ParseCommentAsync(false, cancellation).ConfigureAwait(false);
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
                            await ProcessCarriageReturnAsync(false, cancellation).ConfigureAwait(false);
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
                await ReadFinishedAsync(cancellation).ConfigureAwait(false);
                return null;
            default:
                throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
        }
    }

    async Task ReadIntoWrappedTypeObjectAsync(CancellationToken cancellation)
    {
        await ReaderReadAndAssertAsync(cancellation).ConfigureAwait(false);
        if (Value != null && Value.ToString() == JsonTypeReflector.TypePropertyName)
        {
            await ReaderReadAndAssertAsync(cancellation).ConfigureAwait(false);
            if (Value != null && Value.ToString()!.StartsWith("System.Byte[]", StringComparison.Ordinal))
            {
                await ReaderReadAndAssertAsync(cancellation).ConfigureAwait(false);
                if (Value.ToString() == JsonTypeReflector.ValuePropertyName)
                {
                    return;
                }
            }
        }

        throw JsonReaderException.Create(this, $"Error reading bytes. Unexpected token: {JsonToken.StartObject}.");
    }

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="DateTime"/>.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
    /// property returns the <see cref="Nullable{T}"/> of <see cref="DateTime"/>. This result will be <c>null</c> at the end of an array.</returns>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoReadAsDateTimeAsync(cancellation) : base.ReadAsDateTimeAsync(cancellation);
    }

    internal async Task<DateTime?> DoReadAsDateTimeAsync(CancellationToken cancellation)
    {
        return (DateTime?)await ReadStringValueAsync(ReadType.ReadAsDateTime, cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
    /// property returns the <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>. This result will be <c>null</c> at the end of an array.</returns>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoReadAsDateTimeOffsetAsync(cancellation) : base.ReadAsDateTimeOffsetAsync(cancellation);
    }

    internal async Task<DateTimeOffset?> DoReadAsDateTimeOffsetAsync(CancellationToken cancellation)
    {
        return (DateTimeOffset?)await ReadStringValueAsync(ReadType.ReadAsDateTimeOffset, cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="decimal"/>.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
    /// property returns the <see cref="Nullable{T}"/> of <see cref="decimal"/>. This result will be <c>null</c> at the end of an array.</returns>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoReadAsDecimalAsync(cancellation) : base.ReadAsDecimalAsync(cancellation);
    }

    internal async Task<decimal?> DoReadAsDecimalAsync(CancellationToken cancellation)
    {
        return (decimal?)await ReadNumberValueAsync(ReadType.ReadAsDecimal, cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="double"/>.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
    /// property returns the <see cref="Nullable{T}"/> of <see cref="double"/>. This result will be <c>null</c> at the end of an array.</returns>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task<double?> ReadAsDoubleAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoReadAsDoubleAsync(cancellation) : base.ReadAsDoubleAsync(cancellation);
    }

    internal async Task<double?> DoReadAsDoubleAsync(CancellationToken cancellation)
    {
        return (double?)await ReadNumberValueAsync(ReadType.ReadAsDouble, cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="int"/>.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
    /// property returns the <see cref="Nullable{T}"/> of <see cref="int"/>. This result will be <c>null</c> at the end of an array.</returns>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task<int?> ReadAsInt32Async(CancellationToken cancellation = default)
    {
        return safeAsync ? DoReadAsInt32Async(cancellation) : base.ReadAsInt32Async(cancellation);
    }

    internal async Task<int?> DoReadAsInt32Async(CancellationToken cancellation)
    {
        return (int?)await ReadNumberValueAsync(ReadType.ReadAsInt32, cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="string"/>.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
    /// property returns the <see cref="string"/>. This result will be <c>null</c> at the end of an array.</returns>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task<string?> ReadAsStringAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoReadAsStringAsync(cancellation) : base.ReadAsStringAsync(cancellation);
    }

    internal async Task<string?> DoReadAsStringAsync(CancellationToken cancellation)
    {
        return (string?)await ReadStringValueAsync(ReadType.ReadAsString, cancellation).ConfigureAwait(false);
    }
}