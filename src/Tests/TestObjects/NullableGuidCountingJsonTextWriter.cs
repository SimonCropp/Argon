// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class NullableGuidCountingJsonTextWriter : JsonTextWriter
{
    public NullableGuidCountingJsonTextWriter(TextWriter textWriter)
        : base(textWriter)
    {
    }

    public int NullableGuidCount { get; private set; }

    public override void WriteValue(Guid? value)
    {
        base.WriteValue(value);
        ++NullableGuidCount;
    }
}