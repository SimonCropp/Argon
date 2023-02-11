// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if NET7_0
public class Issue2694 : TestFixtureBase
{
    [Fact]
    public async Task Test_Reader_DisposeAsync()
    {
        var reader = new JsonTextReader(new StringReader("{}"));
        IAsyncDisposable asyncDisposable = reader;
        await asyncDisposable.DisposeAsync();

        var exception = await Assert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync());
        Assert.Equal("Unexpected state: Closed. Path '', line 1, position 0.", exception.Message);
    }

    [Fact]
    public async Task Test_Writer_DisposeAsync()
    {
        var ms = new MemoryStream();
        Stream s = new AsyncOnlyStream(ms);
        var sr = new StreamWriter(s, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 2, leaveOpen: true);
        await using (var writer = new JsonTextWriter(sr))
        {
            await writer.WriteStartObjectAsync();
        }

        string json = Encoding.UTF8.GetString(ms.ToArray());
        Assert.Equal("{}", json);
    }

    [Fact]
    public async Task Test_Writer_CloseAsync()
    {
        var ms = new MemoryStream();
        Stream s = new AsyncOnlyStream(ms);
        var sr = new StreamWriter(s, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 2, leaveOpen: true);
        var writer = new JsonTextWriter(sr);
        await writer.WriteStartObjectAsync();

        await writer.CloseAsync();

        var json = Encoding.UTF8.GetString(ms.ToArray());
        Assert.Equal("{}", json);
    }

    public class AsyncOnlyStream : Stream
    {
        private readonly Stream _innerStream;
        private int _unflushedContentLength;

        public AsyncOnlyStream(Stream innerStream) =>
            _innerStream = innerStream;

        public override void Flush()
        {
            // It's ok to call Flush if the content was already processed with FlushAsync.
            if (_unflushedContentLength > 0)
            {
                throw new($"Flush when there is {_unflushedContentLength} bytes buffered.");
            }
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            _unflushedContentLength = 0;
            return _innerStream.FlushAsync(cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            _innerStream.Seek(offset, origin);

        public override void SetLength(long value) =>
            _innerStream.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            _innerStream.ReadAsync(buffer, offset, count, cancellationToken);

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _unflushedContentLength += count;
            return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;

        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }
    }
}

#endif