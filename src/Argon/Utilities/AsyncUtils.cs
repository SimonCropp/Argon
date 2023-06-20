// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class AsyncUtils
{
    // Pre-allocate to avoid wasted allocations.
    public static readonly Task<bool> False = Task.FromResult(false);
    public static readonly Task<bool> True = Task.FromResult(true);

    internal static Task<bool> ToAsync(this bool value) =>
        value ? True : False;

    public static Task? CancelIfRequestedAsync(this Cancel cancel) =>
        cancel.IsCancellationRequested ? FromCanceled(cancel) : null;

    public static Task<T>? CancelIfRequestedAsync<T>(this Cancel cancel) =>
        cancel.IsCancellationRequested ? FromCanceled<T>(cancel) : null;

    // From 4.6 on we could use Task.FromCanceled(), but we need an equivalent for
    // previous frameworks.
    public static Task FromCanceled(this Cancel cancel)
    {
        MiscellaneousUtils.Assert(cancel.IsCancellationRequested);
        return new(
            () =>
            {
            },
            cancel);
    }

    static Task<T> FromCanceled<T>(this Cancel cancel)
    {
        MiscellaneousUtils.Assert(cancel.IsCancellationRequested);
#pragma warning disable CS8603 // Possible null reference return.
        return new(() => default, cancel);
#pragma warning restore CS8603 // Possible null reference return.
    }

    public static Task WriteAsync(this TextWriter writer, char value, Cancel cancel) =>
        cancel.IsCancellationRequested ? FromCanceled(cancel) : writer.WriteAsync(value);

    public static Task WriteAsync(this TextWriter writer, string? value, Cancel cancel) =>
        cancel.IsCancellationRequested ? FromCanceled(cancel) : writer.WriteAsync(value);

    public static Task WriteAsync(this TextWriter writer, char[] value, int start, int count, Cancel cancel) =>
        cancel.IsCancellationRequested ? FromCanceled(cancel) : writer.WriteAsync(value, start, count);

    public static Task<int> ReadAsync(this TextReader reader, char[] buffer, int index, int count, Cancel cancel) =>
        cancel.IsCancellationRequested ? FromCanceled<int>(cancel) : reader.ReadAsync(buffer, index, count);

    public static bool IsCompletedSuccessfully(this Task task) =>
        task.Status == TaskStatus.RanToCompletion;
}