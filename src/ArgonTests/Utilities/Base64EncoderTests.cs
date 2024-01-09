// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Base64EncoderTests : TestFixtureBase
{
    [Fact]
    public void Encode()
    {
        var text = "a";
        var data = Encoding.UTF8.GetBytes(text);
        var writer = new StringWriter();
        var base64Encoder = new Base64Encoder(writer);
        base64Encoder.Encode(data);
        base64Encoder.Flush();
        Assert.Equal("SGVsbG8gd29ybGQu",writer.ToString());
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void Short()
    {
        var text = "a";
        var data = Encoding.UTF8.GetBytes(text);
        var writer = new StringWriter();
        Base64Encoder.Encode2(writer, data);
        writer.Flush();
        Assert.Equal("YQ==", writer.ToString());
    }
#endif
}