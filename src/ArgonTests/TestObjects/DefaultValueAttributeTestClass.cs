// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;
#if !NET6_0_OR_GREATER
[Description("DefaultValueAttributeTestClass description!")]
#endif
public sealed class DefaultValueAttributeTestClass
{
    [DefaultValue("TestProperty1Value")]
    public string TestProperty1 { get; set; }

    [DefaultValue(21)]
    public int TestField1;
}