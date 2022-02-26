// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PersonWithPrivateConstructor
{
    PersonWithPrivateConstructor()
    {
    }

    public static PersonWithPrivateConstructor CreatePerson()
    {
        return new PersonWithPrivateConstructor();
    }

    public string Name { get; set; }

    public int Age { get; set; }
}