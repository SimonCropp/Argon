// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class Co : ICo
{
    public string Name { get; set; }
}

public interface ICo
{
    string Name { get; set; }
}

public interface IInterfacePropertyTestClass
{
    ICo co { get; set; }
}

public class InterfacePropertyTestClass : IInterfacePropertyTestClass
{
    public ICo co { get; set; }

    public InterfacePropertyTestClass()
    {
    }
}