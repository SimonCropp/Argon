// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class UnmanagedResourceFakingJsonReader : JsonReader
{
    public static int DisposalCalls;

    public static void CreateAndDispose()
    {
        ((IDisposable)new UnmanagedResourceFakingJsonReader()).Dispose();
    }

    public UnmanagedResourceFakingJsonReader()
    {
        DisposalCalls = 0;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ++DisposalCalls;
    }

    ~UnmanagedResourceFakingJsonReader()
    {
        Dispose(false);
    }

    public override bool Read()
    {
        throw new NotImplementedException();
    }

    public override byte[] ReadAsBytes()
    {
        throw new NotImplementedException();
    }

    public override DateTime? ReadAsDateTime()
    {
        throw new NotImplementedException();
    }

    public override DateTimeOffset? ReadAsDateTimeOffset()
    {
        throw new NotImplementedException();
    }

    public override decimal? ReadAsDecimal()
    {
        throw new NotImplementedException();
    }

    public override int? ReadAsInt32()
    {
        throw new NotImplementedException();
    }

    public override string ReadAsString()
    {
        throw new NotImplementedException();
    }
}