// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PublicParameterizedConstructorRequiringConverterTestClass
{
    readonly NameContainer _nameContainer;

    public PublicParameterizedConstructorRequiringConverterTestClass(NameContainer nameParameter)
    {
        _nameContainer = nameParameter;
    }

    public NameContainer Name => _nameContainer;
}