// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public partial class JTokenWriter
{
    // This is the only method that can benefit from Task-based asynchronicity, and that only when
    // the reader provides it.
    internal override Task WriteTokenAsync(JsonReader reader, bool writeChildren, bool writeComments, Cancel cancel)
    {
        // Since JTokenReader is a common target (and with an optimised path) and since it can't
        // read truly async, catch that case.
        if (reader is JTokenReader)
        {
            WriteToken(reader, writeChildren, writeComments);
            return Task.CompletedTask;
        }

        return WriteTokenSyncReadingAsync(reader, cancel);
    }
}