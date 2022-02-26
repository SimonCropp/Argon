// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

[TypeConverter(typeof(MyInterfaceConverter))]
interface IMyInterface
{
    string Name { get; }

    string PrintTest();
}