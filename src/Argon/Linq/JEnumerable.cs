// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Represents a collection of <see cref="JToken" /> objects.
/// </summary>
/// <typeparam name="T">The type of token.</typeparam>
public readonly struct JEnumerable<T> :
    IJEnumerable<T>,
    IEquatable<JEnumerable<T>>
    where T : JToken
{
    /// <summary>
    /// An empty collection of <see cref="JToken" /> objects.
    /// </summary>
    public static readonly JEnumerable<T> Empty = new(Enumerable.Empty<T>());

    readonly IEnumerable<T> enumerable;

    /// <summary>
    /// Initializes a new instance of the <see cref="JEnumerable{T}" /> struct.
    /// </summary>
    public JEnumerable(IEnumerable<T> enumerable) =>
        this.enumerable = enumerable;

    /// <summary>
    /// Returns an enumerator that can be used to iterate through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}" /> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<T> GetEnumerator() =>
        (enumerable ?? Empty).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    /// <summary>
    /// Gets the <see cref="IJEnumerable{T}" /> of <see cref="JToken" /> with the specified key.
    /// </summary>
    public IJEnumerable<JToken> this[object key]
    {
        get
        {
            if (enumerable == null)
            {
                return JEnumerable<JToken>.Empty;
            }

            return new JEnumerable<JToken>(enumerable.Values<T, JToken>(key)!);
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="JEnumerable{T}" /> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="JEnumerable{T}" /> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="JEnumerable{T}" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(JEnumerable<T> other) =>
        Equals(enumerable, other.enumerable);

    /// <summary>
    /// Determines whether the specified <see cref="Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="Object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj is JEnumerable<T> enumerable)
        {
            return Equals(enumerable);
        }

        return false;
    }

    public override int GetHashCode()
    {
        if (enumerable == null)
        {
            return 0;
        }

        return enumerable.GetHashCode();
    }
}