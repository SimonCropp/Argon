// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

class Base64Encoder(TextWriter writer)
{
    const int Base64LineSize = 76;
    const int LineSizeInBytes = 57;

    readonly char[] charsLine = new char[Base64LineSize];

    byte[]? leftOverBytes;
    int leftOverBytesCount;

    public void Encode(byte[] buffer)
    {
        var count = buffer.Length;
        if (leftOverBytesCount > 0)
        {
            if (FulfillFromLeftover(buffer, ref count))
            {
                return;
            }

            var num2 = Convert.ToBase64CharArray(leftOverBytes!, 0, 3, charsLine, 0);
            WriteChars(charsLine, 0, num2);
        }

        StoreLeftOverBytes(buffer, ref count);

        var index = 0;
        var length = LineSizeInBytes;
        while (index < count)
        {
            if (index + length > count)
            {
                length = count - index;
            }

            var num6 = Convert.ToBase64CharArray(buffer, index, length, charsLine, 0);
            WriteChars(charsLine, 0, num6);
            index += length;
        }
    }

    void StoreLeftOverBytes(byte[] buffer, ref int count)
    {
        var leftOverBytesCount = count % 3;
        if (leftOverBytesCount > 0)
        {
            count -= leftOverBytesCount;
            leftOverBytes ??= new byte[3];

            for (var i = 0; i < leftOverBytesCount; i++)
            {
                leftOverBytes[i] = buffer[count + i];
            }
        }

        this.leftOverBytesCount = leftOverBytesCount;
    }

    bool FulfillFromLeftover(byte[] buffer, ref int count)
    {
        var index = 0;
        var leftOverBytesCount = this.leftOverBytesCount;
        while (leftOverBytesCount < 3 && count > 0)
        {
            leftOverBytes![leftOverBytesCount++] = buffer[index++];
            count--;
        }

        if (count == 0 && leftOverBytesCount < 3)
        {
            this.leftOverBytesCount = leftOverBytesCount;
            return true;
        }

        return false;
    }

    public void Flush()
    {
        if (leftOverBytesCount > 0)
        {
            var count = Convert.ToBase64CharArray(leftOverBytes!, 0, leftOverBytesCount, charsLine, 0);
            WriteChars(charsLine, 0, count);
            leftOverBytesCount = 0;
        }
    }

    void WriteChars(char[] chars, int index, int count) =>
        writer.Write(chars, index, count);

    public async Task EncodeAsync(byte[] buffer, Cancel cancel)
    {
        var count = buffer.Length;
        var index = 0;
        if (leftOverBytesCount > 0)
        {
            if (FulfillFromLeftover(buffer, ref count))
            {
                return;
            }

            var num2 = Convert.ToBase64CharArray(leftOverBytes!, 0, 3, charsLine, 0);
            await WriteCharsAsync(charsLine, 0, num2, cancel).ConfigureAwait(false);
        }

        StoreLeftOverBytes(buffer, ref count);

        var num4 = buffer.Length;
        var length = LineSizeInBytes;
        while (index < num4)
        {
            if (index + length > num4)
            {
                length = num4 - index;
            }

            var num6 = Convert.ToBase64CharArray(buffer, index, length, charsLine, 0);
            await WriteCharsAsync(charsLine, 0, num6, cancel).ConfigureAwait(false);
            index += length;
        }
    }

    Task WriteCharsAsync(char[] chars, int index, int count, Cancel cancel) =>
        writer.WriteAsync(chars, index, count, cancel);

    public Task FlushAsync(Cancel cancel)
    {
        if (cancel.IsCancellationRequested)
        {
            return cancel.FromCanceled();
        }

        if (leftOverBytesCount > 0)
        {
            var count = Convert.ToBase64CharArray(leftOverBytes!, 0, leftOverBytesCount, charsLine, 0);
            leftOverBytesCount = 0;
            return WriteCharsAsync(charsLine, 0, count, cancel);
        }

        return Task.CompletedTask;
    }
}