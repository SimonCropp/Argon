// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// The exception thrown when an error occurs while writing JSON text.
/// </summary>
public class JsonWriterException : JsonException
{
    /// <summary>
    /// Gets the path to the JSON where the error occurred.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonWriterException"/> class
    /// with a specified error message, JSON path and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="path">The path to the JSON where the error occurred.</param>
    public JsonWriterException(string message, string path)
        : base(message)
    {
        Path = path;
    }

    internal static JsonWriterException Create(JsonWriter writer, string message)
    {
        message = JsonPosition.FormatMessage(null, writer.ContainerPath, message);

        return new(message, writer.ContainerPath);
    }
}