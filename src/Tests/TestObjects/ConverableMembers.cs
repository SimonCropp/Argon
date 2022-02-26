// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject]
public class ConverableMembers
{
    public string String = "string";
    public int Int32 = int.MaxValue;
    public uint UInt32 = uint.MaxValue;
    public byte Byte = byte.MaxValue;
    public sbyte SByte = sbyte.MaxValue;
    public short Short = short.MaxValue;
    public ushort UShort = ushort.MaxValue;
    public long Long = long.MaxValue;
    public ulong ULong = long.MaxValue;
    public double Double = double.MaxValue;
    public float Float = float.MaxValue;
    public DBNull DBNull = DBNull.Value;
    public bool Bool = true;
    public char Char = '\0';
}