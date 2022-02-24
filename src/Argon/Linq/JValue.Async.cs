#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

namespace Argon.Linq;

public partial class JValue
{
    /// <summary>
    /// Writes this token to a <see cref="JsonWriter"/> asynchronously.
    /// </summary>
    public override Task WriteToAsync(JsonWriter writer, CancellationToken cancellation, params JsonConverter[] converters)
    {
        if (converters is {Length: > 0} && value != null)
        {
            var matchingConverter = JsonSerializer.GetMatchingConverter(converters, value.GetType());
            if (matchingConverter is {CanWrite: true})
            {
                // TODO: Call WriteJsonAsync when it exists.
                matchingConverter.WriteJson(writer, value, JsonSerializer.CreateDefault());
                return AsyncUtils.CompletedTask;
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

                return writer.WriteValueAsync(Convert.ToInt64(value, CultureInfo.InvariantCulture), cancellation);
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

                return writer.WriteValueAsync(Convert.ToDouble(value, CultureInfo.InvariantCulture), cancellation);
            case JTokenType.String:
                return writer.WriteValueAsync(value?.ToString(), cancellation);
            case JTokenType.Boolean:
                return writer.WriteValueAsync(Convert.ToBoolean(value, CultureInfo.InvariantCulture), cancellation);
            case JTokenType.Date:
                if (value is DateTimeOffset offset)
                {
                    return writer.WriteValueAsync(offset, cancellation);
                }

                return writer.WriteValueAsync(Convert.ToDateTime(value, CultureInfo.InvariantCulture), cancellation);
            case JTokenType.Bytes:
                return writer.WriteValueAsync((byte[]?)value, cancellation);
            case JTokenType.Guid:
                return writer.WriteValueAsync(value != null ? (Guid?)value : null, cancellation);
            case JTokenType.TimeSpan:
                return writer.WriteValueAsync(value != null ? (TimeSpan?)value : null, cancellation);
            case JTokenType.Uri:
                return writer.WriteValueAsync((Uri?)value, cancellation);
        }

        throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(Type), valueType, "Unexpected token type.");
    }
}