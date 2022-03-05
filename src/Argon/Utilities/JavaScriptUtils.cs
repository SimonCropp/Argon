// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class JavaScriptUtils
{
    internal static readonly bool[] SingleQuoteCharEscapeFlags = new bool[128];
    internal static readonly bool[] DoubleQuoteCharEscapeFlags = new bool[128];
    internal static readonly bool[] HtmlCharEscapeFlags = new bool[128];

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
            SingleQuoteCharEscapeFlags[escapeChar] = true;
        }

        foreach (var escapeChar in escapeChars.Union(new[] {'"'}))
        {
            DoubleQuoteCharEscapeFlags[escapeChar] = true;
        }

        foreach (var escapeChar in escapeChars.Union(new[] {'"', '\'', '<', '>', '&'}))
        {
            HtmlCharEscapeFlags[escapeChar] = true;
        }
    }

    const string escapedUnicodeText = "!";

    public static bool[] GetCharEscapeFlags(EscapeHandling escapeHandling, char quoteChar)
    {
        if (escapeHandling == EscapeHandling.EscapeHtml)
        {
            return HtmlCharEscapeFlags;
        }

        if (quoteChar == '"')
        {
            return DoubleQuoteCharEscapeFlags;
        }

        return SingleQuoteCharEscapeFlags;
    }

    public static bool ShouldEscapeJavaScriptString(string? s, bool[] charEscapeFlags)
    {
        if (s == null)
        {
            return false;
        }

        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c >= charEscapeFlags.Length || charEscapeFlags[c])
            {
                return true;
            }
        }

        return false;
    }

    public static void WriteEscapedJavaScriptString(TextWriter writer, string? s, char delimiter, bool appendDelimiters,
        bool[] charEscapeFlags, EscapeHandling escapeHandling, IArrayPool<char>? bufferPool, ref char[]? writeBuffer)
    {
        // leading delimiter
        if (appendDelimiters)
        {
            writer.Write(delimiter);
        }

        if (!StringUtils.IsNullOrEmpty(s))
        {
            var lastWritePosition = FirstCharToEscape(s, charEscapeFlags, escapeHandling);
            if (lastWritePosition == -1)
            {
                writer.Write(s);
            }
            else
            {
                if (lastWritePosition != 0)
                {
                    if (writeBuffer == null || writeBuffer.Length < lastWritePosition)
                    {
                        writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, lastWritePosition, writeBuffer);
                    }

                    // write unchanged chars at start of text.
                    s.CopyTo(0, writeBuffer, 0, lastWritePosition);
                    writer.Write(writeBuffer, 0, lastWritePosition);
                }

                int length;
                for (var i = lastWritePosition; i < s.Length; i++)
                {
                    var c = s[i];

                    if (c < charEscapeFlags.Length && !charEscapeFlags[c])
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
                            if (c < charEscapeFlags.Length || escapeHandling == EscapeHandling.EscapeNonAscii)
                            {
                                if (c == '\'' && escapeHandling != EscapeHandling.EscapeHtml)
                                {
                                    escapedValue = @"\'";
                                }
                                else if (c == '"' && escapeHandling != EscapeHandling.EscapeHtml)
                                {
                                    escapedValue = "\\\"";
                                }
                                else
                                {
                                    if (writeBuffer == null || writeBuffer.Length < unicodeTextLength)
                                    {
                                        writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, unicodeTextLength, writeBuffer);
                                    }

                                    StringUtils.ToCharAsUnicode(c, writeBuffer);

                                    // slightly hacky but it saves multiple conditions in if test
                                    escapedValue = escapedUnicodeText;
                                }
                            }
                            else
                            {
                                escapedValue = null;
                            }

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

                        if (writeBuffer == null || writeBuffer.Length < length)
                        {
                            var newBuffer = BufferUtils.RentBuffer(bufferPool, length);

                            // the unicode text is already in the buffer
                            // copy it over when creating new buffer
                            if (isEscapedUnicodeText)
                            {
                                MiscellaneousUtils.Assert(writeBuffer != null, "Write buffer should never be null because it is set when the escaped unicode text is encountered.");

                                Array.Copy(writeBuffer, newBuffer, unicodeTextLength);
                            }

                            BufferUtils.ReturnBuffer(bufferPool, writeBuffer);

                            writeBuffer = newBuffer;
                        }

                        s.CopyTo(lastWritePosition, writeBuffer, start, length - start);

                        // write unchanged chars before writing escaped text
                        writer.Write(writeBuffer, start, length - start);
                    }

                    lastWritePosition = i + 1;
                    if (isEscapedUnicodeText)
                    {
                        writer.Write(writeBuffer!, 0, unicodeTextLength);
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
                    if (writeBuffer == null || writeBuffer.Length < length)
                    {
                        writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, length, writeBuffer);
                    }

                    s.CopyTo(lastWritePosition, writeBuffer, 0, length);

                    // write remaining text
                    writer.Write(writeBuffer, 0, length);
                }
            }
        }

        // trailing delimiter
        if (appendDelimiters)
        {
            writer.Write(delimiter);
        }
    }

    public static string ToEscapedJavaScriptString(string? value, char delimiter, bool appendDelimiters, EscapeHandling escapeHandling)
    {
        var charEscapeFlags = GetCharEscapeFlags(escapeHandling, delimiter);

        using var w = StringUtils.CreateStringWriter(value?.Length ?? 16);
        char[]? buffer = null;
        WriteEscapedJavaScriptString(w, value, delimiter, appendDelimiters, charEscapeFlags, escapeHandling, null, ref buffer);
        return w.ToString();
    }

    static int FirstCharToEscape(string s, bool[] charEscapeFlags, EscapeHandling escapeHandling)
    {
        for (var i = 0; i != s.Length; i++)
        {
            var c = s[i];

            if (c < charEscapeFlags.Length)
            {
                if (charEscapeFlags[c])
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

    public static Task WriteEscapedJavaScriptStringAsync(TextWriter writer, string s, char delimiter, bool appendDelimiters, bool[] charEscapeFlags, EscapeHandling escapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellation = default)
    {
        if (cancellation.IsCancellationRequested)
        {
            return cancellation.FromCanceled();
        }

        if (appendDelimiters)
        {
            return WriteEscapedJavaScriptStringWithDelimitersAsync(writer, s, delimiter, charEscapeFlags, escapeHandling, client, writeBuffer, cancellation);
        }

        if (StringUtils.IsNullOrEmpty(s))
        {
            return cancellation.CancelIfRequestedAsync() ?? AsyncUtils.CompletedTask;
        }

        return WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, charEscapeFlags, escapeHandling, client, writeBuffer, cancellation);
    }

    static Task WriteEscapedJavaScriptStringWithDelimitersAsync(TextWriter writer, string s, char delimiter,
        bool[] charEscapeFlags, EscapeHandling escapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellation)
    {
        var task = writer.WriteAsync(delimiter, cancellation);
        if (!task.IsCompletedSucessfully())
        {
            return WriteEscapedJavaScriptStringWithDelimitersAsync(task, writer, s, delimiter, charEscapeFlags, escapeHandling, client, writeBuffer, cancellation);
        }

        if (!StringUtils.IsNullOrEmpty(s))
        {
            task = WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, charEscapeFlags, escapeHandling, client, writeBuffer, cancellation);
            if (task.IsCompletedSucessfully())
            {
                return writer.WriteAsync(delimiter, cancellation);
            }
        }

        return WriteCharAsync(task, writer, delimiter, cancellation);
    }

    static async Task WriteEscapedJavaScriptStringWithDelimitersAsync(Task task, TextWriter writer, string s, char delimiter,
        bool[] charEscapeFlags, EscapeHandling escapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);

        if (!StringUtils.IsNullOrEmpty(s))
        {
            await WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, charEscapeFlags, escapeHandling, client, writeBuffer, cancellation).ConfigureAwait(false);
        }

        await writer.WriteAsync(delimiter).ConfigureAwait(false);
    }

    public static async Task WriteCharAsync(Task task, TextWriter writer, char c, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);
        await writer.WriteAsync(c, cancellation).ConfigureAwait(false);
    }

    static Task WriteEscapedJavaScriptStringWithoutDelimitersAsync(
        TextWriter writer, string s, bool[] charEscapeFlags, EscapeHandling escapeHandling,
        JsonTextWriter client, char[] writeBuffer, CancellationToken cancellation)
    {
        var i = FirstCharToEscape(s, charEscapeFlags, escapeHandling);
        return i == -1
            ? writer.WriteAsync(s, cancellation)
            : WriteDefinitelyEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, i, charEscapeFlags, escapeHandling, client, writeBuffer, cancellation);
    }

    static async Task WriteDefinitelyEscapedJavaScriptStringWithoutDelimitersAsync(
        TextWriter writer, string s, int lastWritePosition, bool[] charEscapeFlags,
        EscapeHandling escapeHandling, JsonTextWriter client, char[] writeBuffer,
        CancellationToken cancellation)
    {
        if (writeBuffer == null || writeBuffer.Length < lastWritePosition)
        {
            writeBuffer = client.EnsureWriteBuffer(lastWritePosition, unicodeTextLength);
        }

        if (lastWritePosition != 0)
        {
            s.CopyTo(0, writeBuffer, 0, lastWritePosition);

            // write unchanged chars at start of text.
            await writer.WriteAsync(writeBuffer, 0, lastWritePosition, cancellation).ConfigureAwait(false);
        }

        int length;
        var isEscapedUnicodeText = false;
        string? escapedValue = null;

        for (var i = lastWritePosition; i < s.Length; i++)
        {
            var c = s[i];

            if (c < charEscapeFlags.Length && !charEscapeFlags[c])
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
                    if (c < charEscapeFlags.Length || escapeHandling == EscapeHandling.EscapeNonAscii)
                    {
                        if (c == '\'' && escapeHandling != EscapeHandling.EscapeHtml)
                        {
                            escapedValue = @"\'";
                        }
                        else if (c == '"' && escapeHandling != EscapeHandling.EscapeHtml)
                        {
                            escapedValue = @"\""";
                        }
                        else
                        {
                            if (writeBuffer.Length < unicodeTextLength)
                            {
                                writeBuffer = client.EnsureWriteBuffer(unicodeTextLength, 0);
                            }

                            StringUtils.ToCharAsUnicode(c, writeBuffer);

                            isEscapedUnicodeText = true;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    break;
            }

            if (i > lastWritePosition)
            {
                length = i - lastWritePosition + (isEscapedUnicodeText ? unicodeTextLength : 0);
                var start = isEscapedUnicodeText ? unicodeTextLength : 0;

                if (writeBuffer.Length < length)
                {
                    writeBuffer = client.EnsureWriteBuffer(length, unicodeTextLength);
                }

                s.CopyTo(lastWritePosition, writeBuffer, start, length - start);

                // write unchanged chars before writing escaped text
                await writer.WriteAsync(writeBuffer, start, length - start, cancellation).ConfigureAwait(false);
            }

            lastWritePosition = i + 1;
            if (isEscapedUnicodeText)
            {
                await writer.WriteAsync(writeBuffer, 0, unicodeTextLength, cancellation).ConfigureAwait(false);
                isEscapedUnicodeText = false;
            }
            else
            {
                await writer.WriteAsync(escapedValue!, cancellation).ConfigureAwait(false);
            }
        }

        length = s.Length - lastWritePosition;

        if (length != 0)
        {
            if (writeBuffer.Length < length)
            {
                writeBuffer = client.EnsureWriteBuffer(length, 0);
            }

            s.CopyTo(lastWritePosition, writeBuffer, 0, length);

            // write remaining text
            await writer.WriteAsync(writeBuffer, 0, length, cancellation).ConfigureAwait(false);
        }
    }
}