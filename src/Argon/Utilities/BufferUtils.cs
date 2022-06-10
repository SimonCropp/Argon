// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Buffers;

static class BufferUtils
{
    public static char[] RentBuffer(int minSize) =>
        ArrayPool<char>.Shared.Rent(minSize);

    public static void ReturnBuffer(char[]? buffer)
    {
        if (buffer != null)
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    public static char[] EnsureBufferSize(int size, char[]? buffer)
    {
        if (buffer != null)
        {
            ArrayPool<char>.Shared.Return(buffer);
        }

        return ArrayPool<char>.Shared.Rent(size);
    }
}