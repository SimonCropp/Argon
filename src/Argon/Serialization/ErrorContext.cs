// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Provides information surrounding an error.
/// </summary>
public class ErrorContext
{
    internal ErrorContext(object? originalObject, object? member, string path, Exception exception)
    {
        OriginalObject = originalObject;
        Member = member;
        Exception = exception;
        Path = path;
    }

    /// <summary>
    /// Gets the error.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the original object that caused the error.
    /// </summary>
    public object? OriginalObject { get; }

    /// <summary>
    /// Gets the member that caused the error.
    /// </summary>
    public object? Member { get; }

    /// <summary>
    /// Gets the path of the JSON location where the error occurred.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ErrorContext" /> is handled.
    /// </summary>
    public bool Handled { get; set; }
}