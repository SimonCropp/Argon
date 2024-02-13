// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// The exception thrown when an error occurs while reading JSON text.
/// </summary>
public class JsonReaderException : JsonException
{
    /// <summary>
    /// Gets the line number indicating where the error occurred.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the line position indicating where the error occurred.
    /// </summary>
    public int LinePosition { get; }

    /// <summary>
    /// Gets the path to the JSON where the error occurred.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonReaderException" /> class
    /// with a specified error message, JSON path, line number, line position, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="path">The path to the JSON where the error occurred.</param>
    /// <param name="lineNumber">The line number indicating where the error occurred.</param>
    /// <param name="linePosition">The line position indicating where the error occurred.</param>
    public JsonReaderException(string message, string path, int lineNumber, int linePosition, Exception? innerException)
        : base(message, innerException)
    {
        Path = path;
        LineNumber = lineNumber;
        LinePosition = linePosition;
    }

    internal static JsonReaderException Create(JsonReader reader, string message, Exception? exception = null) =>
        Create(reader as IJsonLineInfo, reader.Path, message, exception);

    internal static JsonReaderException Create(IJsonLineInfo? info, string path, string message, Exception? exception)
    {
        message = JsonPosition.FormatMessage(info, path, message);

        if (info != null &&
            info.HasLineInfo())
        {
            var lineNumber = info.LineNumber;
            var linePosition = info.LinePosition;
            return new(message, path, lineNumber, linePosition, exception);
        }

        return new(message, path, 0, 0, exception);
    }
}