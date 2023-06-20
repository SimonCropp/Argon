// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1569 : TestFixtureBase
{
    [Fact]
    public async Task Test()
    {
        var json = "[1,2,3,456789999999999999999999999999999999999999999999999999999999999999456789999999999999999999999999999999999999999999999999999999999999456789999999999999999999999999999999999999999999999999999999999999]";

        Stream s = new AsyncOnlyStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        var sr = new StreamReader(s, Encoding.UTF8, true, 2);
        var reader = new JsonTextReader(sr, 2);

        while (await reader.ReadAsync())
        {
        }
    }

    public class AsyncOnlyStream : Stream
    {
        readonly Stream innerStream;

        public AsyncOnlyStream(Stream innerStream) =>
            this.innerStream = innerStream;

        public override void Flush() =>
            throw new NotSupportedException();

        public override Task FlushAsync(Cancel cancel) =>
            innerStream.FlushAsync(cancel);

        public override long Seek(long offset, SeekOrigin origin) =>
            innerStream.Seek(offset, origin);

        public override void SetLength(long value) =>
            innerStream.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, Cancel cancel) =>
            innerStream.ReadAsync(buffer, offset, count, cancel);

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override Task WriteAsync(byte[] buffer, int offset, int count, Cancel cancel) =>
            innerStream.WriteAsync(buffer, offset, count, cancel);

        public override bool CanRead => innerStream.CanRead;
        public override bool CanSeek => innerStream.CanSeek;
        public override bool CanWrite => innerStream.CanWrite;
        public override long Length => innerStream.Length;

        public override long Position
        {
            get => innerStream.Position;
            set => innerStream.Position = value;
        }
    }
}