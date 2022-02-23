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

public class Issue1569 : TestFixtureBase
{
    [Fact]
    public async Task Test()
    {
        var json = "[1,2,3,456789999999999999999999999999999999999999999999999999999999999999456789999999999999999999999999999999999999999999999999999999999999456789999999999999999999999999999999999999999999999999999999999999]";

        Stream s = new AsyncOnlyStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        var sr = new StreamReader(s, Encoding.UTF8, true, 2);
        var reader = new JsonTextReader(sr);
#if !RELEASE
        reader.CharBuffer = new char[2];
#endif

        while (await reader.ReadAsync())
        {
        }
    }

    public class AsyncOnlyStream : Stream
    {
        readonly Stream innerStream;

        public AsyncOnlyStream(Stream innerStream)
        {
            this.innerStream = innerStream;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override Task FlushAsync(CancellationToken cancellation)
        {
            return innerStream.FlushAsync(cancellation);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellation)
        {
            return innerStream.ReadAsync(buffer, offset, count, cancellation);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellation)
        {
            return innerStream.WriteAsync(buffer, offset, count, cancellation);
        }

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