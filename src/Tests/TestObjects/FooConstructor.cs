// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class FooConstructor
{
    [JsonProperty(PropertyName = "something_else")]
    public readonly string Bar;

    public FooConstructor(string bar)
    {
        Bar = bar;
    }
}