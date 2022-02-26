// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ContentSubClass : ContentBaseClass
{
    public ContentSubClass()
    {
    }

    public ContentSubClass(string EasyIn)
    {
        SomeString = EasyIn;
    }

    public string SomeString { get; set; }
}