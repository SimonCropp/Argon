// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public partial class JObject
{
    /// <summary>
    /// Writes this token to a <see cref="JsonWriter" /> asynchronously.
    /// </summary>
    public override Task WriteToAsync(JsonWriter writer, CancellationToken cancellation, params JsonConverter[] converters)
    {
        var t = writer.WriteStartObjectAsync(cancellation);
        if (!t.IsCompletedSucessfully())
        {
            return AwaitProperties(t, 0, writer, cancellation, converters);
        }

        for (var i = 0; i < properties.Count; i++)
        {
            t = properties[i].WriteToAsync(writer, cancellation, converters);
            if (!t.IsCompletedSucessfully())
            {
                return AwaitProperties(t, i + 1, writer, cancellation, converters);
            }
        }

        return writer.WriteEndObjectAsync(cancellation);

        // Local functions, params renamed (capitalized) so as not to capture and allocate when calling async
        async Task AwaitProperties(Task task, int i, JsonWriter Writer, CancellationToken CancellationToken, JsonConverter[] Converters)
        {
            await task.ConfigureAwait(false);
            for (; i < properties.Count; i++)
            {
                await properties[i].WriteToAsync(Writer, CancellationToken, Converters).ConfigureAwait(false);
            }

            await Writer.WriteEndObjectAsync(CancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously loads a <see cref="JObject" /> from a <see cref="JsonReader" />.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader" /> that will be read for the content of the <see cref="JObject" />.</param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous load. The <see cref="Task{TResult}.Result" />
    /// property returns a <see cref="JObject" /> that contains the JSON that was read from the specified <see cref="JsonReader" />.
    /// </returns>
    public new static Task<JObject> LoadAsync(JsonReader reader, CancellationToken cancellation = default) =>
        LoadAsync(reader, null, cancellation);

    /// <summary>
    /// Asynchronously loads a <see cref="JObject" /> from a <see cref="JsonReader" />.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader" /> that will be read for the content of the <see cref="JObject" />.</param>
    /// <param name="settings">
    /// The <see cref="JsonLoadSettings" /> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous load. The <see cref="Task{TResult}.Result" />
    /// property returns a <see cref="JObject" /> that contains the JSON that was read from the specified <see cref="JsonReader" />.
    /// </returns>
    public new static async Task<JObject> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellation = default)
    {
        if (reader.TokenType == JsonToken.None)
        {
            if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
            {
                throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader.");
            }
        }

        await reader.MoveToContentAsync(cancellation).ConfigureAwait(false);

        if (reader.TokenType != JsonToken.StartObject)
        {
            throw JsonReaderException.Create(reader, $"Error reading JObject from JsonReader. Current JsonReader item is not an object: {reader.TokenType}");
        }

        var o = new JObject();
        o.SetLineInfo(reader as IJsonLineInfo, settings);

        await o.ReadTokenFromAsync(reader, settings, cancellation).ConfigureAwait(false);

        return o;
    }
}