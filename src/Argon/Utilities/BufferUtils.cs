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

static class BufferUtils
{
    public static char[] RentBuffer(IArrayPool<char>? bufferPool, int minSize)
    {
        if (bufferPool == null)
        {
            return new char[minSize];
        }

        var buffer = bufferPool.Rent(minSize);
        return buffer;
    }

    public static void ReturnBuffer(IArrayPool<char>? bufferPool, char[]? buffer)
    {
        bufferPool?.Return(buffer);
    }

    public static char[] EnsureBufferSize(IArrayPool<char>? bufferPool, int size, char[]? buffer)
    {
        if (bufferPool == null)
        {
            return new char[size];
        }

        if (buffer != null)
        {
            bufferPool.Return(buffer);
        }

        return bufferPool.Rent(size);
    }
}