// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class StringUtils
{
    public const string CarriageReturnLineFeed = "\r\n";
    public const char CarriageReturn = '\r';
    public const char LineFeed = '\n';
    public const char Tab = '\t';

    public static bool IsNullOrEmpty([NotNullWhen(false)] string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Determines whether the string is all white space. Empty string will return <c>false</c>.
    /// </summary>
    /// <param name="s">The string to test whether it is all white space.</param>
    /// <returns>
    /// <c>true</c> if the string is all white space; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsWhiteSpace(string s)
    {
        if (s.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < s.Length; i++)
        {
            if (!char.IsWhiteSpace(s[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static StringWriter CreateStringWriter(int capacity)
    {
        var stringBuilder = new StringBuilder(capacity);
        return new(stringBuilder, CultureInfo.InvariantCulture);
    }

    public static void ToCharAsUnicode(char c, char[] buffer)
    {
        buffer[0] = '\\';
        buffer[1] = 'u';
        buffer[2] = MathUtils.IntToHex((c >> 12) & '\x000f');
        buffer[3] = MathUtils.IntToHex((c >> 8) & '\x000f');
        buffer[4] = MathUtils.IntToHex((c >> 4) & '\x000f');
        buffer[5] = MathUtils.IntToHex(c & '\x000f');
    }

    public static TSource ForgivingCaseSensitiveFind<TSource>(this IEnumerable<TSource> source, Func<TSource, string> valueSelector, string testValue)
    {
        var caseInsensitiveResults = source.Where(s => string.Equals(valueSelector(s), testValue, StringComparison.OrdinalIgnoreCase));
        if (caseInsensitiveResults.Count() <= 1)
        {
            return caseInsensitiveResults.SingleOrDefault()!;
        }

        // multiple results returned. now filter using case sensitivity
        var caseSensitiveResults = source.Where(s => string.Equals(valueSelector(s), testValue, StringComparison.Ordinal));
        return caseSensitiveResults.SingleOrDefault()!;
    }

    public static string ToCamelCase(string s)
    {
        if (IsNullOrEmpty(s) || !char.IsUpper(s[0]))
        {
            return s;
        }

        var chars = s.ToCharArray();

        for (var i = 0; i < chars.Length; i++)
        {
            if (i == 1 && !char.IsUpper(chars[i]))
            {
                break;
            }

            var hasNext = i + 1 < chars.Length;
            if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
            {
                // if the next character is a space, which is not considered uppercase
                // (otherwise we wouldn't be here...)
                // we want to ensure that the following:
                // 'FOO bar' is rewritten as 'foo bar', and not as 'foO bar'
                // The code was written in such a way that the first word in uppercase
                // ends when if finds an uppercase letter followed by a lowercase letter.
                // now a ' ' (space, (char)32) is considered not upper
                // but in that case we still want our current character to become lowercase
                if (char.IsSeparator(chars[i + 1]))
                {
                    chars[i] = ToLower(chars[i]);
                }

                break;
            }

            chars[i] = ToLower(chars[i]);
        }

        return new(chars);
    }

    static char ToLower(char c)
    {
        return char.ToLower(c, CultureInfo.InvariantCulture);
    }

    public static string ToSnakeCase(string s)
    {
        return ToSeparatedCase(s, '_');
    }

    public static string ToKebabCase(string s)
    {
        return ToSeparatedCase(s, '-');
    }

    enum SeparatedCaseState
    {
        Start,
        Lower,
        Upper,
        NewWord
    }

    static string ToSeparatedCase(string s, char separator)
    {
        if (IsNullOrEmpty(s))
        {
            return s;
        }

        var stringBuilder = new StringBuilder();
        var state = SeparatedCaseState.Start;

        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == ' ')
            {
                if (state != SeparatedCaseState.Start)
                {
                    state = SeparatedCaseState.NewWord;
                }
            }
            else if (char.IsUpper(s[i]))
            {
                switch (state)
                {
                    case SeparatedCaseState.Upper:
                        var hasNext = i + 1 < s.Length;
                        if (i > 0 && hasNext)
                        {
                            var nextChar = s[i + 1];
                            if (!char.IsUpper(nextChar) && nextChar != separator)
                            {
                                stringBuilder.Append(separator);
                            }
                        }

                        break;
                    case SeparatedCaseState.Lower:
                    case SeparatedCaseState.NewWord:
                        stringBuilder.Append(separator);
                        break;
                }

                var c = char.ToLower(s[i], CultureInfo.InvariantCulture);
                stringBuilder.Append(c);

                state = SeparatedCaseState.Upper;
            }
            else if (s[i] == separator)
            {
                stringBuilder.Append(separator);
                state = SeparatedCaseState.Start;
            }
            else
            {
                if (state == SeparatedCaseState.NewWord)
                {
                    stringBuilder.Append(separator);
                }

                stringBuilder.Append(s[i]);
                state = SeparatedCaseState.Lower;
            }
        }

        return stringBuilder.ToString();
    }

    public static bool IsHighSurrogate(char c)
    {
        return char.IsHighSurrogate(c);
    }

    public static bool IsLowSurrogate(char c)
    {
        return char.IsLowSurrogate(c);
    }

    public static bool StartsWith(this string source, char value)
    {
        return source.Length > 0 && source[0] == value;
    }

    public static bool EndsWith(this string source, char value)
    {
        return source.Length > 0 && source[source.Length - 1] == value;
    }

    public static string Trim(this string s, int start, int length)
    {
        // References: https://referencesource.microsoft.com/#mscorlib/system/string.cs,2691
        // https://referencesource.microsoft.com/#mscorlib/system/string.cs,1226
        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        var end = start + length - 1;
        if (end >= s.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        for (; start < end; start++)
        {
            if (!char.IsWhiteSpace(s[start]))
            {
                break;
            }
        }

        for (; end >= start; end--)
        {
            if (!char.IsWhiteSpace(s[end]))
            {
                break;
            }
        }

        return s.Substring(start, end - start + 1);
    }
}