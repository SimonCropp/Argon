// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public sealed class JsonReaderStubWithIsClosed : JsonReader
{
    public bool IsClosed { get; private set; }

    public override void Close()
    {
        IsClosed = true;
    }

    public override bool Read()
    {
        throw new NotSupportedException();
    }
}