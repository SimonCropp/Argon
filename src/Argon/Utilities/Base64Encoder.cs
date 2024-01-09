// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.


class Base64Encoder(TextWriter writer)
{
    public static void Encode2(TextWriter writer, ReadOnlySpan<byte> buffer)
    {
        var index = 0;
        Span<char> target = stackalloc char[Base64LineSize];
        do
        {
            var min = Math.Min(LineSizeInBytes, buffer.Length - index );
#if NET8_0_OR_GREATER
            var slice = buffer.Slice(index, min);
            Convert.TryToBase64Chars(slice, target, out var charsWritten);
            index += LineSizeInBytes;
            writer.Write(target[..charsWritten]);
#else
#endif
        } while (index < buffer.Length);
    }
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
            WriteChars(charsLine, num2);
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
            WriteChars(charsLine, num6);
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
            WriteChars(charsLine, count);
            leftOverBytesCount = 0;
        }
    }

    void WriteChars(char[] chars, int count) =>
        writer.Write(chars, 0, count);
}