// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

class TraceWriter : IMyInterface
{
    public string Name => "Trace Writer";

    public override string ToString() =>
        Name;

    public string PrintTest() =>
        "TraceWriter";
}