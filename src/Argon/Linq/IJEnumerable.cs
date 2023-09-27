// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Represents a collection of <see cref="JToken" /> objects.
/// </summary>
/// <typeparam name="T">The type of token.</typeparam>
public interface IJEnumerable<out T> :
    IEnumerable<T>
    where T : JToken
{
    /// <summary>
    /// Gets the <see cref="IJEnumerable{T}" /> of <see cref="JToken" /> with the specified key.
    /// </summary>
    IJEnumerable<JToken> this[object key] { get; }
}