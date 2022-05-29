// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class SubKlass : SuperKlass
{
    public string SubProp { get; set; }

    public SubKlass(string subprop) =>
        SubProp = subprop;
}