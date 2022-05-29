// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class MyFactory
{
    public static ISubclass InstantiateSubclass() =>
        new Subclass
        {
            ID = 123,
            Name = "ABC",
            P1 = true,
            P2 = 44
        };

    public static IMainClass InstantiateManiClass() =>
        new MainClass
        {
            ID = 567,
            Name = "XYZ",
            Subclass = InstantiateSubclass()
        };
}