// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class NonPublicConstructorWithJsonConstructor
{
    public string Value { get; }
    public string Constructor { get; }

    [Argon.JsonConstructor]
    NonPublicConstructorWithJsonConstructor() =>
        Constructor = "NonPublic";

    public NonPublicConstructorWithJsonConstructor(string value)
    {
        Value = value;
        Constructor = "Public Parameterized";
    }
}