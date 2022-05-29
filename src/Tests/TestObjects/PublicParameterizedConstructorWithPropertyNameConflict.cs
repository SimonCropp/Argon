// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PublicParameterizedConstructorWithPropertyNameConflict
{
    public PublicParameterizedConstructorWithPropertyNameConflict(string name) =>
        Name = Convert.ToInt32(name);

    public int Name { get; }
}