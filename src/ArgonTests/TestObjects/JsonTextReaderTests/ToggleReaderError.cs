// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ToggleReaderError : TextReader
{
    readonly TextReader _inner;

    public bool Error { get; set; }

    public ToggleReaderError(TextReader inner) =>
        _inner = inner;

    public override int Read(char[] buffer, int index, int count)
    {
        if (Error)
        {
            throw new("Read error");
        }

        return _inner.Read(buffer, index, 1);
    }
}