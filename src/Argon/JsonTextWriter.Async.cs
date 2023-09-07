// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public partial class JsonTextWriter
{
    // It's not safe to perform the async methods here in a derived class as if the synchronous equivalent
    // has been overriden then the asynchronous method will no longer be doing the same operation.
    readonly bool safeAsync;

    /// <summary>
    /// Asynchronously flushes whatever is in the buffer to the destination and also flushes the destination.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task FlushAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoFlushAsync(cancel);
        }

        return base.FlushAsync(cancel);
    }

    Task DoFlushAsync(Cancel cancel) =>
        cancel.CancelIfRequestedAsync() ?? writer.FlushAsync();

    /// <summary>
    /// Asynchronously writes the JSON value delimiter.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    protected override Task WriteValueDelimiterAsync(Cancel cancel)
    {
        if (safeAsync)
        {
            return DoWriteValueDelimiterAsync(cancel);
        }

        return base.WriteValueDelimiterAsync(cancel);
    }

    Task DoWriteValueDelimiterAsync(Cancel cancel) =>
        writer.WriteAsync(',', cancel);

    /// <summary>
    /// Asynchronously writes the specified end token.
    /// </summary>
    /// <param name="token">The end token to write.</param>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    protected override Task WriteEndAsync(JsonToken token, Cancel cancel)
    {
        if (safeAsync)
        {
            return DoWriteEndAsync(token, cancel);
        }

        return base.WriteEndAsync(token, cancel);
    }

    Task DoWriteEndAsync(JsonToken token, Cancel cancel)
    {
        switch (token)
        {
            case JsonToken.EndObject:
                return writer.WriteAsync('}', cancel);
            case JsonToken.EndArray:
                return writer.WriteAsync(']', cancel);
            default:
                throw JsonWriterException.Create(this, $"Invalid JsonToken: {token}");
        }
    }

    /// <summary>
    /// Asynchronously closes this writer.
    /// If <see cref="JsonWriter.CloseOutput" /> is set to <c>true</c>, the destination is also closed.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task CloseAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoCloseAsync(cancel);
        }

        return base.CloseAsync(cancel);
    }

    async Task DoCloseAsync(Cancel cancel)
    {
        if (Top == 0) // otherwise will happen in calls to WriteEndAsync
        {
            cancel.ThrowIfCancellationRequested();
        }

        while (Top > 0)
        {
            await WriteEndAsync(cancel).ConfigureAwait(false);
        }

        await CloseBufferAndWriterAsync().ConfigureAwait(false);
    }

    private async Task CloseBufferAndWriterAsync()
    {
        if (writeBuffer != null)
        {
            BufferUtils.ReturnBuffer(writeBuffer);
            writeBuffer = null;
        }

        if (CloseOutput && writer != null)
        {
#if HAVE_ASYNC_DISPOABLE
            await _writer.DisposeAsync().ConfigureAwait(false);
#else
            // DisposeAsync isn't available. Instead, flush any remaining content with FlushAsync
            // to prevent Close/Dispose from making a blocking flush.
            //
            // No cancellation token on TextWriter.FlushAsync?!
            await writer.FlushAsync().ConfigureAwait(false);
#if HAVE_STREAM_READER_WRITER_CLOSE
            writer.Close();
#else
            writer.Dispose();
#endif
#endif
        }
    }

    /// <summary>
    /// Asynchronously writes the end of the current JSON object or array.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteEndAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteEndInternalAsync(cancel);
        }

        return base.WriteEndAsync(cancel);
    }

    /// <summary>
    /// Asynchronously writes indent characters.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    protected override Task WriteIndentAsync(Cancel cancel)
    {
        if (safeAsync)
        {
            return DoWriteIndentAsync(cancel);
        }

        return base.WriteIndentAsync(cancel);
    }

    Task DoWriteIndentAsync(Cancel cancel)
    {
        // levels of indentation multiplied by the indent count
        var currentIndentCount = Top * indentation;

        if (currentIndentCount <= indentCharBufferSize)
        {
            return writer.WriteAsync(indentChars, 0, newLine.Length + currentIndentCount, cancel);
        }

        return WriteIndentAsync(currentIndentCount, newLine.Length, cancel);
    }

    async Task WriteIndentAsync(int currentIndentCount, int newLineLen, Cancel cancel)
    {
        await writer.WriteAsync(indentChars, 0, newLineLen + Math.Min(currentIndentCount, indentCharBufferSize), cancel).ConfigureAwait(false);

        while ((currentIndentCount -= indentCharBufferSize) > 0)
        {
            await writer.WriteAsync(indentChars, newLineLen, Math.Min(currentIndentCount, indentCharBufferSize), cancel).ConfigureAwait(false);
        }
    }

    Task WriteValueInternalAsync(JsonToken token, string value, Cancel cancel)
    {
        var task = InternalWriteValueAsync(token, cancel);
        if (task.IsCompletedSuccessfully())
        {
            return writer.WriteAsync(value, cancel);
        }

        return WriteValueInternalAsync(task, value, cancel);
    }

    async Task WriteValueInternalAsync(Task task, string value, Cancel cancel)
    {
        await task.ConfigureAwait(false);
        await writer.WriteAsync(value, cancel).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes an indent space.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    protected override Task WriteIndentSpaceAsync(Cancel cancel)
    {
        if (safeAsync)
        {
            return DoWriteIndentSpaceAsync(cancel);
        }

        return base.WriteIndentSpaceAsync(cancel);
    }

    Task DoWriteIndentSpaceAsync(Cancel cancel) =>
        writer.WriteAsync(' ', cancel);

    /// <summary>
    /// Asynchronously writes raw JSON without changing the writer's state.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteRawAsync(string? json, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteRawAsync(json, cancel);
        }

        return base.WriteRawAsync(json, cancel);
    }

    Task DoWriteRawAsync(string? json, Cancel cancel) =>
        writer.WriteAsync(json, cancel);

    /// <summary>
    /// Asynchronously writes a null value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteNullAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteNullAsync(cancel);
        }

        return base.WriteNullAsync(cancel);
    }

    Task DoWriteNullAsync(Cancel cancel) =>
        WriteValueInternalAsync(JsonToken.Null, JsonConvert.Null, cancel);

    Task WriteDigitsAsync(ulong uvalue, bool negative, Cancel cancel)
    {
        if ((uvalue <= 9) & !negative)
        {
            return writer.WriteAsync((char) ('0' + uvalue), cancel);
        }

        var length = WriteNumberToBuffer(uvalue, negative);
        return writer.WriteAsync(writeBuffer!, 0, length, cancel);
    }

    Task WriteIntegerValueAsync(ulong uvalue, bool negative, Cancel cancel)
    {
        var task = InternalWriteValueAsync(JsonToken.Integer, cancel);
        if (task.IsCompletedSuccessfully())
        {
            return WriteDigitsAsync(uvalue, negative, cancel);
        }

        return WriteIntegerValueAsync(task, uvalue, negative, cancel);
    }

    async Task WriteIntegerValueAsync(Task task, ulong uvalue, bool negative, Cancel cancel)
    {
        await task.ConfigureAwait(false);
        await WriteDigitsAsync(uvalue, negative, cancel).ConfigureAwait(false);
    }

    Task WriteIntegerValueAsync(long value, Cancel cancel)
    {
        var negative = value < 0;
        if (negative)
        {
            value = -value;
        }

        return WriteIntegerValueAsync((ulong) value, negative, cancel);
    }

    Task WriteIntegerValueAsync(ulong uvalue, Cancel cancel) =>
        WriteIntegerValueAsync(uvalue, false, cancel);

    Task WriteEscapedStringAsync(string value, bool quote, Cancel cancel) =>
        JavaScriptUtils.WriteEscapedJavaScriptStringAsync(writer, value, quoteChar, quote, charEscapeFlags!, EscapeHandling, this, writeBuffer!, cancel);

    /// <summary>
    /// Asynchronously writes the property name of a name/value pair of a JSON object.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WritePropertyNameAsync(string name, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWritePropertyNameAsync(name, cancel);
        }

        return base.WritePropertyNameAsync(name, cancel);
    }

    Task DoWritePropertyNameAsync(string name, Cancel cancel)
    {
        var task = InternalWritePropertyNameAsync(name, cancel);
        if (!task.IsCompletedSuccessfully())
        {
            return DoWritePropertyNameAsync(task, name, cancel);
        }

        task = WriteEscapedStringAsync(name, QuoteName, cancel);
        if (task.IsCompletedSuccessfully())
        {
            return writer.WriteAsync(':', cancel);
        }

        return JavaScriptUtils.WriteCharAsync(task, writer, ':', cancel);
    }

    async Task DoWritePropertyNameAsync(Task task, string name, Cancel cancel)
    {
        await task.ConfigureAwait(false);

        await WriteEscapedStringAsync(name, QuoteName, cancel).ConfigureAwait(false);

        await writer.WriteAsync(':').ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes the property name of a name/value pair of a JSON object.
    /// </summary>
    /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WritePropertyNameAsync(string name, bool escape, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWritePropertyNameAsync(name, escape, cancel);
        }

        return base.WritePropertyNameAsync(name, escape, cancel);
    }

    async Task DoWritePropertyNameAsync(string name, bool escape, Cancel cancel)
    {
        await InternalWritePropertyNameAsync(name, cancel).ConfigureAwait(false);

        if (escape)
        {
            await WriteEscapedStringAsync(name, QuoteName, cancel).ConfigureAwait(false);
        }
        else
        {
            if (QuoteName)
            {
                await writer.WriteAsync(quoteChar).ConfigureAwait(false);
            }

            await writer.WriteAsync(name, cancel).ConfigureAwait(false);

            if (QuoteName)
            {
                await writer.WriteAsync(quoteChar).ConfigureAwait(false);
            }
        }

        await writer.WriteAsync(':').ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes the beginning of a JSON array.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteStartArrayAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteStartArrayAsync(cancel);
        }

        return base.WriteStartArrayAsync(cancel);
    }

    Task DoWriteStartArrayAsync(Cancel cancel)
    {
        var task = InternalWriteStartAsync(JsonToken.StartArray, JsonContainerType.Array, cancel);
        if (task.IsCompletedSuccessfully())
        {
            return writer.WriteAsync('[', cancel);
        }

        return DoWriteStartArrayAsync(task, cancel);
    }

    async Task DoWriteStartArrayAsync(Task task, Cancel cancel)
    {
        await task.ConfigureAwait(false);

        await writer.WriteAsync('[', cancel).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes the beginning of a JSON object.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteStartObjectAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteStartObjectAsync(cancel);
        }

        return base.WriteStartObjectAsync(cancel);
    }

    Task DoWriteStartObjectAsync(Cancel cancel)
    {
        var task = InternalWriteStartAsync(JsonToken.StartObject, JsonContainerType.Object, cancel);
        if (task.IsCompletedSuccessfully())
        {
            return writer.WriteAsync('{', cancel);
        }

        return DoWriteStartObjectAsync(task, cancel);
    }

    async Task DoWriteStartObjectAsync(Task task, Cancel cancel)
    {
        await task.ConfigureAwait(false);

        await writer.WriteAsync('{', cancel).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes an undefined value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteUndefinedAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteUndefinedAsync(cancel);
        }

        return base.WriteUndefinedAsync(cancel);
    }

    Task DoWriteUndefinedAsync(Cancel cancel)
    {
        var task = InternalWriteValueAsync(JsonToken.Undefined, cancel);
        if (task.IsCompletedSuccessfully())
        {
            return writer.WriteAsync(JsonConvert.Undefined, cancel);
        }

        return DoWriteUndefinedAsync(task, cancel);
    }

    async Task DoWriteUndefinedAsync(Task task, Cancel cancel)
    {
        await task.ConfigureAwait(false);
        await writer.WriteAsync(JsonConvert.Undefined, cancel).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes the given white space.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteWhitespaceAsync(string ws, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteWhitespaceAsync(ws, cancel);
        }

        return base.WriteWhitespaceAsync(ws, cancel);
    }

    Task DoWriteWhitespaceAsync(string ws, Cancel cancel)
    {
        InternalWriteWhitespace(ws);
        return writer.WriteAsync(ws, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="bool" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(bool value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(bool value, Cancel cancel) =>
        WriteValueInternalAsync(JsonToken.Boolean, JsonConvert.ToString(value), cancel);

    /// <summary>
    /// Asynchronously writes a <see cref="bool" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(bool? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(bool? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return DoWriteValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="byte" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(byte value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteIntegerValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="byte" /> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{T}" /> of <see cref="byte" /> value to write.</param>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(byte? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(byte? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return WriteIntegerValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="byte" />[] value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(byte[]? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return value == null ? WriteNullAsync(cancel) : WriteValueNonNullAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    async Task WriteValueNonNullAsync(byte[] value, Cancel cancel)
    {
        await InternalWriteValueAsync(JsonToken.Bytes, cancel).ConfigureAwait(false);
        if (QuoteValue)
        {
            await writer.WriteAsync(quoteChar).ConfigureAwait(false);
        }
        await Base64Encoder.EncodeAsync(value, 0, value.Length, cancel).ConfigureAwait(false);
        await Base64Encoder.FlushAsync(cancel).ConfigureAwait(false);
        if (QuoteValue)
        {
            await writer.WriteAsync(quoteChar).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously writes a <see cref="char" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(char value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(char value, Cancel cancel) =>
        WriteValueInternalAsync(JsonToken.String, JsonConvert.ToString(value), cancel);

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="char" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(char? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(char? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return DoWriteValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="DateTime" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(DateTime value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    async Task DoWriteValueAsync(DateTime value, Cancel cancel)
    {
        await InternalWriteValueAsync(JsonToken.Date, cancel).ConfigureAwait(false);

        var length = WriteValueToBuffer(value);

        await writer.WriteAsync(writeBuffer!, 0, length, cancel).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="DateTime" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(DateTime? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(DateTime? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return DoWriteValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="DateTimeOffset" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(DateTimeOffset value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    async Task DoWriteValueAsync(DateTimeOffset value, Cancel cancel)
    {
        await InternalWriteValueAsync(JsonToken.Date, cancel).ConfigureAwait(false);

        var length = WriteValueToBuffer(value);

        await writer.WriteAsync(writeBuffer!, 0, length, cancel).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="DateTimeOffset" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(DateTimeOffset? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(DateTimeOffset? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return DoWriteValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="decimal" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(decimal value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(decimal value, Cancel cancel) =>
        WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value), cancel);

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="decimal" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(decimal? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(decimal? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return DoWriteValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="double" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(double value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteValueAsync(value, false, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task WriteValueAsync(double value, bool nullable, Cancel cancel)
    {
        var convertedValue = JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, nullable);
        return WriteValueInternalAsync(JsonToken.Float, convertedValue, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="double" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(double? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            if (value.HasValue)
            {
                return WriteValueAsync(value.GetValueOrDefault(), true, cancel);
            }

            return WriteNullAsync(cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="float" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(float value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteValueAsync(value, false, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task WriteValueAsync(float value, bool nullable, Cancel cancel) =>
        WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, nullable), cancel);

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="float" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(float? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            if (value.HasValue)
            {
                return WriteValueAsync(value.GetValueOrDefault(), true, cancel);
            }

            return WriteNullAsync(cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Guid" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(Guid value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    async Task DoWriteValueAsync(Guid value, Cancel cancel)
    {
        await InternalWriteValueAsync(JsonToken.String, cancel).ConfigureAwait(false);

        if (QuoteValue)
        {
            await writer.WriteAsync(quoteChar).ConfigureAwait(false);
        }

        await writer.WriteAsync(value.ToString("D", InvariantCulture), cancel).ConfigureAwait(false);
        if (QuoteValue)
        {
            await writer.WriteAsync(quoteChar).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="Guid" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(Guid? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(Guid? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return DoWriteValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="int" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(int value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteIntegerValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="int" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(int? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(int? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return WriteIntegerValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="long" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(long value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteIntegerValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="long" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(long? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(long? value, Cancel cancel) =>
        value == null ? DoWriteNullAsync(cancel) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancel);

    internal Task WriteValueAsync(BigInteger value, Cancel cancel) =>
        WriteValueInternalAsync(JsonToken.Integer, value.ToString(InvariantCulture), cancel);

    /// <summary>
    /// Asynchronously writes a <see cref="object" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(object? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            if (value == null)
            {
                return WriteNullAsync(cancel);
            }

            if (value is BigInteger i)
            {
                return WriteValueAsync(i, cancel);
            }

            return WriteValueAsync(this, ConvertUtils.GetTypeCode(value.GetType()), value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="sbyte" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(sbyte value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteIntegerValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="sbyte" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(sbyte? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(sbyte? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return WriteIntegerValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="short" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(short value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteIntegerValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="short" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(short? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(short? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return WriteIntegerValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="string" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(string? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(string? value, Cancel cancel)
    {
        var task = InternalWriteValueAsync(JsonToken.String, cancel);
        if (task.IsCompletedSuccessfully())
        {
            if (value == null)
            {
                return writer.WriteAsync(JsonConvert.Null, cancel);
            }

            return WriteEscapedStringAsync(value, QuoteValue, cancel);
        }

        return DoWriteValueAsync(task, value, cancel);
    }

    async Task DoWriteValueAsync(Task task, string? value, Cancel cancel)
    {
        await task.ConfigureAwait(false);

        if (value == null)
        {
            await writer.WriteAsync(JsonConvert.Null, cancel).ConfigureAwait(false);
            return;
        }

        await WriteEscapedStringAsync(value, QuoteValue, cancel).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="TimeSpan" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(TimeSpan value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    async Task DoWriteValueAsync(TimeSpan value, Cancel cancel)
    {
        await InternalWriteValueAsync(JsonToken.String, cancel).ConfigureAwait(false);
        if (QuoteValue)
        {
            await writer.WriteAsync(quoteChar, cancel).ConfigureAwait(false);
        }

        await writer.WriteAsync(value.ToString(null, InvariantCulture), cancel).ConfigureAwait(false);
        if (QuoteValue)
        {
            await writer.WriteAsync(quoteChar, cancel).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="TimeSpan" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(TimeSpan? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(TimeSpan? value, Cancel cancel) =>
        value == null ? DoWriteNullAsync(cancel) : DoWriteValueAsync(value.GetValueOrDefault(), cancel);

    /// <summary>
    /// Asynchronously writes a <see cref="uint" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(uint value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteIntegerValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="uint" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(uint? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(uint? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return WriteIntegerValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="ulong" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(ulong value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteIntegerValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="ulong" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(ulong? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(ulong? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return WriteIntegerValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Uri" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(Uri? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            if (value == null)
            {
                return WriteNullAsync(cancel);
            }

            return WriteValueNotNullAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task WriteValueNotNullAsync(Uri value, Cancel cancel)
    {
        var task = InternalWriteValueAsync(JsonToken.String, cancel);
        if (task.IsCompletedSuccessfully())
        {
            return WriteEscapedStringAsync(value.OriginalString, QuoteValue, cancel);
        }

        return WriteValueNotNullAsync(task, value, cancel);
    }

    async Task WriteValueNotNullAsync(Task task, Uri value, Cancel cancel)
    {
        await task.ConfigureAwait(false);
        await WriteEscapedStringAsync(value.OriginalString, QuoteValue, cancel).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="ushort" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(ushort value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return WriteIntegerValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}" /> of <see cref="ushort" /> value.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteValueAsync(ushort? value, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteValueAsync(value, cancel);
        }

        return base.WriteValueAsync(value, cancel);
    }

    Task DoWriteValueAsync(ushort? value, Cancel cancel)
    {
        if (value == null)
        {
            return DoWriteNullAsync(cancel);
        }

        return WriteIntegerValueAsync(value.GetValueOrDefault(), cancel);
    }

    /// <summary>
    /// Asynchronously writes a comment <c>/*...*/</c> containing the specified text.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteCommentAsync(string? text, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteCommentAsync(text, cancel);
        }

        return base.WriteCommentAsync(text, cancel);
    }

    async Task DoWriteCommentAsync(string? text, Cancel cancel)
    {
        await InternalWriteCommentAsync(cancel).ConfigureAwait(false);
        await writer.WriteAsync("/*", cancel).ConfigureAwait(false);
        await writer.WriteAsync(text ?? string.Empty, cancel).ConfigureAwait(false);
        await writer.WriteAsync("*/", cancel).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes the end of an array.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteEndArrayAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return InternalWriteEndAsync(JsonContainerType.Array, cancel);
        }

        return base.WriteEndArrayAsync(cancel);
    }

    /// <summary>
    /// Asynchronously writes the end of a JSON object.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteEndObjectAsync(Cancel cancel = default)
    {
        if (safeAsync)
        {
            return InternalWriteEndAsync(JsonContainerType.Object, cancel);
        }

        return base.WriteEndObjectAsync(cancel);
    }

    /// <summary>
    /// Asynchronously writes raw JSON where a value is expected and updates the writer's state.
    /// </summary>
    /// <remarks>
    /// Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.
    /// </remarks>
    public override Task WriteRawValueAsync(string? json, Cancel cancel = default)
    {
        if (safeAsync)
        {
            return DoWriteRawValueAsync(json, cancel);
        }

        return base.WriteRawValueAsync(json, cancel);
    }

    Task DoWriteRawValueAsync(string? json, Cancel cancel)
    {
        UpdateScopeWithFinishedValue();
        var task = AutoCompleteAsync(JsonToken.Undefined, cancel);
        if (task.IsCompletedSuccessfully())
        {
            return WriteRawAsync(json, cancel);
        }

        return DoWriteRawValueAsync(task, json, cancel);
    }

    async Task DoWriteRawValueAsync(Task task, string? json, Cancel cancel)
    {
        await task.ConfigureAwait(false);
        await WriteRawAsync(json, cancel).ConfigureAwait(false);
    }

}