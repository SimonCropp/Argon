// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ConvertibleId : IConvertible
{
    public int Value;

    TypeCode IConvertible.GetTypeCode() =>
        TypeCode.Object;

    object IConvertible.ToType(Type conversionType, IFormatProvider provider)
    {
        if (conversionType == typeof(object))
        {
            return this;
        }
        if (conversionType == typeof(int))
        {
            return Value;
        }
        if (conversionType == typeof(long))
        {
            return (long)Value;
        }
        if (conversionType == typeof(string))
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
        throw new InvalidCastException();
    }

    bool IConvertible.ToBoolean(IFormatProvider provider) =>
        throw new InvalidCastException();

    byte IConvertible.ToByte(IFormatProvider provider) =>
        throw new InvalidCastException();

    char IConvertible.ToChar(IFormatProvider provider) =>
        throw new InvalidCastException();

    DateTime IConvertible.ToDateTime(IFormatProvider provider) =>
        throw new InvalidCastException();

    decimal IConvertible.ToDecimal(IFormatProvider provider) =>
        throw new InvalidCastException();

    double IConvertible.ToDouble(IFormatProvider provider) =>
        throw new InvalidCastException();

    short IConvertible.ToInt16(IFormatProvider provider) =>
        (short)Value;

    int IConvertible.ToInt32(IFormatProvider provider) =>
        Value;

    long IConvertible.ToInt64(IFormatProvider provider) =>
        Value;

    sbyte IConvertible.ToSByte(IFormatProvider provider) =>
        throw new InvalidCastException();

    float IConvertible.ToSingle(IFormatProvider provider) =>
        throw new InvalidCastException();

    string IConvertible.ToString(IFormatProvider provider) =>
        throw new InvalidCastException();

    ushort IConvertible.ToUInt16(IFormatProvider provider) =>
        throw new InvalidCastException();

    uint IConvertible.ToUInt32(IFormatProvider provider) =>
        throw new InvalidCastException();

    ulong IConvertible.ToUInt64(IFormatProvider provider) =>
        throw new InvalidCastException();
}