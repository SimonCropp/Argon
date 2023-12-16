// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public static class FSharpConverters
{
    public static JsonConverter[] Instances { get; } =
    [
        new FSharpListConverter(),
        new FSharpMapConverter(),
        new DiscriminatedUnionConverter()
    ];

    public static void AddFSharpConverters(this JsonSerializerSettings settings) =>
        settings.Converters.AddRange(Instances);
}