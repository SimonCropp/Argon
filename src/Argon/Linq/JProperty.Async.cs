﻿#region License
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

public partial class JProperty
{
    /// <summary>
    /// Writes this token to a <see cref="JsonWriter"/> asynchronously.
    /// </summary>
    public override Task WriteToAsync(JsonWriter writer, CancellationToken cancellation, params JsonConverter[] converters)
    {
        var task = writer.WritePropertyNameAsync(Name, cancellation);
        if (task.IsCompletedSucessfully())
        {
            return WriteValueAsync(writer, cancellation, converters);
        }

        return WriteToAsync(task, writer, cancellation, converters);
    }

    async Task WriteToAsync(Task task, JsonWriter writer, CancellationToken cancellation, params JsonConverter[] converters)
    {
        await task.ConfigureAwait(false);

        await WriteValueAsync(writer, cancellation, converters).ConfigureAwait(false);
    }

    Task WriteValueAsync(JsonWriter writer, CancellationToken cancellation, JsonConverter[] converters)
    {
        var value = Value;
        if (value == null)
        {
            return writer.WriteNullAsync(cancellation);
        }

        return value.WriteToAsync(writer, cancellation, converters);

    }

    /// <summary>
    /// Asynchronously loads a <see cref="JProperty"/> from a <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JProperty"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous creation. The <see cref="Task{TResult}.Result"/>
    /// property returns a <see cref="JProperty"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
    public new static Task<JProperty> LoadAsync(JsonReader reader, CancellationToken cancellation = default)
    {
        return LoadAsync(reader, null, cancellation);
    }

    /// <summary>
    /// Asynchronously loads a <see cref="JProperty"/> from a <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JProperty"/>.</param>
    /// <param name="settings">The <see cref="JsonLoadSettings"/> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous creation. The <see cref="Task{TResult}.Result"/>
    /// property returns a <see cref="JProperty"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
    public new static async Task<JProperty> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellation = default)
    {
        if (reader.TokenType == JsonToken.None)
        {
            if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
            {
                throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader.");
            }
        }

        await reader.MoveToContentAsync(cancellation).ConfigureAwait(false);

        if (reader.TokenType != JsonToken.PropertyName)
        {
            throw JsonReaderException.Create(reader, $"Error reading JProperty from JsonReader. Current JsonReader item is not a property: {reader.TokenType}");
        }

        var p = new JProperty((string)reader.Value!);
        p.SetLineInfo(reader as IJsonLineInfo, settings);

        await p.ReadTokenFromAsync(reader, settings, cancellation).ConfigureAwait(false);

        return p;
    }
}