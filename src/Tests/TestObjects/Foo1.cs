// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class Foo1
{
    public object foo { get; set; }
}

public class Foo1<T> : Foo1
{
    public new T foo { get; set; }

    public T foo2 { get; set; }
}