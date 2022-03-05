// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Provides data for the Error event.
/// </summary>
public class ErrorEventArgs : EventArgs
{
    /// <summary>
    /// Gets the current object the error event is being raised against.
    /// </summary>
    public object? CurrentObject { get; }

    /// <summary>
    /// Gets the error context.
    /// </summary>
    public ErrorContext ErrorContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorEventArgs" /> class.
    /// </summary>
    public ErrorEventArgs(object? currentObject, ErrorContext errorContext)
    {
        CurrentObject = currentObject;
        ErrorContext = errorContext;
    }
}