// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// The exception thrown when an error occurs during JSON serialization or deserialization.
/// </summary>
public class JsonException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonException" /> class
    /// with a specified error message.
    /// </summary>
    public JsonException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonException" /> class
    /// with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public JsonException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}