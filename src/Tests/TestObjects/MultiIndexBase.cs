// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public abstract class MultiIndexBase
{
    protected internal object this[string propertyName]
    {
        get => null;
        set { }
    }

    protected internal object this[object property]
    {
        get => null;
        set { }
    }
}