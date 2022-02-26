// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests;

public class ExceptionTests : TestFixtureBase
{
    [Fact]
    public void JsonSerializationException()
    {
        var exception = new JsonSerializationException("Message!");
        Assert.Equal("Message!", exception.Message);
        Assert.Equal(null, exception.InnerException);

        exception = new JsonSerializationException("Message!", new Exception("Inner!"));
        Assert.Equal("Message!", exception.Message);
        Assert.Equal("Inner!", exception.InnerException.Message);
    }

    [Fact]
    public void JsonWriterException()
    {
        var exception = new JsonWriterException("Message!", new Exception("Inner!"));
        Assert.Equal("Message!", exception.Message);
        Assert.Equal("Inner!", exception.InnerException.Message);
    }

    [Fact]
    public void JsonReaderException()
    {
        var exception = new JsonReaderException("Message!", new Exception("Inner!"));
        Assert.Equal("Message!", exception.Message);
        Assert.Equal("Inner!", exception.InnerException.Message);
    }
}