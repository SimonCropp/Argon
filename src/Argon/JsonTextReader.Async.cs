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
    public override Task<bool> ReadAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoReadAsync(cancel);
        }

        return base.ReadAsync(cancel);
    }

    Task<bool> DoReadAsync(Cancel cancel)
    {
        while (true)
        {
            switch (currentState)
            {
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                    return ParseValueAsync(cancel);
                case State.Object:
                case State.ObjectStart:
                    return ParseObjectAsync(cancel);
                case State.PostValue:
                    var task = ParsePostValueAsync(false, cancel);
                    if (task.IsCompletedSuccessfully())
                    {
                        if (task.Result)
                        {
                            return AsyncUtils.True;
                        }
                    }
                    else
                    {
                        return DoReadAsync(task, cancel);
                    }

                    break;
                case State.Finished:
                    return ReadFromFinishedAsync(cancel);
                default:
                    throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
            }
        }
    }

    async Task<bool> DoReadAsync(Task<bool> task, Cancel cancel)
    {
        var result = await task.ConfigureAwait(false);
        if (result)
        {
            return true;
        }

        return await DoReadAsync(cancel).ConfigureAwait(false);
    }

    async Task<bool> ParsePostValueAsync(bool ignoreComments, Cancel cancel)
    {
        while (true)
        {
            var currentChar = charBuffer[charPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == charPos)
                    {
                        if (await ReadDataAsync(false, cancel).ConfigureAwait(false) == 0)
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
                    await ParseCommentAsync(!ignoreComments, cancel).ConfigureAwait(false);
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
                    await ProcessCarriageReturnAsync(false, cancel).ConfigureAwait(false);
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

    async Task<bool> ReadFromFinishedAsync(Cancel cancel)
    {
        if (await EnsureCharsAsync(0, false, cancel).ConfigureAwait(false))
        {
            await EatWhitespaceAsync(cancel).ConfigureAwait(false);
            if (isEndOfFile)
            {
                SetToken(JsonToken.None);
                return false;
            }

            if (charBuffer[charPos] == '/')
            {
                await ParseCommentAsync(true, cancel).ConfigureAwait(false);
                return true;
            }

            throw JsonReaderException.Create(this, $"Additional text encountered after finished reading JSON content: {charBuffer[charPos]}.");
        }

        SetToken(JsonToken.None);
        return false;
    }

    Task<int> ReadDataAsync(bool append, Cancel cancel) =>
        ReadDataAsync(append, 0, cancel);

    async Task<int> ReadDataAsync(bool append, int charsRequired, Cancel cancel)
    {
        if (isEndOfFile)
        {
            return 0;
        }

        PrepareBufferForReadData(append, charsRequired);

        var charsRead = await reader.ReadAsync(charBuffer, charsUsed, charBuffer.Length - charsUsed - 1, cancel).ConfigureAwait(false);

        charsUsed += charsRead;

        if (charsRead == 0)
        {
            isEndOfFile = true;
        }

        charBuffer[charsUsed] = '\0';
        return charsRead;
    }

    async Task<bool> ParseValueAsync(Cancel cancel)
    {
        while (true)
        {
            var currentChar = charBuffer[charPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == charPos)
                    {
                        if (await ReadDataAsync(false, cancel).ConfigureAwait(false) == 0)
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
                    await ParseStringAsync(currentChar, ReadType.Read, cancel).ConfigureAwait(false);
                    return true;
                case 't':
                    await ParseTrueAsync(cancel).ConfigureAwait(false);
                    return true;
                case 'f':
                    await ParseFalseAsync(cancel).ConfigureAwait(false);
                    return true;
                case 'n':
                    if (await EnsureCharsAsync(1, true, cancel).ConfigureAwait(false))
                    {
                        switch (charBuffer[charPos + 1])
                        {
                            case 'u':
                                await ParseNullAsync(cancel).ConfigureAwait(false);
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
                    await ParseNumberNaNAsync(ReadType.Read, cancel).ConfigureAwait(false);
                    return true;
                case 'I':
                    await ParseNumberPositiveInfinityAsync(ReadType.Read, cancel).ConfigureAwait(false);
                    return true;
                case '-':
                    if (await EnsureCharsAsync(1, true, cancel).ConfigureAwait(false) && charBuffer[charPos + 1] == 'I')
                    {
                        await ParseNumberNegativeInfinityAsync(ReadType.Read, cancel).ConfigureAwait(false);
                    }
                    else
                    {
                        await ParseNumberAsync(ReadType.Read, cancel).ConfigureAwait(false);
                    }

                    return true;
                case '/':
                    await ParseCommentAsync(true, cancel).ConfigureAwait(false);
                    return true;
                case 'u':
                    await ParseUndefinedAsync(cancel).ConfigureAwait(false);
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
                    await ProcessCarriageReturnAsync(false, cancel).ConfigureAwait(false);
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
                        await ParseNumberAsync(ReadType.Read, cancel).ConfigureAwait(false);
                        return true;
                    }

                    throw CreateUnexpectedCharacterException(currentChar);
            }
        }
    }

    async Task ReadStringIntoBufferAsync(char quote, Cancel cancel)
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

                        if (await ReadDataAsync(true, cancel).ConfigureAwait(false) == 0)
                        {
                            this.charPos = charPos;
                            throw JsonReaderException.Create(this, $"Unterminated string. Expected delimiter: {quote}.");
                        }
                    }

                    break;
                case '\\':
                    this.charPos = charPos;
                    if (!await EnsureCharsAsync(0, true, cancel).ConfigureAwait(false))
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
                            writeChar = await ParseUnicodeAsync(cancel).ConfigureAwait(false);

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
                                    if (await EnsureCharsAsync(2, true, cancel).ConfigureAwait(false) && charBuffer[this.charPos] == '\\' && charBuffer[this.charPos + 1] == 'u')
                                    {
                                        var highSurrogate = writeChar;

                                        this.charPos += 2;
                                        writeChar = await ParseUnicodeAsync(cancel).ConfigureAwait(false);

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
                    await ProcessCarriageReturnAsync(true, cancel).ConfigureAwait(false);
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

    Task ProcessCarriageReturnAsync(bool append, Cancel cancel)
    {
        charPos++;

        var task = EnsureCharsAsync(1, append, cancel);
        if (task.IsCompletedSuccessfully())
        {
            SetNewLine(task.Result);
            return Task.CompletedTask;
        }

        return ProcessCarriageReturnAsync(task);
    }

    async Task ProcessCarriageReturnAsync(Task<bool> task) =>
        SetNewLine(await task.ConfigureAwait(false));

    async Task<char> ParseUnicodeAsync(Cancel cancel) =>
        ConvertUnicode(await EnsureCharsAsync(4, true, cancel).ConfigureAwait(false));

    Task<bool> EnsureCharsAsync(int relativePosition, bool append, Cancel cancel)
    {
        if (charPos + relativePosition < charsUsed)
        {
            return AsyncUtils.True;
        }

        if (isEndOfFile)
        {
            return AsyncUtils.False;
        }

        return ReadCharsAsync(relativePosition, append, cancel);
    }

    async Task<bool> ReadCharsAsync(int relativePosition, bool append, Cancel cancel)
    {
        var charsRequired = charPos + relativePosition - charsUsed + 1;

        // it is possible that the TextReader doesn't return all data at once
        // repeat read until the required text is returned or the reader is out of content
        do
        {
            var charsRead = await ReadDataAsync(append, charsRequired, cancel).ConfigureAwait(false);

            // no more content
            if (charsRead == 0)
            {
                return false;
            }

            charsRequired -= charsRead;
        } while (charsRequired > 0);

        return true;
    }

    async Task<bool> ParseObjectAsync(Cancel cancel)
    {
        while (true)
        {
            var currentChar = charBuffer[charPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == charPos)
                    {
                        if (await ReadDataAsync(false, cancel).ConfigureAwait(false) == 0)
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
                    await ParseCommentAsync(true, cancel).ConfigureAwait(false);
                    return true;
                case StringUtils.CarriageReturn:
                    await ProcessCarriageReturnAsync(false, cancel).ConfigureAwait(false);
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
                        return await ParsePropertyAsync(cancel).ConfigureAwait(false);
                    }

                    break;
            }
        }
    }

    async Task ParseCommentAsync(bool setToken, Cancel cancel)
    {
        // should have already parsed / character before reaching this method
        charPos++;

        if (!await EnsureCharsAsync(1, false, cancel).ConfigureAwait(false))
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
                        if (await ReadDataAsync(true, cancel).ConfigureAwait(false) == 0)
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
                        if (await EnsureCharsAsync(0, true, cancel).ConfigureAwait(false))
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

                    await ProcessCarriageReturnAsync(true, cancel).ConfigureAwait(false);
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

    async Task EatWhitespaceAsync(Cancel cancel)
    {
        while (true)
        {
            var currentChar = charBuffer[charPos];

            switch (currentChar)
            {
                case '\0':
                    if (charsUsed == charPos)
                    {
                        if (await ReadDataAsync(false, cancel).ConfigureAwait(false) == 0)
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
                    await ProcessCarriageReturnAsync(false, cancel).ConfigureAwait(false);
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

    async Task ParseStringAsync(char quote, ReadType readType, Cancel cancel)
    {
        cancel.ThrowIfCancellationRequested();
        charPos++;

        ShiftBufferIfNeeded();
        await ReadStringIntoBufferAsync(quote, cancel).ConfigureAwait(false);
        ParseReadString(quote, readType);
    }

    async Task<bool> MatchValueAsync(string value, Cancel cancel) =>
        MatchValue(await EnsureCharsAsync(value.Length - 1, true, cancel).ConfigureAwait(false), value);

    async Task<bool> MatchValueWithTrailingSeparatorAsync(string value, Cancel cancel)
    {
        // will match value and then move to the next character, checking that it is a separator character
        if (!await MatchValueAsync(value, cancel).ConfigureAwait(false))
        {
            return false;
        }

        if (!await EnsureCharsAsync(0, false, cancel).ConfigureAwait(false))
        {
            return true;
        }

        return IsSeparator(charBuffer[charPos]) || charBuffer[charPos] == '\0';
    }

    async Task MatchAndSetAsync(string value, JsonToken newToken, object? tokenValue, Cancel cancel)
    {
        if (await MatchValueWithTrailingSeparatorAsync(value, cancel).ConfigureAwait(false))
        {
            SetToken(newToken, tokenValue);
        }
        else
        {
            throw JsonReaderException.Create(this, $"Error parsing {newToken.ToString().ToLowerInvariant()} value.");
        }
    }

    Task ParseTrueAsync(Cancel cancel) =>
        MatchAndSetAsync(JsonConvert.True, JsonToken.Boolean, true, cancel);

    Task ParseFalseAsync(Cancel cancel) =>
        MatchAndSetAsync(JsonConvert.False, JsonToken.Boolean, false, cancel);

    Task ParseNullAsync(Cancel cancel) =>
        MatchAndSetAsync(JsonConvert.Null, JsonToken.Null, null, cancel);

    async Task<object> ParseNumberNaNAsync(ReadType readType, Cancel cancel) =>
        ParseNumberNaN(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.NaN, cancel).ConfigureAwait(false));

    async Task<object> ParseNumberPositiveInfinityAsync(ReadType readType, Cancel cancel) =>
        ParseNumberPositiveInfinity(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.PositiveInfinity, cancel).ConfigureAwait(false));

    async Task<object> ParseNumberNegativeInfinityAsync(ReadType readType, Cancel cancel) =>
        ParseNumberNegativeInfinity(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.NegativeInfinity, cancel).ConfigureAwait(false));

    async Task ParseNumberAsync(ReadType readType, Cancel cancel)
    {
        ShiftBufferIfNeeded();

        var firstChar = charBuffer[charPos];
        var initialPosition = charPos;

        await ReadNumberIntoBufferAsync(cancel).ConfigureAwait(false);

        ParseReadNumber(readType, firstChar, initialPosition);
    }

    Task ParseUndefinedAsync(Cancel cancel) =>
        MatchAndSetAsync(JsonConvert.Undefined, JsonToken.Undefined, null, cancel);

    async Task<bool> ParsePropertyAsync(Cancel cancel)
    {
        var firstChar = charBuffer[charPos];
        char quoteChar;

        if (firstChar is '"' or '\'')
        {
            charPos++;
            quoteChar = firstChar;
            ShiftBufferIfNeeded();
            await ReadStringIntoBufferAsync(quoteChar, cancel).ConfigureAwait(false);
        }
        else if (ValidIdentifierChar(firstChar))
        {
            quoteChar = '\0';
            ShiftBufferIfNeeded();
            await ParseUnquotedPropertyAsync(cancel).ConfigureAwait(false);
        }
        else
        {
            throw JsonReaderException.Create(this, $"Invalid property identifier character: {charBuffer[charPos]}.");
        }

        string propertyName;

        if (PropertyNameTable == null)
        {
            propertyName = stringReference.ToString();
        }
        else
        {
            propertyName = PropertyNameTable.Get(stringReference.Chars, stringReference.StartIndex, stringReference.Length)
                           // no match in name table
                           ?? stringReference.ToString();
        }

        await EatWhitespaceAsync(cancel).ConfigureAwait(false);

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

    async Task ReadNumberIntoBufferAsync(Cancel cancel)
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
                    if (await ReadDataAsync(true, cancel).ConfigureAwait(false) == 0)
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

    async Task ParseUnquotedPropertyAsync(Cancel cancel)
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
                    if (await ReadDataAsync(true, cancel).ConfigureAwait(false) == 0)
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

    async Task<bool> ReadNullCharAsync(Cancel cancel)
    {
        if (charsUsed == charPos)
        {
            if (await ReadDataAsync(false, cancel).ConfigureAwait(false) == 0)
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

    async Task HandleNullAsync(Cancel cancel)
    {
        if (await EnsureCharsAsync(1, true, cancel).ConfigureAwait(false))
        {
            if (charBuffer[charPos + 1] == 'u')
            {
                await ParseNullAsync(cancel).ConfigureAwait(false);
                return;
            }

            charPos += 2;
            throw CreateUnexpectedCharacterException(charBuffer[charPos - 1]);
        }

        charPos = charsUsed;
        throw CreateUnexpectedEndException();
    }

    async Task ReadFinishedAsync(Cancel cancel)
    {
        if (await EnsureCharsAsync(0, false, cancel).ConfigureAwait(false))
        {
            await EatWhitespaceAsync(cancel).ConfigureAwait(false);
            if (isEndOfFile)
            {
                SetToken(JsonToken.None);
                return;
            }

            if (charBuffer[charPos] == '/')
            {
                await ParseCommentAsync(false, cancel).ConfigureAwait(false);
            }
            else
            {
                throw JsonReaderException.Create(this, $"Additional text encountered after finished reading JSON content: {charBuffer[charPos]}.");
            }
        }

        SetToken(JsonToken.None);
    }

    async Task<object?> ReadStringValueAsync(ReadType readType, Cancel cancel)
    {
        switch (currentState)
        {
            case State.PostValue:
                if (await ParsePostValueAsync(true, cancel).ConfigureAwait(false))
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
                            if (await ReadNullCharAsync(cancel).ConfigureAwait(false))
                            {
                                SetNoneToken();
                                return null;
                            }

                            break;
                        case '"':
                        case '\'':
                            await ParseStringAsync(currentChar, readType, cancel).ConfigureAwait(false);
                            return FinishReadQuotedStringValue(readType);
                        case '-':
                            if (await EnsureCharsAsync(1, true, cancel).ConfigureAwait(false) && charBuffer[charPos + 1] == 'I')
                            {
                                return await ParseNumberNegativeInfinityAsync(readType, cancel);
                            }

                            await ParseNumberAsync(readType, cancel).ConfigureAwait(false);
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

                            await ParseNumberAsync(ReadType.ReadAsString, cancel).ConfigureAwait(false);
                            return Value;
                        case 't':
                        case 'f':
                            if (readType != ReadType.ReadAsString)
                            {
                                charPos++;
                                throw CreateUnexpectedCharacterException(currentChar);
                            }

                            var expected = currentChar == 't' ? JsonConvert.True : JsonConvert.False;
                            if (!await MatchValueWithTrailingSeparatorAsync(expected, cancel).ConfigureAwait(false))
                            {
                                throw CreateUnexpectedCharacterException(charBuffer[charPos]);
                            }

                            SetToken(JsonToken.String, expected);
                            return expected;
                        case 'I':
                            return await ParseNumberPositiveInfinityAsync(readType, cancel).ConfigureAwait(false);
                        case 'N':
                            return await ParseNumberNaNAsync(readType, cancel).ConfigureAwait(false);
                        case 'n':
                            await HandleNullAsync(cancel).ConfigureAwait(false);
                            return null;
                        case '/':
                            await ParseCommentAsync(false, cancel).ConfigureAwait(false);
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
                            await ProcessCarriageReturnAsync(false, cancel).ConfigureAwait(false);
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
                await ReadFinishedAsync(cancel).ConfigureAwait(false);
                return null;
            default:
                throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
        }
    }

    async Task<object?> ReadNumberValueAsync(ReadType readType, Cancel cancel)
    {
        switch (currentState)
        {
            case State.PostValue:
                if (await ParsePostValueAsync(true, cancel).ConfigureAwait(false))
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
                            if (await ReadNullCharAsync(cancel).ConfigureAwait(false))
                            {
                                SetNoneToken();
                                return null;
                            }

                            break;
                        case '"':
                        case '\'':
                            await ParseStringAsync(currentChar, readType, cancel).ConfigureAwait(false);
                            return FinishReadQuotedNumber(readType);
                        case 'n':
                            await HandleNullAsync(cancel).ConfigureAwait(false);
                            return null;
                        case 'N':
                            return await ParseNumberNaNAsync(readType, cancel).ConfigureAwait(false);
                        case 'I':
                            return await ParseNumberPositiveInfinityAsync(readType, cancel).ConfigureAwait(false);
                        case '-':
                            if (await EnsureCharsAsync(1, true, cancel).ConfigureAwait(false) && charBuffer[charPos + 1] == 'I')
                            {
                                return await ParseNumberNegativeInfinityAsync(readType, cancel).ConfigureAwait(false);
                            }

                            await ParseNumberAsync(readType, cancel).ConfigureAwait(false);
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
                            await ParseNumberAsync(readType, cancel).ConfigureAwait(false);
                            return Value;
                        case '/':
                            await ParseCommentAsync(false, cancel).ConfigureAwait(false);
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
                            await ProcessCarriageReturnAsync(false, cancel).ConfigureAwait(false);
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
                await ReadFinishedAsync(cancel).ConfigureAwait(false);
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
    public override Task<bool?> ReadAsBooleanAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoReadAsBooleanAsync(cancel);
        }

        return base.ReadAsBooleanAsync(cancel);
    }

    async Task<bool?> DoReadAsBooleanAsync(Cancel cancel)
    {
        switch (currentState)
        {
            case State.PostValue:
                if (await ParsePostValueAsync(true, cancel).ConfigureAwait(false))
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
                            if (await ReadNullCharAsync(cancel).ConfigureAwait(false))
                            {
                                SetNoneToken();
                                return null;
                            }

                            break;
                        case '"':
                        case '\'':
                            await ParseStringAsync(currentChar, ReadType.Read, cancel).ConfigureAwait(false);
                            return ReadBooleanString(stringReference.ToString());
                        case 'n':
                            await HandleNullAsync(cancel).ConfigureAwait(false);
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
                            await ParseNumberAsync(ReadType.Read, cancel).ConfigureAwait(false);
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
                            if (!await MatchValueWithTrailingSeparatorAsync(isTrue ? JsonConvert.True : JsonConvert.False, cancel).ConfigureAwait(false))
                            {
                                throw CreateUnexpectedCharacterException(charBuffer[charPos]);
                            }

                            SetToken(JsonToken.Boolean, BoxedPrimitives.Get(isTrue));
                            return isTrue;
                        case '/':
                            await ParseCommentAsync(false, cancel).ConfigureAwait(false);
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
                            await ProcessCarriageReturnAsync(false, cancel).ConfigureAwait(false);
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
                await ReadFinishedAsync(cancel).ConfigureAwait(false);
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
    public override Task<byte[]?> ReadAsBytesAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoReadAsBytesAsync(cancel);
        }

        return base.ReadAsBytesAsync(cancel);
    }

    async Task<byte[]?> DoReadAsBytesAsync(Cancel cancel)
    {
        var isWrapped = false;

        switch (currentState)
        {
            case State.PostValue:
                if (await ParsePostValueAsync(true, cancel).ConfigureAwait(false))
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
                            if (await ReadNullCharAsync(cancel).ConfigureAwait(false))
                            {
                                SetNoneToken();
                                return null;
                            }

                            break;
                        case '"':
                        case '\'':
                            await ParseStringAsync(currentChar, ReadType.ReadAsBytes, cancel).ConfigureAwait(false);
                            var data = (byte[]?) Value;
                            if (isWrapped)
                            {
                                await ReaderReadAndAssertAsync(cancel).ConfigureAwait(false);
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
                            await ReadIntoWrappedTypeObjectAsync(cancel).ConfigureAwait(false);
                            isWrapped = true;
                            break;
                        case '[':
                            charPos++;
                            SetToken(JsonToken.StartArray);
                            return await ReadArrayIntoByteArrayAsync(cancel).ConfigureAwait(false);
                        case 'n':
                            await HandleNullAsync(cancel).ConfigureAwait(false);
                            return null;
                        case '/':
                            await ParseCommentAsync(false, cancel).ConfigureAwait(false);
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
                            await ProcessCarriageReturnAsync(false, cancel).ConfigureAwait(false);
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
                await ReadFinishedAsync(cancel).ConfigureAwait(false);
                return null;
            default:
                throw JsonReaderException.Create(this, $"Unexpected state: {CurrentState}.");
        }
    }

    async Task ReadIntoWrappedTypeObjectAsync(Cancel cancel)
    {
        await ReaderReadAndAssertAsync(cancel).ConfigureAwait(false);
        if (Value != null && Value.ToString() == JsonTypeReflector.TypePropertyName)
        {
            await ReaderReadAndAssertAsync(cancel).ConfigureAwait(false);
            if (Value != null && Value.ToString()!.StartsWith("System.Byte[]", StringComparison.Ordinal))
            {
                await ReaderReadAndAssertAsync(cancel).ConfigureAwait(false);
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
    public override Task<DateTime?> ReadAsDateTimeAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoReadAsDateTimeAsync(cancel);
        }

        return base.ReadAsDateTimeAsync(cancel);
    }

    async Task<DateTime?> DoReadAsDateTimeAsync(Cancel cancel) =>
        (DateTime?) await ReadStringValueAsync(ReadType.ReadAsDateTime, cancel).ConfigureAwait(false);

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
    public override Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoReadAsDateTimeOffsetAsync(cancel);
        }

        return base.ReadAsDateTimeOffsetAsync(cancel);
    }

    async Task<DateTimeOffset?> DoReadAsDateTimeOffsetAsync(Cancel cancel) =>
        (DateTimeOffset?) await ReadStringValueAsync(ReadType.ReadAsDateTimeOffset, cancel).ConfigureAwait(false);

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
    public override Task<decimal?> ReadAsDecimalAsync(Cancel cancel = default) =>
        safeAsync switch
        {
            true => DoReadAsDecimalAsync(cancel),
            _ => base.ReadAsDecimalAsync(cancel)
        };

    async Task<decimal?> DoReadAsDecimalAsync(Cancel cancel) =>
        (decimal?) await ReadNumberValueAsync(ReadType.ReadAsDecimal, cancel).ConfigureAwait(false);

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
    public override Task<double?> ReadAsDoubleAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoReadAsDoubleAsync(cancel);
        }

        return base.ReadAsDoubleAsync(cancel);
    }

    async Task<double?> DoReadAsDoubleAsync(Cancel cancel) =>
        (double?) await ReadNumberValueAsync(ReadType.ReadAsDouble, cancel).ConfigureAwait(false);

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
    public override Task<int?> ReadAsInt32Async(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoReadAsInt32Async(cancel);
        }

        return base.ReadAsInt32Async(cancel);
    }

    async Task<int?> DoReadAsInt32Async(Cancel cancel) =>
        (int?) await ReadNumberValueAsync(ReadType.ReadAsInt32, cancel).ConfigureAwait(false);

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
    public override Task<string?> ReadAsStringAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoReadAsStringAsync(cancel);
        }

        return base.ReadAsStringAsync(cancel);
    }

    async Task<string?> DoReadAsStringAsync(Cancel cancel) =>
        (string?) await ReadStringValueAsync(ReadType.ReadAsString, cancel).ConfigureAwait(false);
}