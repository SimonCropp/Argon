// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable RedundantSuppressNullableWarningExpression
namespace Argon;

public partial class JsonTextReader
{
    // It's not safe to perform the async methods here in a derived class as if the synchronous equivalent
    // has been overriden then the asychronous method will no longer be doing the same operation
    readonly bool safeAsync;

    /// <summary>
    /// Asynchronously reads the next JSON token from the source.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
    /// </returns>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task<bool> ReadAsync(Cancellation cancellation = default)
    {
        if (safeAsync)
        {
            return DoReadAsync(cancellation);
        }

        return base.ReadAsync(cancellation);
    }

    Task<bool> DoReadAsync(Cancellation cancellation)
    {
        while (true)
        {
            switch (currentState)
            {
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                    return ParseValueAsync(cancellation);
                case State.Object:
                case State.ObjectStart:
                    return ParseObjectAsync(cancellation);
                case State.PostValue:
                    var task = ParsePostValueAsync(false, cancellation);
                    if (task.IsCompletedSuccessfully())
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

    async Task<bool> DoReadAsync(Task<bool> task, Cancellation cancellation)
    {
        var result = await task.ConfigureAwait(false);
        if (result)
        {
            return true;
        }

        return await DoReadAsync(cancellation).ConfigureAwait(false);
    }

    async Task<bool> ParsePostValueAsync(bool ignoreComments, Cancellation cancellation)
    {
        while (true)
        {
            var currentChar = charBuffer[charPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == charPos)
                    {
                        if (await ReadDataAsync(false, cancellation).ConfigureAwait(false) == 0)
                        {
                            currentState = State.Finished;
                            return false;
                        }
                    }
                    else
                    {
                        charPos++;
                    }

                    break;
                case '}':
                    charPos++;
                    SetToken(JsonToken.EndObject);
                    return true;
                case ']':
                    charPos++;
                    SetToken(JsonToken.EndArray);
                    return true;
                case '/':
                    await ParseCommentAsync(!ignoreComments, cancellation).ConfigureAwait(false);
                    if (!ignoreComments)
                    {
                        return true;
                    }

                    break;
                case ',':
                    charPos++;

                    // finished parsing
                    SetStateBasedOnCurrent();
                    return false;
                case ' ':
                case StringUtils.Tab:

                    // eat
                    charPos++;
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
                        charPos++;
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

    async Task<bool> ReadFromFinishedAsync(Cancellation cancellation)
    {
        if (await EnsureCharsAsync(0, false, cancellation).ConfigureAwait(false))
        {
            await EatWhitespaceAsync(cancellation).ConfigureAwait(false);
            if (isEndOfFile)
            {
                SetToken(JsonToken.None);
                return false;
            }

            if (charBuffer[charPos] == '/')
            {
                await ParseCommentAsync(true, cancellation).ConfigureAwait(false);
                return true;
            }

            throw JsonReaderException.Create(this, $"Additional text encountered after finished reading JSON content: {charBuffer[charPos]}.");
        }

        SetToken(JsonToken.None);
        return false;
    }

    Task<int> ReadDataAsync(bool append, Cancellation cancellation) =>
        ReadDataAsync(append, 0, cancellation);

    async Task<int> ReadDataAsync(bool append, int charsRequired, Cancellation cancellation)
    {
        if (isEndOfFile)
        {
            return 0;
        }

        PrepareBufferForReadData(append, charsRequired);

        var charsRead = await reader.ReadAsync(charBuffer, charsUsed, charBuffer.Length - charsUsed - 1, cancellation).ConfigureAwait(false);

        charsUsed += charsRead;

        if (charsRead == 0)
        {
            isEndOfFile = true;
        }

        charBuffer[charsUsed] = '\0';
        return charsRead;
    }

    async Task<bool> ParseValueAsync(Cancellation cancellation)
    {
        while (true)
        {
            var currentChar = charBuffer[charPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == charPos)
                    {
                        if (await ReadDataAsync(false, cancellation).ConfigureAwait(false) == 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        charPos++;
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
                        switch (charBuffer[charPos + 1])
                        {
                            case 'u':
                                await ParseNullAsync(cancellation).ConfigureAwait(false);
                                break;
                            default:
                                throw CreateUnexpectedCharacterException(charBuffer[charPos]);
                        }
                    }
                    else
                    {
                        charPos++;
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
                    if (await EnsureCharsAsync(1, true, cancellation).ConfigureAwait(false) && charBuffer[charPos + 1] == 'I')
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
                    charPos++;
                    SetToken(JsonToken.StartObject);
                    return true;
                case '[':
                    charPos++;
                    SetToken(JsonToken.StartArray);
                    return true;
                case ']':
                    charPos++;
                    SetToken(JsonToken.EndArray);
                    return true;
                case ',':

                    // don't increment position, the next call to read will handle comma
                    // this is done to handle multiple empty comma values
                    SetToken(JsonToken.Undefined);
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
                    charPos++;
                    break;
                default:
                    if (char.IsWhiteSpace(currentChar))
                    {
                        // eat
                        charPos++;
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

    async Task ReadStringIntoBufferAsync(char quote, Cancellation cancellation)
    {
        var charPos = this.charPos;
        var initialPosition = this.charPos;
        var lastWritePosition = this.charPos;
        stringBuffer.Position = 0;

        while (true)
        {
            switch (charBuffer[charPos++])
            {
                case '\0':
                    if (charsUsed == charPos - 1)
                    {
                        charPos--;

                        if (await ReadDataAsync(true, cancellation).ConfigureAwait(false) == 0)
                        {
                            this.charPos = charPos;
                            throw JsonReaderException.Create(this, $"Unterminated string. Expected delimiter: {quote}.");
                        }
                    }

                    break;
                case '\\':
                    this.charPos = charPos;
                    if (!await EnsureCharsAsync(0, true, cancellation).ConfigureAwait(false))
                    {
                        throw JsonReaderException.Create(this, $"Unterminated string. Expected delimiter: {quote}.");
                    }

                    // start of escape sequence
                    var escapeStartPos = charPos - 1;

                    var currentChar = charBuffer[charPos];
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
                            this.charPos = charPos;
                            writeChar = await ParseUnicodeAsync(cancellation).ConfigureAwait(false);

                            if (char.IsLowSurrogate(writeChar))
                            {
                                // low surrogate with no preceding high surrogate; this char is replaced
                                writeChar = unicodeReplacementChar;
                            }
                            else if (char.IsHighSurrogate(writeChar))
                            {
                                bool anotherHighSurrogate;

                                // loop for handling situations where there are multiple consecutive high surrogates
                                do
                                {
                                    anotherHighSurrogate = false;

                                    // potential start of a surrogate pair
                                    if (await EnsureCharsAsync(2, true, cancellation).ConfigureAwait(false) && charBuffer[this.charPos] == '\\' && charBuffer[this.charPos + 1] == 'u')
                                    {
                                        var highSurrogate = writeChar;

                                        this.charPos += 2;
                                        writeChar = await ParseUnicodeAsync(cancellation).ConfigureAwait(false);

                                        if (char.IsLowSurrogate(writeChar))
                                        {
                                            // a valid surrogate pair!
                                        }
                                        else if (char.IsHighSurrogate(writeChar))
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
                                        lastWritePosition = this.charPos;
                                    }
                                    else
                                    {
                                        // there are not enough remaining chars for the low surrogate or is not follow by unicode sequence
                                        // replace high surrogate and continue on as usual
                                        writeChar = unicodeReplacementChar;
                                    }
                                } while (anotherHighSurrogate);
                            }

                            charPos = this.charPos;
                            break;
                        default:
                            this.charPos = charPos;
                            throw JsonReaderException.Create(this, $"Bad JSON escape sequence: \\{currentChar}.");
                    }

                    EnsureBufferNotEmpty();
                    WriteCharToBuffer(writeChar, lastWritePosition, escapeStartPos);

                    lastWritePosition = charPos;
                    break;
                case StringUtils.CarriageReturn:
                    this.charPos = charPos - 1;
                    await ProcessCarriageReturnAsync(true, cancellation).ConfigureAwait(false);
                    charPos = this.charPos;
                    break;
                case StringUtils.LineFeed:
                    this.charPos = charPos - 1;
                    ProcessLineFeed();
                    charPos = this.charPos;
                    break;
                case '"':
                case '\'':
                    if (charBuffer[charPos - 1] == quote)
                    {
                        FinishReadStringIntoBuffer(charPos - 1, initialPosition, lastWritePosition);
                        return;
                    }

                    break;
            }
        }
    }

    Task ProcessCarriageReturnAsync(bool append, Cancellation cancellation)
    {
        charPos++;

        var task = EnsureCharsAsync(1, append, cancellation);
        if (task.IsCompletedSuccessfully())
        {
            SetNewLine(task.Result);
            return Task.CompletedTask;
        }

        return ProcessCarriageReturnAsync(task);
    }

    async Task ProcessCarriageReturnAsync(Task<bool> task) =>
        SetNewLine(await task.ConfigureAwait(false));

    async Task<char> ParseUnicodeAsync(Cancellation cancellation) =>
        ConvertUnicode(await EnsureCharsAsync(4, true, cancellation).ConfigureAwait(false));

    Task<bool> EnsureCharsAsync(int relativePosition, bool append, Cancellation cancellation)
    {
        if (charPos + relativePosition < charsUsed)
        {
            return AsyncUtils.True;
        }

        if (isEndOfFile)
        {
            return AsyncUtils.False;
        }

        return ReadCharsAsync(relativePosition, append, cancellation);
    }

    async Task<bool> ReadCharsAsync(int relativePosition, bool append, Cancellation cancellation)
    {
        var charsRequired = charPos + relativePosition - charsUsed + 1;

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

    async Task<bool> ParseObjectAsync(Cancellation cancellation)
    {
        while (true)
        {
            var currentChar = charBuffer[charPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == charPos)
                    {
                        if (await ReadDataAsync(false, cancellation).ConfigureAwait(false) == 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        charPos++;
                    }

                    break;
                case '}':
                    SetToken(JsonToken.EndObject);
                    charPos++;
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
                    charPos++;
                    break;
                default:
                    if (char.IsWhiteSpace(currentChar))
                    {
                        // eat
                        charPos++;
                    }
                    else
                    {
                        return await ParsePropertyAsync(cancellation).ConfigureAwait(false);
                    }

                    break;
            }
        }
    }

    async Task ParseCommentAsync(bool setToken, Cancellation cancellation)
    {
        // should have already parsed / character before reaching this method
        charPos++;

        if (!await EnsureCharsAsync(1, false, cancellation).ConfigureAwait(false))
        {
            throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
        }

        bool singlelineComment;

        if (charBuffer[charPos] == '*')
        {
            singlelineComment = false;
        }
        else if (charBuffer[charPos] == '/')
        {
            singlelineComment = true;
        }
        else
        {
            throw JsonReaderException.Create(this, $"Error parsing comment. Expected: *, got {charBuffer[charPos]}.");
        }

        charPos++;

        var initialPosition = charPos;

        while (true)
        {
            switch (charBuffer[charPos])
            {
                case '\0':
                    if (charsUsed == charPos)
                    {
                        if (await ReadDataAsync(true, cancellation).ConfigureAwait(false) == 0)
                        {
                            if (!singlelineComment)
                            {
                                throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
                            }

                            EndComment(setToken, initialPosition, charPos);
                            return;
                        }
                    }
                    else
                    {
                        charPos++;
                    }

                    break;
                case '*':
                    charPos++;

                    if (!singlelineComment)
                    {
                        if (await EnsureCharsAsync(0, true, cancellation).ConfigureAwait(false))
                        {
                            if (charBuffer[charPos] == '/')
                            {
                                EndComment(setToken, initialPosition, charPos - 1);

                                charPos++;
                                return;
                            }
                        }
                    }

                    break;
                case StringUtils.CarriageReturn:
                    if (singlelineComment)
                    {
                        EndComment(setToken, initialPosition, charPos);
                        return;
                    }

                    await ProcessCarriageReturnAsync(true, cancellation).ConfigureAwait(false);
                    break;
                case StringUtils.LineFeed:
                    if (singlelineComment)
                    {
                        EndComment(setToken, initialPosition, charPos);
                        return;
                    }

                    ProcessLineFeed();
                    break;
                default:
                    charPos++;
                    break;
            }
        }
    }

    async Task EatWhitespaceAsync(Cancellation cancellation)
    {
        while (true)
        {
            var currentChar = charBuffer[charPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == charPos)
                    {
                        if (await ReadDataAsync(false, cancellation).ConfigureAwait(false) == 0)
                        {
                            return;
                        }
                    }
                    else
                    {
                        charPos++;
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
                        charPos++;
                    }
                    else
                    {
                        return;
                    }

                    break;
            }
        }
    }

    async Task ParseStringAsync(char quote, ReadType readType, Cancellation cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        charPos++;

        ShiftBufferIfNeeded();
        await ReadStringIntoBufferAsync(quote, cancellation).ConfigureAwait(false);
        ParseReadString(quote, readType);
    }

    async Task<bool> MatchValueAsync(string value, Cancellation cancellation) =>
        MatchValue(await EnsureCharsAsync(value.Length - 1, true, cancellation).ConfigureAwait(false), value);

    async Task<bool> MatchValueWithTrailingSeparatorAsync(string value, Cancellation cancellation)
    {
        // will match value and then move to the next character, checking that it is a separator character
        if (!await MatchValueAsync(value, cancellation).ConfigureAwait(false))
        {
            return false;
        }

        if (!await EnsureCharsAsync(0, false, cancellation).ConfigureAwait(false))
        {
            return true;
        }

        return IsSeparator(charBuffer[charPos]) || charBuffer[charPos] == '\0';
    }

    async Task MatchAndSetAsync(string value, JsonToken newToken, object? tokenValue, Cancellation cancellation)
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

    Task ParseTrueAsync(Cancellation cancellation) =>
        MatchAndSetAsync(JsonConvert.True, JsonToken.Boolean, true, cancellation);

    Task ParseFalseAsync(Cancellation cancellation) =>
        MatchAndSetAsync(JsonConvert.False, JsonToken.Boolean, false, cancellation);

    Task ParseNullAsync(Cancellation cancellation) =>
        MatchAndSetAsync(JsonConvert.Null, JsonToken.Null, null, cancellation);

    async Task<object> ParseNumberNaNAsync(ReadType readType, Cancellation cancellation) =>
        ParseNumberNaN(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.NaN, cancellation).ConfigureAwait(false));

    async Task<object> ParseNumberPositiveInfinityAsync(ReadType readType, Cancellation cancellation) =>
        ParseNumberPositiveInfinity(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.PositiveInfinity, cancellation).ConfigureAwait(false));

    async Task<object> ParseNumberNegativeInfinityAsync(ReadType readType, Cancellation cancellation) =>
        ParseNumberNegativeInfinity(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.NegativeInfinity, cancellation).ConfigureAwait(false));

    async Task ParseNumberAsync(ReadType readType, Cancellation cancellation)
    {
        ShiftBufferIfNeeded();

        var firstChar = charBuffer[charPos];
        var initialPosition = charPos;

        await ReadNumberIntoBufferAsync(cancellation).ConfigureAwait(false);

        ParseReadNumber(readType, firstChar, initialPosition);
    }

    Task ParseUndefinedAsync(Cancellation cancellation) =>
        MatchAndSetAsync(JsonConvert.Undefined, JsonToken.Undefined, null, cancellation);

    async Task<bool> ParsePropertyAsync(Cancellation cancellation)
    {
        var firstChar = charBuffer[charPos];
        char quoteChar;

        if (firstChar is '"' or '\'')
        {
            charPos++;
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
            throw JsonReaderException.Create(this, $"Invalid property identifier character: {charBuffer[charPos]}.");
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

        if (charBuffer[charPos] != ':')
        {
            throw JsonReaderException.Create(this, $"Invalid character after parsing property name. Expected ':' but got: {charBuffer[charPos]}.");
        }

        charPos++;

        SetToken(JsonToken.PropertyName, propertyName);
        this.quoteChar = quoteChar;
        ClearRecentString();

        return true;
    }

    async Task ReadNumberIntoBufferAsync(Cancellation cancellation)
    {
        var charPos = this.charPos;

        while (true)
        {
            var currentChar = charBuffer[charPos];
            if (currentChar == '\0')
            {
                this.charPos = charPos;

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

    async Task ParseUnquotedPropertyAsync(Cancellation cancellation)
    {
        var initialPosition = charPos;

        // parse unquoted property name until whitespace or colon
        while (true)
        {
            var currentChar = charBuffer[charPos];
            if (currentChar == '\0')
            {
                if (charsUsed == charPos)
                {
                    if (await ReadDataAsync(true, cancellation).ConfigureAwait(false) == 0)
                    {
                        throw JsonReaderException.Create(this, "Unexpected end while parsing unquoted property name.");
                    }

                    continue;
                }

                stringReference = new(charBuffer, initialPosition, charPos - initialPosition);
                return;
            }

            if (ReadUnquotedPropertyReportIfDone(currentChar, initialPosition))
            {
                return;
            }
        }
    }

    async Task<bool> ReadNullCharAsync(Cancellation cancellation)
    {
        if (charsUsed == charPos)
        {
            if (await ReadDataAsync(false, cancellation).ConfigureAwait(false) == 0)
            {
                isEndOfFile = true;
                return true;
            }
        }
        else
        {
            charPos++;
        }

        return false;
    }

    async Task HandleNullAsync(Cancellation cancellation)
    {
        if (await EnsureCharsAsync(1, true, cancellation).ConfigureAwait(false))
        {
            if (charBuffer[charPos + 1] == 'u')
            {
                await ParseNullAsync(cancellation).ConfigureAwait(false);
                return;
            }

            charPos += 2;
            throw CreateUnexpectedCharacterException(charBuffer[charPos - 1]);
        }

        charPos = charsUsed;
        throw CreateUnexpectedEndException();
    }

    async Task ReadFinishedAsync(Cancellation cancellation)
    {
        if (await EnsureCharsAsync(0, false, cancellation).ConfigureAwait(false))
        {
            await EatWhitespaceAsync(cancellation).ConfigureAwait(false);
            if (isEndOfFile)
            {
                SetToken(JsonToken.None);
                return;
            }

            if (charBuffer[charPos] == '/')
            {
                await ParseCommentAsync(false, cancellation).ConfigureAwait(false);
            }
            else
            {
                throw JsonReaderException.Create(this, $"Additional text encountered after finished reading JSON content: {charBuffer[charPos]}.");
            }
        }

        SetToken(JsonToken.None);
    }

    async Task<object?> ReadStringValueAsync(ReadType readType, Cancellation cancellation)
    {
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
                while (true)
                {
                    var currentChar = charBuffer[charPos];

                    switch (currentChar)
                    {
                        case '\0':
                            if (await ReadNullCharAsync(cancellation).ConfigureAwait(false))
                            {
                                SetNoneToken();
                                return null;
                            }

                            break;
                        case '"':
                        case '\'':
                            await ParseStringAsync(currentChar, readType, cancellation).ConfigureAwait(false);
                            return FinishReadQuotedStringValue(readType);
                        case '-':
                            if (await EnsureCharsAsync(1, true, cancellation).ConfigureAwait(false) && charBuffer[charPos + 1] == 'I')
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
                                charPos++;
                                throw CreateUnexpectedCharacterException(currentChar);
                            }

                            await ParseNumberAsync(ReadType.ReadAsString, cancellation).ConfigureAwait(false);
                            return Value;
                        case 't':
                        case 'f':
                            if (readType != ReadType.ReadAsString)
                            {
                                charPos++;
                                throw CreateUnexpectedCharacterException(currentChar);
                            }

                            var expected = currentChar == 't' ? JsonConvert.True : JsonConvert.False;
                            if (!await MatchValueWithTrailingSeparatorAsync(expected, cancellation).ConfigureAwait(false))
                            {
                                throw CreateUnexpectedCharacterException(charBuffer[charPos]);
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
                            charPos++;
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
                            charPos++;
                            break;
                        default:
                            charPos++;

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

    async Task<object?> ReadNumberValueAsync(ReadType readType, Cancellation cancellation)
    {
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
                while (true)
                {
                    var currentChar = charBuffer[charPos];

                    switch (currentChar)
                    {
                        case '\0':
                            if (await ReadNullCharAsync(cancellation).ConfigureAwait(false))
                            {
                                SetNoneToken();
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
                            if (await EnsureCharsAsync(1, true, cancellation).ConfigureAwait(false) && charBuffer[charPos + 1] == 'I')
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
                            charPos++;
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
                            charPos++;
                            break;
                        default:
                            charPos++;

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
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="bool" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="bool" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task<bool?> ReadAsBooleanAsync(Cancellation cancellation = default)
    {
        if (safeAsync)
        {
            return DoReadAsBooleanAsync(cancellation);
        }

        return base.ReadAsBooleanAsync(cancellation);
    }

    async Task<bool?> DoReadAsBooleanAsync(Cancellation cancellation)
    {
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
                while (true)
                {
                    var currentChar = charBuffer[charPos];

                    switch (currentChar)
                    {
                        case '\0':
                            if (await ReadNullCharAsync(cancellation).ConfigureAwait(false))
                            {
                                SetNoneToken();
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
                                b = Convert.ToBoolean(Value, InvariantCulture);
                            }

                            SetToken(b);
                            return b;
                        case 't':
                        case 'f':
                            var isTrue = currentChar == 't';
                            if (!await MatchValueWithTrailingSeparatorAsync(isTrue ? JsonConvert.True : JsonConvert.False, cancellation).ConfigureAwait(false))
                            {
                                throw CreateUnexpectedCharacterException(charBuffer[charPos]);
                            }

                            SetToken(JsonToken.Boolean, BoxedPrimitives.Get(isTrue));
                            return isTrue;
                        case '/':
                            await ParseCommentAsync(false, cancellation).ConfigureAwait(false);
                            break;
                        case ',':
                            ProcessValueComma();
                            break;
                        case ']':
                            charPos++;
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
                            charPos++;
                            break;
                        default:
                            charPos++;

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
    /// Asynchronously reads the next JSON token from the source as a <see cref="byte" />[].
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="byte" />[]. This result will be <c>null</c> at the end of an array.
    /// </returns>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task<byte[]?> ReadAsBytesAsync(Cancellation cancellation = default)
    {
        if (safeAsync)
        {
            return DoReadAsBytesAsync(cancellation);
        }

        return base.ReadAsBytesAsync(cancellation);
    }

    async Task<byte[]?> DoReadAsBytesAsync(Cancellation cancellation)
    {
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
                while (true)
                {
                    var currentChar = charBuffer[charPos];

                    switch (currentChar)
                    {
                        case '\0':
                            if (await ReadNullCharAsync(cancellation).ConfigureAwait(false))
                            {
                                SetNoneToken();
                                return null;
                            }

                            break;
                        case '"':
                        case '\'':
                            await ParseStringAsync(currentChar, ReadType.ReadAsBytes, cancellation).ConfigureAwait(false);
                            var data = (byte[]?) Value;
                            if (isWrapped)
                            {
                                await ReaderReadAndAssertAsync(cancellation).ConfigureAwait(false);
                                if (TokenType != JsonToken.EndObject)
                                {
                                    throw JsonReaderException.Create(this, $"Error reading bytes. Unexpected token: {TokenType}.");
                                }

                                SetToken(data);
                            }

                            return data;
                        case '{':
                            charPos++;
                            SetToken(JsonToken.StartObject);
                            await ReadIntoWrappedTypeObjectAsync(cancellation).ConfigureAwait(false);
                            isWrapped = true;
                            break;
                        case '[':
                            charPos++;
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
                            charPos++;
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
                            charPos++;
                            break;
                        default:
                            charPos++;

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

    async Task ReadIntoWrappedTypeObjectAsync(Cancellation cancellation)
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
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="DateTime" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="DateTime" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task<DateTime?> ReadAsDateTimeAsync(Cancellation cancellation = default)
    {
        if (safeAsync)
        {
            return DoReadAsDateTimeAsync(cancellation);
        }

        return base.ReadAsDateTimeAsync(cancellation);
    }

    async Task<DateTime?> DoReadAsDateTimeAsync(Cancellation cancellation) =>
        (DateTime?) await ReadStringValueAsync(ReadType.ReadAsDateTime, cancellation).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="DateTimeOffset" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="DateTimeOffset" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(Cancellation cancellation = default)
    {
        if (safeAsync)
        {
            return DoReadAsDateTimeOffsetAsync(cancellation);
        }

        return base.ReadAsDateTimeOffsetAsync(cancellation);
    }

    async Task<DateTimeOffset?> DoReadAsDateTimeOffsetAsync(Cancellation cancellation) =>
        (DateTimeOffset?) await ReadStringValueAsync(ReadType.ReadAsDateTimeOffset, cancellation).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="decimal" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="decimal" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task<decimal?> ReadAsDecimalAsync(Cancellation cancellation = default) =>
        safeAsync switch
        {
            true => DoReadAsDecimalAsync(cancellation),
            _ => base.ReadAsDecimalAsync(cancellation)
        };

    async Task<decimal?> DoReadAsDecimalAsync(Cancellation cancellation) =>
        (decimal?) await ReadNumberValueAsync(ReadType.ReadAsDecimal, cancellation).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="double" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="double" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task<double?> ReadAsDoubleAsync(Cancellation cancellation = default)
    {
        if (safeAsync)
        {
            return DoReadAsDoubleAsync(cancellation);
        }

        return base.ReadAsDoubleAsync(cancellation);
    }

    async Task<double?> DoReadAsDoubleAsync(Cancellation cancellation) =>
        (double?) await ReadNumberValueAsync(ReadType.ReadAsDouble, cancellation).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="int" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="int" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task<int?> ReadAsInt32Async(Cancellation cancellation = default)
    {
        if (safeAsync)
        {
            return DoReadAsInt32Async(cancellation);
        }

        return base.ReadAsInt32Async(cancellation);
    }

    async Task<int?> DoReadAsInt32Async(Cancellation cancellation) =>
        (int?) await ReadNumberValueAsync(ReadType.ReadAsInt32, cancellation).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="string" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="string" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task<string?> ReadAsStringAsync(Cancellation cancellation = default)
    {
        if (safeAsync)
        {
            return DoReadAsStringAsync(cancellation);
        }

        return base.ReadAsStringAsync(cancellation);
    }

    async Task<string?> DoReadAsStringAsync(Cancellation cancellation) =>
        (string?) await ReadStringValueAsync(ReadType.ReadAsString, cancellation).ConfigureAwait(false);
}