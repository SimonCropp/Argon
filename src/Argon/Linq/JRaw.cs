// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Represents a raw JSON string.
/// </summary>
public partial class JRaw : JValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JRaw"/> class from another <see cref="JRaw"/> object.
    /// </summary>
    /// <param name="other">A <see cref="JRaw"/> object to copy from.</param>
    public JRaw(JRaw other)
        : base(other)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JRaw"/> class.
    /// </summary>
    public JRaw(object? rawJson)
        : base(rawJson, JTokenType.Raw)
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="JRaw"/> with the content of the reader's current token.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>An instance of <see cref="JRaw"/> with the content of the reader's current token.</returns>
    public static JRaw Create(JsonReader reader)
    {
        using var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
        using var jsonWriter = new JsonTextWriter(stringWriter);
        jsonWriter.WriteToken(reader);

        return new JRaw(stringWriter.ToString());
    }

    internal override JToken CloneToken()
    {
        return new JRaw(this);
    }
}