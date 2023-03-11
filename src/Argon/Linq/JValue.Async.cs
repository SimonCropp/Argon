// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public partial class JValue
{
    /// <summary>
    /// Writes this token to a <see cref="JsonWriter" /> asynchronously.
    /// </summary>
    public override Task WriteToAsync(JsonWriter writer, Cancellation cancellation, params JsonConverter[] converters)
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
                return writer.WriteCommentAsync(value?.ToString(), cancellation);
            case JTokenType.Raw:
                return writer.WriteRawValueAsync(value?.ToString(), cancellation);
            case JTokenType.Null:
                return writer.WriteNullAsync(cancellation);
            case JTokenType.Undefined:
                return writer.WriteUndefinedAsync(cancellation);
            case JTokenType.Integer:
                if (value is int i)
                {
                    return writer.WriteValueAsync(i, cancellation);
                }

                if (value is long l)
                {
                    return writer.WriteValueAsync(l, cancellation);
                }

                if (value is ulong ul)
                {
                    return writer.WriteValueAsync(ul, cancellation);
                }

                if (value is BigInteger integer)
                {
                    return writer.WriteValueAsync(integer, cancellation);
                }

                return writer.WriteValueAsync(Convert.ToInt64(value, InvariantCulture), cancellation);
            case JTokenType.Float:
                if (value is decimal dec)
                {
                    return writer.WriteValueAsync(dec, cancellation);
                }

                if (value is double d)
                {
                    return writer.WriteValueAsync(d, cancellation);
                }

                if (value is float f)
                {
                    return writer.WriteValueAsync(f, cancellation);
                }

                return writer.WriteValueAsync(Convert.ToDouble(value, InvariantCulture), cancellation);
            case JTokenType.String:
                return writer.WriteValueAsync(value?.ToString(), cancellation);
            case JTokenType.Boolean:
                return writer.WriteValueAsync(Convert.ToBoolean(value, InvariantCulture), cancellation);
            case JTokenType.Date:
                if (value is DateTimeOffset offset)
                {
                    return writer.WriteValueAsync(offset, cancellation);
                }

                return writer.WriteValueAsync(Convert.ToDateTime(value, InvariantCulture), cancellation);
            case JTokenType.Bytes:
                return writer.WriteValueAsync((byte[]?) value, cancellation);
            case JTokenType.Guid:
                return writer.WriteValueAsync((Guid?) value, cancellation);
            case JTokenType.TimeSpan:
                return writer.WriteValueAsync((TimeSpan?) value, cancellation);
            case JTokenType.Uri:
                return writer.WriteValueAsync((Uri?) value, cancellation);
        }

        throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(Type), valueType, "Unexpected token type.");
    }
}