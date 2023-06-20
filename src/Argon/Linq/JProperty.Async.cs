// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public partial class JProperty
{
    /// <summary>
    /// Writes this token to a <see cref="JsonWriter" /> asynchronously.
    /// </summary>
    public override Task WriteToAsync(JsonWriter writer, Cancel cancel, params JsonConverter[] converters)
    {
        var task = writer.WritePropertyNameAsync(Name, cancel);
        if (task.IsCompletedSuccessfully())
        {
            return WriteValueAsync(writer, cancel, converters);
        }

        return WriteToAsync(task, writer, cancel, converters);
    }

    async Task WriteToAsync(Task task, JsonWriter writer, Cancel cancel, params JsonConverter[] converters)
    {
        await task.ConfigureAwait(false);

        await WriteValueAsync(writer, cancel, converters).ConfigureAwait(false);
    }

    Task WriteValueAsync(JsonWriter writer, Cancel cancel, JsonConverter[] converters)
    {
        var value = content.token;
        if (value == null)
        {
            return writer.WriteNullAsync(cancel);
        }

        return value.WriteToAsync(writer, cancel, converters);
    }

    /// <summary>
    /// Asynchronously loads a <see cref="JProperty" /> from a <see cref="JsonReader" />.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader" /> that will be read for the content of the <see cref="JProperty" />.</param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> representing the asynchronous creation. The <see cref="Task{TResult}.Result" />
    /// property returns a <see cref="JProperty" /> that contains the JSON that was read from the specified <see cref="JsonReader" />.
    /// </returns>
    public new static Task<JProperty> LoadAsync(JsonReader reader, Cancel cancel = default) =>
        LoadAsync(reader, null, cancel);

    /// <summary>
    /// Asynchronously loads a <see cref="JProperty" /> from a <see cref="JsonReader" />.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader" /> that will be read for the content of the <see cref="JProperty" />.</param>
    /// <param name="settings">
    /// The <see cref="JsonLoadSettings" /> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> representing the asynchronous creation. The <see cref="Task{TResult}.Result" />
    /// property returns a <see cref="JProperty" /> that contains the JSON that was read from the specified <see cref="JsonReader" />.
    /// </returns>
    public new static async Task<JProperty> LoadAsync(JsonReader reader, JsonLoadSettings? settings, Cancel cancel = default)
    {
        if (reader.TokenType == JsonToken.None)
        {
            if (!await reader.ReadAsync(cancel).ConfigureAwait(false))
            {
                throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader.");
            }
        }

        await reader.MoveToContentAsync(cancel).ConfigureAwait(false);

        if (reader.TokenType != JsonToken.PropertyName)
        {
            throw JsonReaderException.Create(reader, $"Error reading JProperty from JsonReader. Current JsonReader item is not a property: {reader.TokenType}");
        }

        var p = new JProperty((string) reader.Value!);
        p.SetLineInfo(reader as IJsonLineInfo, settings);

        await p.ReadTokenFromAsync(reader, settings, cancel).ConfigureAwait(false);

        return p;
    }
}