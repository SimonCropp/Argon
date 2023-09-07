// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ToggleReaderError(TextReader inner) : TextReader
{
    public bool Error { get; set; }

    public override int Read(char[] buffer, int index, int count)
    {
        if (Error)
        {
            throw new("Read error");
        }

        return inner.Read(buffer, index, 1);
    }
}