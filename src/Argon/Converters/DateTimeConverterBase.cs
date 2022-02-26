// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Provides a base class for converting a <see cref="DateTime"/> to and from JSON.
/// </summary>
public abstract class DateTimeConverterBase : JsonConverter
{
    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type type)
    {
        if (type == typeof(DateTime) ||
            type == typeof(DateTime?))
        {
            return true;
        }

        return type == typeof(DateTimeOffset) ||
               type == typeof(DateTimeOffset?);
    }
}