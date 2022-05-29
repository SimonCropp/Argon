// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonConverter(typeof(ClassConverterPrecedenceClassConverter))]
public class ConverterPrecedenceClass
{
    public string TestValue { get; set; }

    public ConverterPrecedenceClass(string testValue) =>
        TestValue = testValue;
}