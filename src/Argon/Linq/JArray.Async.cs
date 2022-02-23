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

public partial class JArray
{
    /// <summary>
    /// Writes this token to a <see cref="JsonWriter"/> asynchronously.
    /// </summary>
    public override async Task WriteToAsync(JsonWriter writer, CancellationToken cancellation, params JsonConverter[] converters)
    {
        await writer.WriteStartArrayAsync(cancellation).ConfigureAwait(false);

        for (var i = 0; i < values.Count; i++)
        {
            await values[i].WriteToAsync(writer, cancellation, converters).ConfigureAwait(false);
        }

        await writer.WriteEndArrayAsync(cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads a <see cref="JArray"/> from a <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JArray"/>.
    /// If this is <c>null</c>, default load settings will be used.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous load. The <see cref="Task{TResult}.Result"/> property contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
    public new static Task<JArray> LoadAsync(JsonReader reader, CancellationToken cancellation = default)
    {
        return LoadAsync(reader, null, cancellation);
    }

    /// <summary>
    /// Asynchronously loads a <see cref="JArray"/> from a <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JArray"/>.</param>
    /// <param name="settings">The <see cref="JsonLoadSettings"/> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous load. The <see cref="Task{TResult}.Result"/> property contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
    public new static async Task<JArray> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellation = default)
    {
        if (reader.TokenType == JsonToken.None)
        {
            if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
            {
                throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader.");
            }
        }

        await reader.MoveToContentAsync(cancellation).ConfigureAwait(false);

        if (reader.TokenType != JsonToken.StartArray)
        {
            throw JsonReaderException.Create(reader, $"Error reading JArray from JsonReader. Current JsonReader item is not an array: {reader.TokenType}");
        }

        var a = new JArray();
        a.SetLineInfo(reader as IJsonLineInfo, settings);

        await a.ReadTokenFromAsync(reader, settings, cancellation).ConfigureAwait(false);

        return a;
    }
}