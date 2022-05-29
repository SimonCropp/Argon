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
}

static class StringReferenceExtensions
{
    public static bool StartsWith(this StringReference s, string text)
    {
        if (text.Length > s.Length)
        {
            return false;
        }

        var chars = s.Chars;

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != chars[i + s.StartIndex])
            {
                return false;
            }
        }

        return true;
    }

    public static bool EndsWith(this StringReference s, string text)
    {
        if (text.Length > s.Length)
        {
            return false;
        }

        var chars = s.Chars;

        var start = s.StartIndex + s.Length - text.Length;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != chars[i + start])
            {
                return false;
            }
        }

        return true;
    }
}