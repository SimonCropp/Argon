// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public partial class JValue
{
    /// <summary>
    /// Writes this token to a <see cref="JsonWriter" /> asynchronously.
    /// </summary>
    public override Task WriteToAsync(JsonWriter writer, Cancel cancel, params JsonConverter[] converters)
    {
        if (converters is {Length: > 0} && value != null)
        {
            var matchingConverter = JsonSerializer.GetMatchingConverter(converters, value.GetType());
            if (matchingConverter is {CanWrite: true})
            {
                // TODO: Call WriteJsonAsync when it exists.
                matchingConverter.WriteJson(writer, value, JsonSerializer.CreateDefault());
                return Task.CompletedTask;
            }
        }

        switch (valueType)
        {
            case JTokenType.Comment:
                return writer.WriteCommentAsync(value?.ToString(), cancel);
            case JTokenType.Raw:
                return writer.WriteRawValueAsync(value?.ToString(), cancel);
            case JTokenType.Null:
                return writer.WriteNullAsync(cancel);
            case JTokenType.Undefined:
                return writer.WriteUndefinedAsync(cancel);
            case JTokenType.Integer:
                if (value is int i)
                {
                    return writer.WriteValueAsync(i, cancel);
                }

                if (value is long l)
                {
                    return writer.WriteValueAsync(l, cancel);
                }

                if (value is ulong ul)
                {
                    return writer.WriteValueAsync(ul, cancel);
                }

                if (value is BigInteger integer)
                {
                    return writer.WriteValueAsync(integer, cancel);
                }

                return writer.WriteValueAsync(Convert.ToInt64(value, InvariantCulture), cancel);
            case JTokenType.Float:
                if (value is decimal dec)
                {
                    return writer.WriteValueAsync(dec, cancel);
                }

                if (value is double d)
                {
                    return writer.WriteValueAsync(d, cancel);
                }

                if (value is float f)
                {
                    return writer.WriteValueAsync(f, cancel);
                }

                return writer.WriteValueAsync(Convert.ToDouble(value, InvariantCulture), cancel);
            case JTokenType.String:
                return writer.WriteValueAsync(value?.ToString(), cancel);
            case JTokenType.Boolean:
                return writer.WriteValueAsync(Convert.ToBoolean(value, InvariantCulture), cancel);
            case JTokenType.Date:
                if (value is DateTimeOffset offset)
                {
                    return writer.WriteValueAsync(offset, cancel);
                }

                return writer.WriteValueAsync(Convert.ToDateTime(value, InvariantCulture), cancel);
            case JTokenType.Bytes:
                return writer.WriteValueAsync((byte[]?) value, cancel);
            case JTokenType.Guid:
                return writer.WriteValueAsync((Guid?) value, cancel);
            case JTokenType.TimeSpan:
                return writer.WriteValueAsync((TimeSpan?) value, cancel);
            case JTokenType.Uri:
                return writer.WriteValueAsync((Uri?) value, cancel);
        }

        throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(Type), valueType, "Unexpected token type.");
    }
}