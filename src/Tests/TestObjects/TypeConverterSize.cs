// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

[TypeConverter(typeof(TypeConverterSizeConverter))]
public struct TypeConverterSize
{
    public static readonly TypeConverterSize Empty;

    public TypeConverterSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; set; }

    public int Height { get; set; }
}