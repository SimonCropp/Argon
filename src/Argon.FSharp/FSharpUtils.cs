// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public static class FSharpConverters
{
    public static IReadOnlyList<JsonConverter> Instances { get; } = new JsonConverter[]
    {
        new FSharpListConverter(),
        new FSharpMapConverter(),
        new DiscriminatedUnionConverter()
    };
}