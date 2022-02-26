// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

class HasByteArray
{
    public byte[] EncryptedPassword { get; set; }
}

class HasMultidimensionalByteArray
{
    public byte[,] Array2D { get; set; }
    public byte[,,] Array3D { get; set; }
}