// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PublicParameterizedConstructorWithPropertyNameConflictWithAttribute([JsonProperty("name")] string nameParameter)
{
    public int Name { get; } = Convert.ToInt32(nameParameter);
}