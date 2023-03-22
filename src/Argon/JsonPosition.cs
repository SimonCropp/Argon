// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

enum JsonContainerType
{
    None = 0,
    Object = 1,
    Array = 2
}

struct JsonPosition
{
    static readonly char[] specialCharacters = {'.', ' ', '\'', '/', '"', '[', ']', '(', ')', '\t', '\n', '\r', '\f', '\b', '\\', '\u0085', '\u2028', '\u2029'};

    internal JsonContainerType Type;
    internal int Position;
    internal string? PropertyName;
    internal bool HasIndex;

    public JsonPosition(JsonContainerType type)
    {
        Type = type;
        HasIndex = TypeHasIndex(type);
        Position = -1;
        PropertyName = null;
    }

    int CalculateLength() =>
        Type switch
        {
            JsonContainerType.Object => PropertyName!.Length + 5,
            JsonContainerType.Array => MathUtils.IntLength((ulong) Position) + 2,
            _ => throw new ArgumentOutOfRangeException(nameof(Type))
        };

    void WriteTo(StringBuilder sb, ref StringWriter? writer, ref char[]? buffer)
    {
        switch (Type)
        {
            case JsonContainerType.Object:
                var propertyName = PropertyName!;
                if (propertyName.IndexOfAny(specialCharacters) != -1)
                {
                    sb.Append("['");

                    writer ??= new(sb);

                    JavaScriptUtils.WriteEscapedJavaScriptString(writer, propertyName, '\'', false, JavaScriptUtils.SingleQuoteEscapeFlags, EscapeHandling.Default, ref buffer);

                    sb.Append("']");
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        sb.Append('.');
                    }

                    sb.Append(propertyName);
                }

                break;
            case JsonContainerType.Array:
                sb.Append('[');
                sb.Append(Position);
                sb.Append(']');
                break;
        }
    }

    static bool TypeHasIndex(JsonContainerType type) =>
        type is JsonContainerType.Array;

    internal static string BuildPath(List<JsonPosition> positions, JsonPosition? currentPosition)
    {
        var capacity = 0;
        for (var i = 0; i < positions.Count; i++)
        {
            capacity += positions[i].CalculateLength();
        }

        if (currentPosition != null)
        {
            capacity += currentPosition.GetValueOrDefault().CalculateLength();
        }

        var stringBuilder = new StringBuilder(capacity);
        StringWriter? writer = null;
        char[]? buffer = null;
        foreach (var state in positions)
        {
            state.WriteTo(stringBuilder, ref writer, ref buffer);
        }

        currentPosition?.WriteTo(stringBuilder, ref writer, ref buffer);

        return stringBuilder.ToString();
    }

    internal static string FormatMessage(IJsonLineInfo? lineInfo, string path, string message)
    {
        // don't add a fullstop and space when message ends with a new line
        if (!message.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            message = message.Trim();

            if (!message.EndsWith('.'))
            {
                message += ".";
            }

            message += " ";
        }

        message += $"Path '{path}'";

        if (lineInfo != null && lineInfo.HasLineInfo())
        {
            message += $", line {lineInfo.LineNumber}, position {lineInfo.LinePosition}";
        }

        message += ".";

        return message;
    }
}