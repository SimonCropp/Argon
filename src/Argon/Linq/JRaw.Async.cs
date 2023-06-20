// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public partial class JRaw
{
    /// <summary>
    /// Asynchronously creates an instance of <see cref="JRaw" /> with the content of the reader's current token.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>
    /// A <see cref="Task{TResult}" /> representing the asynchronous creation. The <see cref="Task{TResult}.Result" />
    /// property returns an instance of <see cref="JRaw" /> with the content of the reader's current token.
    /// </returns>
    public static async Task<JRaw> CreateAsync(JsonReader reader, Cancel cancel = default)
    {
        using var stringWriter = new StringWriter(InvariantCulture);
        using var jsonWriter = new JsonTextWriter(stringWriter);
        await jsonWriter.WriteTokenSyncReadingAsync(reader, cancel).ConfigureAwait(false);

        return new(stringWriter.ToString());
    }
}