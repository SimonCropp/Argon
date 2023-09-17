// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class SlowStream(byte[] content, int bytesPerRead) : Stream
{
    int totalBytesRead;

    public SlowStream(string content, Encoding encoding, int bytesPerRead)
        : this(encoding.GetBytes(content), bytesPerRead)
    {
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override void Flush()
    {
    }

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var toReturn = Math.Min(count, bytesPerRead);
        toReturn = Math.Min(toReturn, content.Length - totalBytesRead);
        if (toReturn > 0)
        {
            Array.Copy(content, totalBytesRead, buffer, offset, toReturn);
        }

        totalBytesRead += toReturn;
        return toReturn;
    }

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    public override void SetLength(long value) =>
        throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();
}