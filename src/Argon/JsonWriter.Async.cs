// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable RedundantSuppressNullableWarningExpression
namespace Argon;

public abstract partial class JsonWriter
#if NET5_0_OR_GREATER
        : IAsyncDisposable
{
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (currentState != State.Closed)
        {
            await CloseAsync().ConfigureAwait(false);
        }
    }
#else
{
#endif

    internal Task AutoCompleteAsync(JsonToken tokenBeingWritten, Cancel cancel)
    {
        var oldState = currentState;

        // gets new state based on the current state and what is being written
        var newState = stateArray[(int) tokenBeingWritten][(int) oldState];

        if (newState == State.Error)
        {
            throw JsonWriterException.Create(this, $"Token {tokenBeingWritten} in state {oldState} would result in an invalid JSON object.");
        }

        currentState = newState;

        if (Formatting == Formatting.Indented)
        {
            switch (oldState)
            {
                case State.Start:
                    break;
                case State.Property:
                    return WriteIndentSpaceAsync(cancel);
                case State.ArrayStart:
                    return WriteIndentAsync(cancel);
                case State.Array:
                    return tokenBeingWritten == JsonToken.Comment ? WriteIndentAsync(cancel) : AutoCompleteAsync(cancel);
                case State.Object:
                    switch (tokenBeingWritten)
                    {
                        case JsonToken.Comment:
                            break;
                        case JsonToken.PropertyName:
                            return AutoCompleteAsync(cancel);
                        default:
                            return WriteValueDelimiterAsync(cancel);
                    }

                    break;
                default:
                    if (tokenBeingWritten == JsonToken.PropertyName)
                    {
                        return WriteIndentAsync(cancel);
                    }

                    break;
            }
        }
        else if (tokenBeingWritten != JsonToken.Comment)
        {
            switch (oldState)
            {
                case State.Object:
                case State.Array:
                    return WriteValueDelimiterAsync(cancel);
            }
        }

        return Task.CompletedTask;
    }

    async Task AutoCompleteAsync(Cancel cancel)
    {
        await WriteValueDelimiterAsync(cancel).ConfigureAwait(false);
        await WriteIndentAsync(cancel).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously closes this writer.
    /// If <see cref="JsonWriter.CloseOutput" /> is set to <c>true</c>, the destination is also closed.
    /// </summary>
    public virtual Task CloseAsync(Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        Close();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously flushes whatever is in the buffer to the destination and also flushes the destination.
    /// </summary>
    public virtual Task FlushAsync(Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        Flush();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes the specified end token.
    /// </summary>
    protected virtual Task WriteEndAsync(JsonToken token, Cancel cancel)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteEnd(token);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes indent characters.
    /// </summary>
    protected virtual Task WriteIndentAsync(Cancel cancel)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteIndent();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes the JSON value delimiter.
    /// </summary>
    protected virtual Task WriteValueDelimiterAsync(Cancel cancel)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValueDelimiter();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes an indent space.
    /// </summary>
    protected virtual Task WriteIndentSpaceAsync(Cancel cancel)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteIndentSpace();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes raw JSON without changing the writer's state.
    /// </summary>
    public virtual Task WriteRawAsync(string? json, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteRaw(json);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes the end of the current JSON object or array.
    /// </summary>
    public virtual Task WriteEndAsync(Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteEnd();
        return Task.CompletedTask;
    }

    internal Task WriteEndInternalAsync(Cancel cancel)
    {
        var type = Peek();
        switch (type)
        {
            case JsonContainerType.Object:
                return WriteEndObjectAsync(cancel);
            case JsonContainerType.Array:
                return WriteEndArrayAsync(cancel);
            default:
                if (cancel.IsCancellationRequested)
                {
                    return cancel.FromCanceled();
                }

                throw JsonWriterException.Create(this, $"Unexpected type when writing end: {type}");
        }
    }

    internal Task InternalWriteEndAsync(JsonContainerType type, Cancel cancel)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        var levelsToComplete = CalculateLevelsToComplete(type);
        while (levelsToComplete-- > 0)
        {
            var token = GetCloseTokenForType(Pop());

            Task t;
            if (currentState == State.Property)
            {
                t = WriteNullAsync(cancel);
                if (!t.IsCompletedSuccessfully())
                {
                    return AwaitProperty(t, levelsToComplete, token, cancel);
                }
            }

            if (Formatting == Formatting.Indented)
            {
                if (currentState != State.ObjectStart && currentState != State.ArrayStart)
                {
                    t = WriteIndentAsync(cancel);
                    if (!t.IsCompletedSuccessfully())
                    {
                        return AwaitIndent(t, levelsToComplete, token, cancel);
                    }
                }
            }

            t = WriteEndAsync(token, cancel);
            if (!t.IsCompletedSuccessfully())
            {
                return AwaitEnd(t, levelsToComplete, cancel);
            }

            UpdateCurrentState();
        }

        return Task.CompletedTask;
    }

    async Task AwaitIndent(Task task, int levelsToComplete, JsonToken token, Cancel cancel)
    {
        await task.ConfigureAwait(false);

        //  Finish current loop

        await WriteEndAsync(token, cancel).ConfigureAwait(false);

        UpdateCurrentState();

        await AwaitRemaining(levelsToComplete, cancel).ConfigureAwait(false);
    }

    async Task AwaitProperty(Task task, int levelsToComplete, JsonToken token, Cancel cancel)
    {
        await task.ConfigureAwait(false);

        //  Finish current loop
        if (Formatting == Formatting.Indented)
        {
            if (currentState != State.ObjectStart && currentState != State.ArrayStart)
            {
                await WriteIndentAsync(cancel).ConfigureAwait(false);
            }
        }

        await WriteEndAsync(token, cancel).ConfigureAwait(false);

        UpdateCurrentState();

        await AwaitRemaining(levelsToComplete, cancel).ConfigureAwait(false);
    }

    async Task AwaitEnd(Task task, int levelsToComplete, Cancel cancel)
    {
        await task.ConfigureAwait(false);

        //  Finish current loop

        UpdateCurrentState();

        await AwaitRemaining(levelsToComplete, cancel).ConfigureAwait(false);
    }

    async Task AwaitRemaining(int levelsToComplete, Cancel cancel)
    {
        while (levelsToComplete-- > 0)
        {
            var token = GetCloseTokenForType(Pop());

            if (currentState == State.Property)
            {
                await WriteNullAsync(cancel).ConfigureAwait(false);
            }

            if (Formatting == Formatting.Indented)
            {
                if (currentState != State.ObjectStart && currentState != State.ArrayStart)
                {
                    await WriteIndentAsync(cancel).ConfigureAwait(false);
                }
            }

            await WriteEndAsync(token, cancel).ConfigureAwait(false);

            UpdateCurrentState();
        }
    }

    /// <summary>
    /// Asynchronously writes the end of an array.
    /// </summary>
    public virtual Task WriteEndArrayAsync(Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteEndArray();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes the end of a JSON object.
    /// </summary>
    public virtual Task WriteEndObjectAsync(Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteEndObject();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a null value.
    /// </summary>
    public virtual Task WriteNullAsync(Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteNull();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes the property name of a name/value pair of a JSON object.
    /// </summary>
    public virtual Task WritePropertyNameAsync(string name, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WritePropertyName(name);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes the property name of a name/value pair of a JSON object.
    /// </summary>
    /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
    public virtual Task WritePropertyNameAsync(string name, bool escape, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WritePropertyName(name, escape);
        return Task.CompletedTask;
    }

    internal Task InternalWritePropertyNameAsync(string name, Cancel cancel)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        currentPosition.PropertyName = name;
        return AutoCompleteAsync(JsonToken.PropertyName, cancel);
    }

    /// <summary>
    /// Asynchronously writes the beginning of a JSON array.
    /// </summary>
    public virtual Task WriteStartArrayAsync(Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteStartArray();
        return Task.CompletedTask;
    }

    internal async Task InternalWriteStartAsync(JsonToken token, JsonContainerType container, Cancel cancel)
    {
        UpdateScopeWithFinishedValue();
        await AutoCompleteAsync(token, cancel).ConfigureAwait(false);
        Push(container);
    }

    /// <summary>
    /// Asynchronously writes a comment <c>/*...*/</c> containing the specified text.
    /// </summary>
    public virtual Task WriteCommentAsync(string? text, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteComment(text);
        return Task.CompletedTask;
    }

    internal Task InternalWriteCommentAsync(Cancel cancel) =>
        AutoCompleteAsync(JsonToken.Comment, cancel);

    /// <summary>
    /// Asynchronously writes raw JSON where a value is expected and updates the writer's state.
    /// </summary>
    public virtual Task WriteRawValueAsync(string? json, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteRawValue(json);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes the beginning of a JSON object.
    /// </summary>
    public virtual Task WriteStartObjectAsync(Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteStartObject();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes the current <see cref="JsonReader" /> token.
    /// </summary>
    public Task WriteTokenAsync(JsonReader reader, Cancel cancel = default) =>
        WriteTokenAsync(reader, true, cancel);

    /// <summary>
    /// Asynchronously writes the current <see cref="JsonReader" /> token.
    /// </summary>
    /// <param name="writeChildren">A flag indicating whether the current token's children should be written.</param>
    public Task WriteTokenAsync(JsonReader reader, bool writeChildren, Cancel cancel = default) =>
        WriteTokenAsync(reader, writeChildren, true, cancel);

    /// <summary>
    /// Asynchronously writes the <see cref="JsonToken" /> token and its value.
    /// </summary>
    public Task WriteTokenAsync(JsonToken token, Cancel cancel = default) =>
        WriteTokenAsync(token, null, cancel);

    /// <summary>
    /// Asynchronously writes the <see cref="JsonToken" /> token and its value.
    /// </summary>
    /// <param name="value">
    /// The value to write.
    /// A value is only required for tokens that have an associated value, e.g. the <see cref="String" /> property name for <see cref="JsonToken.PropertyName" />.
    /// <c>null</c> can be passed to the method for tokens that don't have a value, e.g. <see cref="JsonToken.StartObject" />.
    /// </param>
    public Task WriteTokenAsync(JsonToken token, object? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        switch (token)
        {
            case JsonToken.None:
                // read to next
                return Task.CompletedTask;
            case JsonToken.StartObject:
                return WriteStartObjectAsync(cancel);
            case JsonToken.StartArray:
                return WriteStartArrayAsync(cancel);
            case JsonToken.PropertyName:
                return WritePropertyNameAsync(value!.ToString()!, cancel);
            case JsonToken.Comment:
                return WriteCommentAsync(value?.ToString(), cancel);
            case JsonToken.Integer:
                return value is BigInteger integer ? WriteValueAsync(integer, cancel) : WriteValueAsync(Convert.ToInt64(value, InvariantCulture), cancel);
            case JsonToken.Float:
                if (value is decimal dec)
                {
                    return WriteValueAsync(dec, cancel);
                }

                if (value is double doub)
                {
                    return WriteValueAsync(doub, cancel);
                }

                if (value is float f)
                {
                    return WriteValueAsync(f, cancel);
                }

                return WriteValueAsync(Convert.ToDouble(value, InvariantCulture), cancel);
            case JsonToken.String:
                return WriteValueAsync(value!.ToString(), cancel);
            case JsonToken.Boolean:
                return WriteValueAsync(Convert.ToBoolean(value, InvariantCulture), cancel);
            case JsonToken.Null:
                return WriteNullAsync(cancel);
            case JsonToken.Undefined:
                return WriteUndefinedAsync(cancel);
            case JsonToken.EndObject:
                return WriteEndObjectAsync(cancel);
            case JsonToken.EndArray:
                return WriteEndArrayAsync(cancel);
            case JsonToken.Date:
                if (value is DateTimeOffset offset)
                {
                    return WriteValueAsync(offset, cancel);
                }

                return WriteValueAsync(Convert.ToDateTime(value, InvariantCulture), cancel);
            case JsonToken.Raw:
                return WriteRawValueAsync(value?.ToString(), cancel);
            case JsonToken.Bytes:
                if (value is Guid guid)
                {
                    return WriteValueAsync(guid, cancel);
                }

                return WriteValueAsync((byte[]?) value, cancel);
            default:
                throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(token), token, "Unexpected token type.");
        }
    }

    internal virtual async Task WriteTokenAsync(JsonReader reader, bool writeChildren, bool writeComments, Cancel cancel)
    {
        var initialDepth = CalculateWriteTokenInitialDepth(reader);

        var initialDepthOffset = initialDepth - 1;
        do
        {
            if (writeComments || reader.TokenType != JsonToken.Comment)
            {
                await WriteTokenAsync(reader.TokenType, reader.Value, cancel).ConfigureAwait(false);
            }
        } while (
            // stop if we have reached the end of the token being read
            initialDepthOffset < reader.Depth - reader.TokenType.EndTokenOffset()
            && writeChildren
            && await reader.ReadAsync(cancel).ConfigureAwait(false));

        if (IsWriteTokenIncomplete(reader, writeChildren, initialDepth))
        {
            throw JsonWriterException.Create(this, "Unexpected end when reading token.");
        }
    }

    // For internal use, when we know the writer does not offer true async support (e.g. when backed
    // by a StringWriter) and therefore async write methods are always in practice just a less efficient
    // path through the sync version.
    internal async Task WriteTokenSyncReadingAsync(JsonReader reader, Cancel cancel)
    {
        var initialDepth = CalculateWriteTokenInitialDepth(reader);

        var initialDepthOffset = initialDepth - 1;
        do
        {
            WriteToken(reader.TokenType, reader.Value);
        } while (
            // stop if we have reached the end of the token being read
            initialDepthOffset < reader.Depth - reader.TokenType.EndTokenOffset()
            && await reader.ReadAsync(cancel).ConfigureAwait(false));

        if (initialDepth < CalculateWriteTokenFinalDepth(reader))
        {
            throw JsonWriterException.Create(this, "Unexpected end when reading token.");
        }
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="bool" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(bool value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="bool" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(bool? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="byte" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(byte value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="byte" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(byte? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="byte" />[] value.
    /// </summary>
    public virtual Task WriteValueAsync(byte[]? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="char" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(char value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="char" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(char? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="DateTime" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(DateTime value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="DateTime" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(DateTime? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="DateTimeOffset" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(DateTimeOffset value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="DateTimeOffset" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(DateTimeOffset? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="decimal" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(decimal value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="decimal" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(decimal? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="double" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(double value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="double" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(double? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="float" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(float value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="float" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(float? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Guid" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(Guid value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="Guid" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(Guid? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="int" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(int value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="int" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(int? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="long" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(long value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="long" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(long? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="object" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(object? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="sbyte" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(sbyte value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="sbyte" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(sbyte? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="short" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(short value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="short" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(short? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="string" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(string? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="TimeSpan" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(TimeSpan value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="TimeSpan" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(TimeSpan? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="uint" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(uint value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="uint" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(uint? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="ulong" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(ulong value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="ulong" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(ulong? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Uri" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(Uri? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="ushort" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(ushort value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="ushort" /> value.
    /// </summary>
    public virtual Task WriteValueAsync(ushort? value, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteValue(value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes an undefined value.
    /// </summary>
    public virtual Task WriteUndefinedAsync(Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteUndefined();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously writes the given white space.
    /// </summary>
    public virtual Task WriteWhitespaceAsync(string ws, Cancel cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        WriteWhitespace(ws);
        return Task.CompletedTask;
    }

    internal Task InternalWriteValueAsync(JsonToken token, Cancel cancel)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        UpdateScopeWithFinishedValue();
        return AutoCompleteAsync(token, cancel);
    }

    /// <summary>
    /// Asynchronously ets the state of the <see cref="JsonWriter" />.
    /// </summary>
    protected Task SetWriteStateAsync(JsonToken token, object value, Cancel cancel)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        switch (token)
        {
            case JsonToken.StartObject:
                return InternalWriteStartAsync(token, JsonContainerType.Object, cancel);
            case JsonToken.StartArray:
                return InternalWriteStartAsync(token, JsonContainerType.Array, cancel);
            case JsonToken.PropertyName:
                if (value is not string s)
                {
                    throw new ArgumentException("A name is required when setting property name state.", nameof(value));
                }

                return InternalWritePropertyNameAsync(s, cancel);
            case JsonToken.Comment:
                return InternalWriteCommentAsync(cancel);
            case JsonToken.Raw:
                return Task.CompletedTask;
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.String:
            case JsonToken.Boolean:
            case JsonToken.Date:
            case JsonToken.Bytes:
            case JsonToken.Null:
            case JsonToken.Undefined:
                return InternalWriteValueAsync(token, cancel);
            case JsonToken.EndObject:
                return InternalWriteEndAsync(JsonContainerType.Object, cancel);
            case JsonToken.EndArray:
                return InternalWriteEndAsync(JsonContainerType.Array, cancel);
            default:
                throw new ArgumentOutOfRangeException(nameof(token));
        }
    }

    internal static Task WriteValueAsync(JsonWriter writer, PrimitiveTypeCode typeCode, object value, Cancel cancel)
    {
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        while (true)
        {
            switch (typeCode)
            {
                case PrimitiveTypeCode.Char:
                    return writer.WriteValueAsync((char) value, cancel);
                case PrimitiveTypeCode.CharNullable:
                    return writer.WriteValueAsync((char?) value, cancel);
                case PrimitiveTypeCode.Boolean:
                    return writer.WriteValueAsync((bool) value, cancel);
                case PrimitiveTypeCode.BooleanNullable:
                    return writer.WriteValueAsync((bool?) value, cancel);
                case PrimitiveTypeCode.SByte:
                    return writer.WriteValueAsync((sbyte) value, cancel);
                case PrimitiveTypeCode.SByteNullable:
                    return writer.WriteValueAsync((sbyte?) value, cancel);
                case PrimitiveTypeCode.Int16:
                    return writer.WriteValueAsync((short) value, cancel);
                case PrimitiveTypeCode.Int16Nullable:
                    return writer.WriteValueAsync((short?) value, cancel);
                case PrimitiveTypeCode.UInt16:
                    return writer.WriteValueAsync((ushort) value, cancel);
                case PrimitiveTypeCode.UInt16Nullable:
                    return writer.WriteValueAsync((ushort?) value, cancel);
                case PrimitiveTypeCode.Int32:
                    return writer.WriteValueAsync((int) value, cancel);
                case PrimitiveTypeCode.Int32Nullable:
                    // ReSharper disable once MergeConditionalExpression
                    return writer.WriteValueAsync(value == null ? null : (int) value, cancel);
                case PrimitiveTypeCode.Byte:
                    return writer.WriteValueAsync((byte) value, cancel);
                case PrimitiveTypeCode.ByteNullable:
                    return writer.WriteValueAsync((byte?) value, cancel);
                case PrimitiveTypeCode.UInt32:
                    return writer.WriteValueAsync((uint) value, cancel);
                case PrimitiveTypeCode.UInt32Nullable:
                    return writer.WriteValueAsync((uint?) value, cancel);
                case PrimitiveTypeCode.Int64:
                    return writer.WriteValueAsync((long) value, cancel);
                case PrimitiveTypeCode.Int64Nullable:
                    return writer.WriteValueAsync((long?) value, cancel);
                case PrimitiveTypeCode.UInt64:
                    return writer.WriteValueAsync((ulong) value, cancel);
                case PrimitiveTypeCode.UInt64Nullable:
                    return writer.WriteValueAsync((ulong?) value, cancel);
                case PrimitiveTypeCode.Single:
                    return writer.WriteValueAsync((float) value, cancel);
                case PrimitiveTypeCode.SingleNullable:
                    return writer.WriteValueAsync((float?) value, cancel);
                case PrimitiveTypeCode.Double:
                    return writer.WriteValueAsync((double) value, cancel);
                case PrimitiveTypeCode.DoubleNullable:
                    return writer.WriteValueAsync((double?) value, cancel);
                case PrimitiveTypeCode.DateTime:
                    return writer.WriteValueAsync((DateTime) value, cancel);
                case PrimitiveTypeCode.DateTimeNullable:
                    return writer.WriteValueAsync((DateTime?) value, cancel);
                case PrimitiveTypeCode.DateTimeOffset:
                    return writer.WriteValueAsync((DateTimeOffset) value, cancel);
                case PrimitiveTypeCode.DateTimeOffsetNullable:
                    return writer.WriteValueAsync((DateTimeOffset?) value, cancel);
                case PrimitiveTypeCode.Decimal:
                    return writer.WriteValueAsync((decimal) value, cancel);
                case PrimitiveTypeCode.DecimalNullable:
                    return writer.WriteValueAsync((decimal?) value, cancel);
                case PrimitiveTypeCode.Guid:
                    return writer.WriteValueAsync((Guid) value, cancel);
                case PrimitiveTypeCode.GuidNullable:
                    return writer.WriteValueAsync((Guid?) value, cancel);
                case PrimitiveTypeCode.TimeSpan:
                    return writer.WriteValueAsync((TimeSpan) value, cancel);
                case PrimitiveTypeCode.TimeSpanNullable:
                    return writer.WriteValueAsync((TimeSpan?) value, cancel);
                case PrimitiveTypeCode.BigInteger:

                    // this will call to WriteValueAsync(object)
                    return writer.WriteValueAsync((BigInteger) value, cancel);
                case PrimitiveTypeCode.BigIntegerNullable:

                    // this will call to WriteValueAsync(object)
                    return writer.WriteValueAsync((BigInteger?) value, cancel);
                case PrimitiveTypeCode.Uri:
                    return writer.WriteValueAsync((Uri) value, cancel);
                case PrimitiveTypeCode.String:
                    return writer.WriteValueAsync((string) value, cancel);
                case PrimitiveTypeCode.Bytes:
                    return writer.WriteValueAsync((byte[]) value, cancel);
                case PrimitiveTypeCode.DBNull:
                    return writer.WriteNullAsync(cancel);
                default:
                    if (value is IConvertible convertible)
                    {
                        ResolveConvertibleValue(convertible, out typeCode, out value);
                        continue;
                    }

                    // write an unknown null value, fix https://github.com/JamesNK/Newtonsoft.Json/issues/1460
                    if (value == null)
                    {
                        return writer.WriteNullAsync(cancel);
                    }

                    throw CreateUnsupportedTypeException(writer, value);
            }
        }
        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    }
}