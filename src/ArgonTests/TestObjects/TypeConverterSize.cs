// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

[TypeConverter(typeof(TypeConverterSizeConverter))]
public struct TypeConverterSize(int width, int height)
{
    public static readonly TypeConverterSize Empty;

    public int Width { get; set; } = width;

    public int Height { get; set; } = height;
}