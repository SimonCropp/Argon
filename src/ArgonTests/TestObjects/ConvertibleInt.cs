// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public struct ConvertibleInt(int value) :
    IConvertible
{
    public TypeCode GetTypeCode() =>
        TypeCode.Int32;

    public bool ToBoolean(IFormatProvider provider) =>
        throw new NotImplementedException();

    public byte ToByte(IFormatProvider provider) =>
        throw new NotImplementedException();

    public char ToChar(IFormatProvider provider) =>
        throw new NotImplementedException();

    public DateTime ToDateTime(IFormatProvider provider) =>
        throw new NotImplementedException();

    public decimal ToDecimal(IFormatProvider provider) =>
        throw new NotImplementedException();

    public double ToDouble(IFormatProvider provider) =>
        throw new NotImplementedException();

    public short ToInt16(IFormatProvider provider) =>
        throw new NotImplementedException();

    public int ToInt32(IFormatProvider provider) =>
        throw new NotImplementedException();

    public long ToInt64(IFormatProvider provider) =>
        throw new NotImplementedException();

    public sbyte ToSByte(IFormatProvider provider) =>
        throw new NotImplementedException();

    public float ToSingle(IFormatProvider provider) =>
        throw new NotImplementedException();

    public string ToString(IFormatProvider provider) =>
        throw new NotImplementedException();

    public object ToType(Type conversionType, IFormatProvider provider)
    {
        if (conversionType == typeof(int))
        {
            return value;
        }

        throw new($"Type not supported: {conversionType.FullName}");
    }

    public ushort ToUInt16(IFormatProvider provider) =>
        throw new NotImplementedException();

    public uint ToUInt32(IFormatProvider provider) =>
        throw new NotImplementedException();

    public ulong ToUInt64(IFormatProvider provider) =>
        throw new NotImplementedException();
}