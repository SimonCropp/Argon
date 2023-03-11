// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public abstract partial class JsonReader
#if NET5_0_OR_GREATER
        : IAsyncDisposable
{
        ValueTask IAsyncDisposable.DisposeAsync()
        {
            try
            {
                Dispose(true);
                return default;
            }
            catch (Exception exc)
            {
                return ValueTask.FromException(exc);
            }
        }
#else
{
#endif
    /// <summary>
    /// Asynchronously reads the next JSON token from the source.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
    /// </returns>
    public virtual Task<bool> ReadAsync(Cancellation cancellation = default) =>
        cancellation.CancelIfRequestedAsync<bool>() ?? Read().ToAsync();

    /// <summary>
    /// Asynchronously skips the children of the current token.
    /// </summary>
    public async Task SkipAsync(Cancellation cancellation = default)
    {
        if (TokenType == JsonToken.PropertyName)
        {
            await ReadAsync(cancellation).ConfigureAwait(false);
        }

        if (TokenType.IsStartToken())
        {
            var depth = Depth;

            while (await ReadAsync(cancellation).ConfigureAwait(false) && depth < Depth)
            {
            }
        }
    }

    internal async Task ReaderReadAndAssertAsync(Cancellation cancellation)
    {
        if (!await ReadAsync(cancellation).ConfigureAwait(false))
        {
            throw CreateUnexpectedEndException();
        }
    }

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="bool" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="bool" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    public virtual Task<bool?> ReadAsBooleanAsync(Cancellation cancellation = default) =>
        cancellation.CancelIfRequestedAsync<bool?>() ?? Task.FromResult(ReadAsBoolean());

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="byte" />[].
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="byte" />[]. This result will be <c>null</c> at the end of an array.
    /// </returns>
    public virtual Task<byte[]?> ReadAsBytesAsync(Cancellation cancellation = default) =>
        cancellation.CancelIfRequestedAsync<byte[]?>() ?? Task.FromResult(ReadAsBytes());

    internal async Task<byte[]?> ReadArrayIntoByteArrayAsync(Cancellation cancellation)
    {
        var buffer = new List<byte>();

        while (true)
        {
            if (!await ReadAsync(cancellation).ConfigureAwait(false))
            {
                SetToken(JsonToken.None);
            }

            if (ReadArrayElementIntoByteArrayReportDone(buffer))
            {
                var d = buffer.ToArray();
                SetToken(d);
                return d;
            }
        }
    }

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="DateTime" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="DateTime" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    public virtual Task<DateTime?> ReadAsDateTimeAsync(Cancellation cancellation = default) =>
        cancellation.CancelIfRequestedAsync<DateTime?>() ?? Task.FromResult(ReadAsDateTime());

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="DateTimeOffset" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="DateTimeOffset" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    public virtual Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(Cancellation cancellation = default) =>
        cancellation.CancelIfRequestedAsync<DateTimeOffset?>() ?? Task.FromResult(ReadAsDateTimeOffset());

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="decimal" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="decimal" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    public virtual Task<decimal?> ReadAsDecimalAsync(Cancellation cancellation = default) =>
        cancellation.CancelIfRequestedAsync<decimal?>() ?? Task.FromResult(ReadAsDecimal());

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="double" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="double" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    public virtual Task<double?> ReadAsDoubleAsync(Cancellation cancellation = default) =>
        Task.FromResult(ReadAsDouble());

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="int" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="Nullable{T}" /> of <see cref="int" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    public virtual Task<int?> ReadAsInt32Async(Cancellation cancellation = default) =>
        cancellation.CancelIfRequestedAsync<int?>() ?? Task.FromResult(ReadAsInt32());

    /// <summary>
    /// Asynchronously reads the next JSON token from the source as a <see cref="string" />.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous read. The <see cref="Task{TResult}.Result" />
    /// property returns the <see cref="string" />. This result will be <c>null</c> at the end of an array.
    /// </returns>
    public virtual Task<string?> ReadAsStringAsync(Cancellation cancellation = default) =>
        cancellation.CancelIfRequestedAsync<string?>() ?? Task.FromResult(ReadAsString());

    internal async Task<bool> ReadAndMoveToContentAsync(Cancellation cancellation) =>
        await ReadAsync(cancellation).ConfigureAwait(false) && await MoveToContentAsync(cancellation).ConfigureAwait(false);

    internal Task<bool> MoveToContentAsync(Cancellation cancellation)
    {
        switch (TokenType)
        {
            case JsonToken.None:
            case JsonToken.Comment:
                return MoveToContentFromNonContentAsync(cancellation);
            default:
                return AsyncUtils.True;
        }
    }

    async Task<bool> MoveToContentFromNonContentAsync(Cancellation cancellation)
    {
        while (true)
        {
            if (!await ReadAsync(cancellation).ConfigureAwait(false))
            {
                return false;
            }

            switch (TokenType)
            {
                case JsonToken.None:
                case JsonToken.Comment:
                    break;
                default:
                    return true;
            }
        }
    }
}