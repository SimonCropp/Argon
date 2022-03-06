// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class JavaScriptUtils
{
    internal static readonly bool[] SingleQuoteEscapeFlags = new bool[128];
    internal static readonly bool[] DoubleQuoteEscapeFlags = new bool[128];
    internal static readonly bool[] HtmlEscapeFlags = new bool[128];

    const int unicodeTextLength = 6;

    static JavaScriptUtils()
    {
        var escapeChars = new List<char>
        {
            '\n', '\r', '\t', '\\', '\f', '\b'
        };
        for (var i = 0; i < ' '; i++)
        {
            escapeChars.Add((char) i);
        }

        foreach (var escapeChar in escapeChars.Union(new[] {'\''}))
        {
            SingleQuoteEscapeFlags[escapeChar] = true;
        }

        foreach (var escapeChar in escapeChars.Union(new[] {'"'}))
        {
            DoubleQuoteEscapeFlags[escapeChar] = true;
        }

        foreach (var escapeChar in escapeChars.Union(new[] {'"', '\'', '<', '>', '&'}))
        {
            HtmlEscapeFlags[escapeChar] = true;
        }
    }

    const string escapedUnicodeText = "!";

    public static bool[] GetCharEscapeFlags(EscapeHandling escapeHandling, char quoteChar)
    {
        if (escapeHandling == EscapeHandling.EscapeHtml)
        {
            return HtmlEscapeFlags;
        }

        if (quoteChar == '"')
        {
            return DoubleQuoteEscapeFlags;
        }

        return SingleQuoteEscapeFlags;
    }

    public static bool ShouldEscapeJavaScriptString(string? s, bool[] escapeFlags)
    {
        if (s == null)
        {
            return false;
        }

        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c >= escapeFlags.Length || escapeFlags[c])
            {
                return true;
            }
        }

        return false;
    }

    public static void WriteEscapedJavaScriptString(TextWriter writer, string? s, char delimiter, bool appendDelimiters, bool[] escapeFlags, EscapeHandling escapeHandling, IArrayPool<char>? bufferPool, ref char[]? buffer)
    {
        // leading delimiter
        if (appendDelimiters)
        {
            writer.Write(delimiter);
        }

        if (!StringUtils.IsNullOrEmpty(s))
        {
            WriteEscapedJavaScriptNonNullString(writer, s, escapeFlags, escapeHandling, bufferPool, ref buffer);
        }

        // trailing delimiter
        if (appendDelimiters)
        {
            writer.Write(delimiter);
        }
    }

    static void WriteEscapedJavaScriptNonNullString(TextWriter writer, string s, bool[] escapeFlags, EscapeHandling escapeHandling, IArrayPool<char>? bufferPool, ref char[]? buffer)
    {
        var lastWritePosition = FirstCharToEscape(s, escapeFlags, escapeHandling);
        if (lastWritePosition == -1)
        {
            writer.Write(s);
            return;
        }

        if (lastWritePosition != 0)
        {
            if (buffer == null || buffer.Length < lastWritePosition)
            {
                buffer = BufferUtils.EnsureBufferSize(bufferPool, lastWritePosition, buffer);
            }

            // write unchanged chars at start of text.
            s.CopyTo(0, buffer, 0, lastWritePosition);
            writer.Write(buffer, 0, lastWritePosition);
        }

        int length;
        for (var i = lastWritePosition; i < s.Length; i++)
        {
            var c = s[i];

            if (c < escapeFlags.Length && !escapeFlags[c])
            {
                continue;
            }

            string? escapedValue;

            switch (c)
            {
                case '\t':
                    escapedValue = @"\t";
                    break;
                case '\n':
                    escapedValue = @"\n";
                    break;
                case '\r':
                    escapedValue = @"\r";
                    break;
                case '\f':
                    escapedValue = @"\f";
                    break;
                case '\b':
                    escapedValue = @"\b";
                    break;
                case '\\':
                    escapedValue = @"\\";
                    break;
                case '\u0085': // Next Line
                    escapedValue = @"\u0085";
                    break;
                case '\u2028': // Line Separator
                    escapedValue = @"\u2028";
                    break;
                case '\u2029': // Paragraph Separator
                    escapedValue = @"\u2029";
                    break;
                default:
                    if (c >= escapeFlags.Length && escapeHandling != EscapeHandling.EscapeNonAscii)
                    {
                        escapedValue = null;
                        break;
                    }

                    if (c == '\'' && escapeHandling != EscapeHandling.EscapeHtml)
                    {
                        escapedValue = @"\'";
                        break;
                    }

                    if (c == '"' && escapeHandling != EscapeHandling.EscapeHtml)
                    {
                        escapedValue = "\\\"";
                        break;
                    }

                    if (buffer == null || buffer.Length < unicodeTextLength)
                    {
                        buffer = BufferUtils.EnsureBufferSize(bufferPool, unicodeTextLength, buffer);
                    }

                    StringUtils.ToCharAsUnicode(c, buffer);

                    // slightly hacky but it saves multiple conditions in if test
                    escapedValue = escapedUnicodeText;

                    break;
            }

            if (escapedValue == null)
            {
                continue;
            }

            var isEscapedUnicodeText = string.Equals(escapedValue, escapedUnicodeText, StringComparison.Ordinal);

            if (i > lastWritePosition)
            {
                length = i - lastWritePosition + (isEscapedUnicodeText ? unicodeTextLength : 0);
                var start = isEscapedUnicodeText ? unicodeTextLength : 0;

                if (buffer == null || buffer.Length < length)
                {
                    var newBuffer = BufferUtils.RentBuffer(bufferPool, length);

                    // the unicode text is already in the buffer
                    // copy it over when creating new buffer
                    if (isEscapedUnicodeText)
                    {
                        MiscellaneousUtils.Assert(buffer != null, "Write buffer should never be null because it is set when the escaped unicode text is encountered.");

                        Array.Copy(buffer, newBuffer, unicodeTextLength);
                    }

                    BufferUtils.ReturnBuffer(bufferPool, buffer);

                    buffer = newBuffer;
                }

                s.CopyTo(lastWritePosition, buffer, start, length - start);

                // write unchanged chars before writing escaped text
                writer.Write(buffer, start, length - start);
            }

            lastWritePosition = i + 1;
            if (isEscapedUnicodeText)
            {
                writer.Write(buffer!, 0, unicodeTextLength);
            }
            else
            {
                writer.Write(escapedValue);
            }
        }

        MiscellaneousUtils.Assert(lastWritePosition != 0);
        length = s.Length - lastWritePosition;
        if (length > 0)
        {
            if (buffer == null || buffer.Length < length)
            {
                buffer = BufferUtils.EnsureBufferSize(bufferPool, length, buffer);
            }

            s.CopyTo(lastWritePosition, buffer, 0, length);

            // write remaining text
            writer.Write(buffer, 0, length);
        }
    }

    public static string ToEscapedJavaScriptString(string? value, char delimiter, bool appendDelimiters, EscapeHandling escapeHandling)
    {
        var escapeFlags = GetCharEscapeFlags(escapeHandling, delimiter);

        using var w = StringUtils.CreateStringWriter(value?.Length ?? 16);
        char[]? buffer = null;
        WriteEscapedJavaScriptString(w, value, delimiter, appendDelimiters, escapeFlags, escapeHandling, null, ref buffer);
        return w.ToString();
    }

    static int FirstCharToEscape(string s, bool[] escapeFlags, EscapeHandling escapeHandling)
    {
        for (var i = 0; i != s.Length; i++)
        {
            var c = s[i];

            if (c < escapeFlags.Length)
            {
                if (escapeFlags[c])
                {
                    return i;
                }
            }
            else if (escapeHandling == EscapeHandling.EscapeNonAscii)
            {
                return i;
            }
            else
            {
                switch (c)
                {
                    case '\u0085':
                    case '\u2028':
                    case '\u2029':
                        return i;
                }
            }
        }

        return -1;
    }

    public static Task WriteEscapedJavaScriptStringAsync(TextWriter writer, string s, char delimiter, bool appendDelimiters, bool[] escapeFlags, EscapeHandling escapeHandling, JsonTextWriter client, char[] buffer, CancellationToken cancellation = default)
    {
        if (cancellation.IsCancellationRequested)
        {
            return cancellation.FromCanceled();
        }

        if (appendDelimiters)
        {
            return WriteEscapedJavaScriptStringWithDelimitersAsync(writer, s, delimiter, escapeFlags, escapeHandling, client, buffer, cancellation);
        }

        if (StringUtils.IsNullOrEmpty(s))
        {
            return cancellation.CancelIfRequestedAsync() ?? AsyncUtils.CompletedTask;
        }

        return WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, escapeFlags, escapeHandling, client, buffer, cancellation);
    }

    static Task WriteEscapedJavaScriptStringWithDelimitersAsync(TextWriter writer, string s, char delimiter, bool[] escapeFlags, EscapeHandling escapeHandling, JsonTextWriter client, char[] buffer, CancellationToken cancellation)
    {
        var task = writer.WriteAsync(delimiter, cancellation);
        if (!task.IsCompletedSucessfully())
        {
            return WriteEscapedJavaScriptStringWithDelimitersAsync(task, writer, s, delimiter, escapeFlags, escapeHandling, client, buffer, cancellation);
        }

        if (!StringUtils.IsNullOrEmpty(s))
        {
            task = WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, escapeFlags, escapeHandling, client, buffer, cancellation);
            if (task.IsCompletedSucessfully())
            {
                return writer.WriteAsync(delimiter, cancellation);
            }
        }

        return WriteCharAsync(task, writer, delimiter, cancellation);
    }

    static async Task WriteEscapedJavaScriptStringWithDelimitersAsync(Task task, TextWriter writer, string s, char delimiter, bool[] escapeFlags, EscapeHandling escapeHandling, JsonTextWriter client, char[] buffer, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);

        if (!StringUtils.IsNullOrEmpty(s))
        {
            await WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, escapeFlags, escapeHandling, client, buffer, cancellation).ConfigureAwait(false);
        }

        await writer.WriteAsync(delimiter).ConfigureAwait(false);
    }

    public static async Task WriteCharAsync(Task task, TextWriter writer, char c, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);
        await writer.WriteAsync(c, cancellation).ConfigureAwait(false);
    }

    static Task WriteEscapedJavaScriptStringWithoutDelimitersAsync(TextWriter writer, string s, bool[] escapeFlags, EscapeHandling escapeHandling, JsonTextWriter client, char[] buffer, CancellationToken cancellation)
    {
        var i = FirstCharToEscape(s, escapeFlags, escapeHandling);
        if (i == -1)
        {
            return writer.WriteAsync(s, cancellation);
        }

        return WriteDefinitelyEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, i, escapeFlags, escapeHandling, client, buffer, cancellation);
    }

    static async Task WriteDefinitelyEscapedJavaScriptStringWithoutDelimitersAsync(TextWriter writer, string s, int lastWritePosition, bool[] escapeFlags, EscapeHandling escapeHandling, JsonTextWriter client, char[] buffer, CancellationToken cancellation)
    {
        if (buffer == null ||
            buffer.Length < lastWritePosition)
        {
            buffer = client.EnsureBuffer(lastWritePosition, unicodeTextLength);
        }

        if (lastWritePosition != 0)
        {
            s.CopyTo(0, buffer, 0, lastWritePosition);

            // write unchanged chars at start of text.
            await writer.WriteAsync(buffer, 0, lastWritePosition, cancellation).ConfigureAwait(false);
        }

        int length;
        var isEscapedUnicodeText = false;
        string? escapedValue = null;

        for (var i = lastWritePosition; i < s.Length; i++)
        {
            var c = s[i];

            if (c < escapeFlags.Length && !escapeFlags[c])
            {
                continue;
            }

            switch (c)
            {
                case '\t':
                    escapedValue = @"\t";
                    break;
                case '\n':
                    escapedValue = @"\n";
                    break;
                case '\r':
                    escapedValue = @"\r";
                    break;
                case '\f':
                    escapedValue = @"\f";
                    break;
                case '\b':
                    escapedValue = @"\b";
                    break;
                case '\\':
                    escapedValue = @"\\";
                    break;
                case '\u0085': // Next Line
                    escapedValue = @"\u0085";
                    break;
                case '\u2028': // Line Separator
                    escapedValue = @"\u2028";
                    break;
                case '\u2029': // Paragraph Separator
                    escapedValue = @"\u2029";
                    break;
                default:
                    if (c >= escapeFlags.Length &&
                        escapeHandling != EscapeHandling.EscapeNonAscii)
                    {
                        continue;
                    }

                    if (c == '\'' &&
                        escapeHandling != EscapeHandling.EscapeHtml)
                    {
                        escapedValue = @"\'";
                    }
                    else if (c == '"' &&
                             escapeHandling != EscapeHandling.EscapeHtml)
                    {
                        escapedValue = @"\""";
                    }
                    else
                    {
                        if (buffer.Length < unicodeTextLength)
                        {
                            buffer = client.EnsureBuffer(unicodeTextLength, 0);
                        }

                        StringUtils.ToCharAsUnicode(c, buffer);

                        isEscapedUnicodeText = true;
                    }

                    break;
            }

            if (i > lastWritePosition)
            {
                length = i - lastWritePosition + (isEscapedUnicodeText ? unicodeTextLength : 0);
                var start = isEscapedUnicodeText ? unicodeTextLength : 0;

                if (buffer.Length < length)
                {
                    buffer = client.EnsureBuffer(length, unicodeTextLength);
                }

                s.CopyTo(lastWritePosition, buffer, start, length - start);

                // write unchanged chars before writing escaped text
                await writer.WriteAsync(buffer, start, length - start, cancellation).ConfigureAwait(false);
            }

            lastWritePosition = i + 1;
            if (isEscapedUnicodeText)
            {
                await writer.WriteAsync(buffer, 0, unicodeTextLength, cancellation).ConfigureAwait(false);
                isEscapedUnicodeText = false;
            }
            else
            {
                await writer.WriteAsync(escapedValue!, cancellation).ConfigureAwait(false);
            }
        }

        length = s.Length - lastWritePosition;

        if (length == 0)
        {
            return;
        }

        if (buffer.Length < length)
        {
            buffer = client.EnsureBuffer(length, 0);
        }

        s.CopyTo(lastWritePosition, buffer, 0, length);

        // write remaining text
        await writer.WriteAsync(buffer, 0, length, cancellation).ConfigureAwait(false);
    }
}