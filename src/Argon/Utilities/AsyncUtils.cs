// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class AsyncUtils
{
    // Pre-allocate to avoid wasted allocations.
    public static readonly Task<bool> False = Task.FromResult(false);
    public static readonly Task<bool> True = Task.FromResult(true);

    internal static Task<bool> ToAsync(this bool value)
    {
        return value ? True : False;
    }

    public static Task? CancelIfRequestedAsync(this CancellationToken cancellation)
    {
        return cancellation.IsCancellationRequested ? FromCanceled(cancellation) : null;
    }

    public static Task<T>? CancelIfRequestedAsync<T>(this CancellationToken cancellation)
    {
        return cancellation.IsCancellationRequested ? FromCanceled<T>(cancellation) : null;
    }

    // From 4.6 on we could use Task.FromCanceled(), but we need an equivalent for
    // previous frameworks.
    public static Task FromCanceled(this CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(cancellation.IsCancellationRequested);
        return new(
            () =>
            {
            },
            cancellation);
    }

    static Task<T> FromCanceled<T>(this CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(cancellation.IsCancellationRequested);
#pragma warning disable CS8603 // Possible null reference return.
        return new(() => default, cancellation);
#pragma warning restore CS8603 // Possible null reference return.
    }

    // Task.Delay(0) is optimised as a cached task within the framework, and indeed
    // the same cached task that Task.CompletedTask returns as of 4.6, but we'll add
    // our own cached field for previous frameworks.
    internal static readonly Task CompletedTask = Task.Delay(0);

    public static Task WriteAsync(this TextWriter writer, char value, CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(writer != null);
        return cancellation.IsCancellationRequested ? FromCanceled(cancellation) : writer.WriteAsync(value);
    }

    public static Task WriteAsync(this TextWriter writer, string? value, CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(writer != null);
        return cancellation.IsCancellationRequested ? FromCanceled(cancellation) : writer.WriteAsync(value);
    }

    public static Task WriteAsync(this TextWriter writer, char[] value, int start, int count, CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(writer != null);
        return cancellation.IsCancellationRequested ? FromCanceled(cancellation) : writer.WriteAsync(value, start, count);
    }

    public static Task<int> ReadAsync(this TextReader reader, char[] buffer, int index, int count, CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(reader != null);
        return cancellation.IsCancellationRequested ? FromCanceled<int>(cancellation) : reader.ReadAsync(buffer, index, count);
    }

    public static bool IsCompletedSucessfully(this Task task)
    {
        // IsCompletedSucessfully is the faster method, but only currently exposed on .NET Core 2.0
#if NETCOREAPP2_0
            return task.IsCompletedSucessfully;
#else
        return task.Status == TaskStatus.RanToCompletion;
#endif
    }
}