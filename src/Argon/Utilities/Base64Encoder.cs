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

class Base64Encoder
{
    const int Base64LineSize = 76;
    const int LineSizeInBytes = 57;

    readonly char[] charsLine = new char[Base64LineSize];
    readonly TextWriter writer;

    byte[]? leftOverBytes;
    int leftOverBytesCount;

    public Base64Encoder(TextWriter writer)
    {
        this.writer = writer;
    }

    static void ValidateEncode(byte[] buffer, int index, int count)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count > buffer.Length - index)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
    }

    public void Encode(byte[] buffer, int index, int count)
    {
        ValidateEncode(buffer, index, count);

        if (leftOverBytesCount > 0)
        {
            if(FulfillFromLeftover(buffer, index, ref count))
            {
                return;
            }

            var num2 = Convert.ToBase64CharArray(leftOverBytes, 0, 3, charsLine, 0);
            WriteChars(charsLine, 0, num2);
        }

        StoreLeftOverBytes(buffer, index, ref count);

        var num4 = index + count;
        var length = LineSizeInBytes;
        while (index < num4)
        {
            if (index + length > num4)
            {
                length = num4 - index;
            }
            var num6 = Convert.ToBase64CharArray(buffer, index, length, charsLine, 0);
            WriteChars(charsLine, 0, num6);
            index += length;
        }
    }

    void StoreLeftOverBytes(byte[] buffer, int index, ref int count)
    {
        var leftOverBytesCount = count % 3;
        if (leftOverBytesCount > 0)
        {
            count -= leftOverBytesCount;
            leftOverBytes ??= new byte[3];

            for (var i = 0; i < leftOverBytesCount; i++)
            {
                leftOverBytes[i] = buffer[index + count + i];
            }
        }

        this.leftOverBytesCount = leftOverBytesCount;
    }

    bool FulfillFromLeftover(byte[] buffer, int index, ref int count)
    {
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
            var count = Convert.ToBase64CharArray(leftOverBytes, 0, leftOverBytesCount, charsLine, 0);
            WriteChars(charsLine, 0, count);
            leftOverBytesCount = 0;
        }
    }

    void WriteChars(char[] chars, int index, int count)
    {
        writer.Write(chars, index, count);
    }

    public async Task EncodeAsync(byte[] buffer, int index, int count, CancellationToken cancellation)
    {
        ValidateEncode(buffer, index, count);

        if (leftOverBytesCount > 0)
        {
            if (FulfillFromLeftover(buffer, index, ref count))
            {
                return;
            }

            var num2 = Convert.ToBase64CharArray(leftOverBytes, 0, 3, charsLine, 0);
            await WriteCharsAsync(charsLine, 0, num2, cancellation).ConfigureAwait(false);
        }

        StoreLeftOverBytes(buffer, index, ref count);

        var num4 = index + count;
        var length = LineSizeInBytes;
        while (index < num4)
        {
            if (index + length > num4)
            {
                length = num4 - index;
            }
            var num6 = Convert.ToBase64CharArray(buffer, index, length, charsLine, 0);
            await WriteCharsAsync(charsLine, 0, num6, cancellation).ConfigureAwait(false);
            index += length;
        }
    }

    Task WriteCharsAsync(char[] chars, int index, int count, CancellationToken cancellation)
    {
        return writer.WriteAsync(chars, index, count, cancellation);
    }

    public Task FlushAsync(CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
        {
            return cancellation.FromCanceled();
        }

        if (leftOverBytesCount > 0)
        {
            var count = Convert.ToBase64CharArray(leftOverBytes, 0, leftOverBytesCount, charsLine, 0);
            leftOverBytesCount = 0;
            return WriteCharsAsync(charsLine, 0, count, cancellation);
        }

        return AsyncUtils.CompletedTask;
    }
}