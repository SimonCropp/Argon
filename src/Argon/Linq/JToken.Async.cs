// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public abstract partial class JToken
{
    /// <summary>
    /// Writes this token to a <see cref="JsonWriter" /> asynchronously.
    /// </summary>
    public virtual Task WriteToAsync(JsonWriter writer, Cancellation cancellation, params JsonConverter[] converters) =>
        throw new NotImplementedException();

    /// <summary>
    /// Writes this token to a <see cref="JsonWriter" /> asynchronously.
    /// </summary>
    public Task WriteToAsync(JsonWriter writer, params JsonConverter[] converters) =>
        WriteToAsync(writer, default, converters);

    /// <summary>
    /// Asynchronously creates a <see cref="JToken" /> from a <see cref="JsonReader" />.
    /// </summary>
    /// <param name="reader">An <see cref="JsonReader" /> positioned at the token to read into this <see cref="JToken" />.</param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous creation. The
    /// <see cref="Task{TResult}.Result" /> property returns a <see cref="JToken" /> that contains
    /// the token and its descendant tokens
    /// that were read from the reader. The runtime type of the token is determined
    /// by the token type of the first token encountered in the reader.
    /// </returns>
    public static Task<JToken> ReadFromAsync(JsonReader reader, Cancellation cancellation = default) =>
        ReadFromAsync(reader, null, cancellation);

    /// <summary>
    /// Asynchronously creates a <see cref="JToken" /> from a <see cref="JsonReader" />.
    /// </summary>
    /// <param name="reader">An <see cref="JsonReader" /> positioned at the token to read into this <see cref="JToken" />.</param>
    /// <param name="settings">
    /// The <see cref="JsonLoadSettings" /> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous creation. The
    /// <see cref="Task{TResult}.Result" /> property returns a <see cref="JToken" /> that contains
    /// the token and its descendant tokens
    /// that were read from the reader. The runtime type of the token is determined
    /// by the token type of the first token encountered in the reader.
    /// </returns>
    public static async Task<JToken> ReadFromAsync(JsonReader reader, JsonLoadSettings? settings, Cancellation cancellation = default)
    {
        if (reader.TokenType == JsonToken.None)
        {
            if (!await (settings is {CommentHandling: CommentHandling.Ignore} ? reader.ReadAndMoveToContentAsync(cancellation) : reader.ReadAsync(cancellation)).ConfigureAwait(false))
            {
                throw JsonReaderException.Create(reader, "Error reading JToken from JsonReader.");
            }
        }

        var lineInfo = reader as IJsonLineInfo;

        switch (reader.TokenType)
        {
            case JsonToken.StartObject:
                return await JObject.LoadAsync(reader, settings, cancellation).ConfigureAwait(false);
            case JsonToken.StartArray:
                return await JArray.LoadAsync(reader, settings, cancellation).ConfigureAwait(false);
            case JsonToken.PropertyName:
                return await JProperty.LoadAsync(reader, settings, cancellation).ConfigureAwait(false);
            case JsonToken.String:
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.Date:
            case JsonToken.Boolean:
            case JsonToken.Bytes:
                var v = new JValue(reader.Value);
                v.SetLineInfo(lineInfo, settings);
                return v;
            case JsonToken.Comment:
                v = JValue.CreateComment(reader.Value?.ToString());
                v.SetLineInfo(lineInfo, settings);
                return v;
            case JsonToken.Null:
                v = JValue.CreateNull();
                v.SetLineInfo(lineInfo, settings);
                return v;
            case JsonToken.Undefined:
                v = JValue.CreateUndefined();
                v.SetLineInfo(lineInfo, settings);
                return v;
            default:
                throw JsonReaderException.Create(reader, $"Error reading JToken from JsonReader. Unexpected token: {reader.TokenType}");
        }
    }

    /// <summary>
    /// Asynchronously creates a <see cref="JToken" /> from a <see cref="JsonReader" />.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader" /> positioned at the token to read into this <see cref="JToken" />.</param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous creation. The <see cref="Task{TResult}.Result" />
    /// property returns a <see cref="JToken" /> that contains the token and its descendant tokens
    /// that were read from the reader. The runtime type of the token is determined
    /// by the token type of the first token encountered in the reader.
    /// </returns>
    public static Task<JToken> LoadAsync(JsonReader reader, Cancellation cancellation = default) =>
        LoadAsync(reader, null, cancellation);

    /// <summary>
    /// Asynchronously creates a <see cref="JToken" /> from a <see cref="JsonReader" />.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader" /> positioned at the token to read into this <see cref="JToken" />.</param>
    /// <param name="settings">
    /// The <see cref="JsonLoadSettings" /> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> that represents the asynchronous creation. The <see cref="Task{TResult}.Result" />
    /// property returns a <see cref="JToken" /> that contains the token and its descendant tokens
    /// that were read from the reader. The runtime type of the token is determined
    /// by the token type of the first token encountered in the reader.
    /// </returns>
    public static Task<JToken> LoadAsync(JsonReader reader, JsonLoadSettings? settings, Cancellation cancellation = default) =>
        ReadFromAsync(reader, settings, cancellation);
}