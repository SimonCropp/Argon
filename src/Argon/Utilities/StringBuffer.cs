// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

/// <summary>
/// Builds a string. Unlike <see cref="System.Text.StringBuilder" /> this class lets you reuse its internal buffer.
/// </summary>
struct StringBuffer
{
    public int Position { get; set; }

    public bool IsEmpty => InternalBuffer == null;

    public StringBuffer(IArrayPool<char>? bufferPool, int initalSize) : this(BufferUtils.RentBuffer(bufferPool, initalSize))
    {
    }

    StringBuffer(char[] buffer)
    {
        InternalBuffer = buffer;
        Position = 0;
    }

    public void Append(IArrayPool<char>? bufferPool, char value)
    {
        // test if the buffer array is large enough to take the value
        if (Position == InternalBuffer!.Length)
        {
            EnsureSize(bufferPool, 1);
        }

        // set value and increment poisition
        InternalBuffer![Position++] = value;
    }

    public void Append(IArrayPool<char>? bufferPool, char[] buffer, int startIndex, int count)
    {
        if (Position + count >= InternalBuffer!.Length)
        {
            EnsureSize(bufferPool, count);
        }

        Array.Copy(buffer, startIndex, InternalBuffer, Position, count);

        Position += count;
    }

    public void Clear(IArrayPool<char>? bufferPool)
    {
        if (InternalBuffer != null)
        {
            BufferUtils.ReturnBuffer(bufferPool, InternalBuffer);
            InternalBuffer = null;
        }

        Position = 0;
    }

    void EnsureSize(IArrayPool<char>? bufferPool, int appendLength)
    {
        var newBuffer = BufferUtils.RentBuffer(bufferPool, (Position + appendLength) * 2);

        if (InternalBuffer != null)
        {
            Array.Copy(InternalBuffer, newBuffer, Position);
            BufferUtils.ReturnBuffer(bufferPool, InternalBuffer);
        }

        InternalBuffer = newBuffer;
    }

    public override string ToString()
    {
        return ToString(0, Position);
    }

    public string ToString(int start, int length)
    {
        // TODO: validation
        return new(InternalBuffer!, start, length);
    }

    public char[]? InternalBuffer { get; private set; }
}