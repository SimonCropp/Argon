// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class DerivedConstructorType : BaseConstructorType
{
    public DerivedConstructorType(string baseProperty, string derivedProperty)
        : base(baseProperty) =>
        DerivedProperty = derivedProperty;

    [JsonProperty]
    public string DerivedProperty { get; }
}