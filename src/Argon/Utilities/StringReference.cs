// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

readonly struct StringReference
{
    public char this[int i] => Chars[i];

    public char[] Chars { get; }

    public int StartIndex { get; }

    public int Length { get; }

    public StringReference(char[] chars, int startIndex, int length)
    {
        Chars = chars;
        StartIndex = startIndex;
        Length = length;
    }

    public override string ToString() =>
        new(Chars, StartIndex, Length);

    public bool StartsWith(string text)
    {
        if (text.Length > Length)
        {
            return false;
        }

        var chars = Chars;

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != chars[i + StartIndex])
            {
                return false;
            }
        }

        return true;
    }
}