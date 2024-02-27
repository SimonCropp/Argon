// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestCaseSource = Xunit.MemberDataAttribute;

public class EnumUtilsTests : TestFixtureBase
{
    [Theory]
    [TestCaseSource(nameof(Parse_TestData))]
    public void Parse(string value, object expected)
    {
        var enumType = expected.GetType();

        var result = (Enum) EnumUtils.ParseEnum(enumType, null, value, false);
        Assert.Equal(expected, result);
    }

    [Theory]
    [TestCaseSource(nameof(Parse_Invalid_TestData))]
    public void Parse_Invalid(Type enumType, string value, Type exceptionType)
    {
        try
        {
            EnumUtils.ParseEnum(enumType, null, value, false);
        }
        catch (Exception exception) when (exception.GetType() == exceptionType)
        {
            // nom nom nom
            return;
        }

        Assert.Fail($"Expected {exceptionType.FullName} exception.");
    }

    [Theory]
    [TestCaseSource(nameof(ToString_Format_TestData))]
    public static void ToString_Format(Enum e, string expected)
    {
        EnumUtils.TryToString(e.GetType(), e, null, out var result);

        Assert.Equal(expected, result);
    }

    #region Test data

    // test data from https://github.com/dotnet/corefx/blob/master/src/System.Runtime/tests/System/EnumTests.cs
    public static IEnumerable<object[]> Parse_TestData()
    {
        // SByte
        yield return ["Min", SByteEnum.Min];
        yield return ["mAx", SByteEnum.Max];
        yield return ["1", SByteEnum.One];
        yield return ["5", (SByteEnum) 5];

        // Byte
        yield return ["Min", ByteEnum.Min];
        yield return ["mAx", ByteEnum.Max];
        yield return ["1", ByteEnum.One];
        yield return ["5", (ByteEnum) 5];

        // Int16
        yield return ["Min", Int16Enum.Min];
        yield return ["mAx", Int16Enum.Max];
        yield return ["1", Int16Enum.One];
        yield return ["5", (Int16Enum) 5];

        // UInt16
        yield return ["Min", UInt16Enum.Min];
        yield return ["mAx", UInt16Enum.Max];
        yield return ["1", UInt16Enum.One];
        yield return ["5", (UInt16Enum) 5];

        // Int32
        yield return ["Min", Int32Enum.Min];
        yield return ["mAx", Int32Enum.Max];
        yield return ["1", Int32Enum.One];
        yield return ["5", (Int32Enum) 5];

        // UInt32
        yield return ["Min", UInt32Enum.Min];
        yield return ["mAx", UInt32Enum.Max];
        yield return ["1", UInt32Enum.One];
        yield return ["5", (UInt32Enum) 5];

        // Int64
        yield return ["Min", Int64Enum.Min];
        yield return ["mAx", Int64Enum.Max];
        yield return ["1", Int64Enum.One];
        yield return ["5", (Int64Enum) 5];

        // UInt64
        yield return ["Min", UInt64Enum.Min];
        yield return ["mAx", UInt64Enum.Max];
        yield return ["1", UInt64Enum.One];
        yield return ["5", (UInt64Enum) 5];

        // SimpleEnum
        yield return ["Red", SimpleEnum.Red];
        yield return [" Red", SimpleEnum.Red];
        yield return ["Red ", SimpleEnum.Red];
        yield return [" red ", SimpleEnum.Red];
        yield return ["B", SimpleEnum.B];
        yield return ["B,B", SimpleEnum.B];
        yield return [" Red , Blue ", SimpleEnum.Red | SimpleEnum.Blue];
        yield return ["Blue,Red,Green", SimpleEnum.Red | SimpleEnum.Blue | SimpleEnum.Green];
        yield return ["Blue,Red,Red,Red,Green", SimpleEnum.Red | SimpleEnum.Blue | SimpleEnum.Green];
        yield return ["Red,Blue,   Green", SimpleEnum.Red | SimpleEnum.Blue | SimpleEnum.Green];
        yield return ["1", SimpleEnum.Red];
        yield return [" 1 ", SimpleEnum.Red];
        yield return ["2", SimpleEnum.Blue];
        yield return ["99", (SimpleEnum) 99];
        yield return ["-42", (SimpleEnum) (-42)];
        yield return ["   -42", (SimpleEnum) (-42)];
        yield return ["   -42 ", (SimpleEnum) (-42)];
    }

    // test data from https://github.com/dotnet/corefx/blob/master/src/System.Runtime/tests/System/EnumTests.cs
    public static IEnumerable<object[]> Parse_Invalid_TestData()
    {
        // SimpleEnum
        yield return [typeof(object), "", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "    \t", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "Purple", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), ",Red", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "Red,", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "B,", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), " , , ,", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "Red,Blue,", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "Red,,Blue", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "Red,Blue, ", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "Red Blue", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "1,Blue", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "Blue,1", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "Blue, 1", typeof(ArgumentException)];
        yield return [typeof(SimpleEnum), "2147483649", typeof(OverflowException)];
        yield return [typeof(SimpleEnum), "2147483648", typeof(OverflowException)];
    }

    // test data from https://github.com/dotnet/corefx/blob/master/src/System.Runtime/tests/System/EnumTests.cs
    public static IEnumerable<object[]> ToString_Format_TestData()
    {
        yield return [SByteEnum.Min, "Min"];
        yield return [(SByteEnum) 5, null];
        yield return [SByteEnum.Max, "Max"];

        yield return [ByteEnum.Min, "Min"];
        yield return [(ByteEnum) 5, null];
        yield return [(ByteEnum) 0xff, "Max"];
        yield return [(ByteEnum) 3, null];

        yield return [Int16Enum.Min, "Min"];
        yield return [(Int16Enum) 5, null];
        yield return [Int16Enum.Max, "Max"];
        yield return [(Int16Enum) 3, null];

        yield return [UInt16Enum.Min, "Min"];
        yield return [(UInt16Enum) 5, null];
        yield return [UInt16Enum.Max, "Max"];
        yield return [(UInt16Enum) 3, null];

        yield return [Int32Enum.Min, "Min"];
        yield return [(Int32Enum) 5, null];
        yield return [Int32Enum.Max, "Max"];
        yield return [(Int32Enum) 3, null];

        yield return [UInt32Enum.Min, "Min"];
        yield return [(UInt32Enum) 5, null];
        yield return [UInt32Enum.Max, "Max"];
        yield return [(UInt32Enum) 3, null];

        yield return [Int64Enum.Min, "Min"];
        yield return [(Int64Enum) 5, null];
        yield return [Int64Enum.Max, "Max"];
        yield return [(Int64Enum) 3, null];

        yield return [UInt64Enum.Min, "Min"];
        yield return [(UInt64Enum) 5, null];
        yield return [UInt64Enum.Max, "Max"];
        yield return [(UInt64Enum) 3, null];

        yield return [SimpleEnum.Red, "Red"];
        yield return [SimpleEnum.Blue, "Blue"];
        yield return [(SimpleEnum) 99, null];
        yield return [(SimpleEnum) 0, null];

        yield return [AttributeTargets.Class | AttributeTargets.Delegate, "Class, Delegate"];
    }

    #endregion
}

public enum SimpleEnum
{
    Red = 1,
    Blue = 2,
    Green = 3,
    Green_a = 3,
    Green_b = 3,
    B = 4
}

public enum ByteEnum : byte
{
    Min = byte.MinValue,
    One = 1,
    Two = 2,
    Max = byte.MaxValue
}

public enum SByteEnum : sbyte
{
    Min = sbyte.MinValue,
    One = 1,
    Two = 2,
    Max = sbyte.MaxValue
}

public enum UInt16Enum : ushort
{
    Min = ushort.MinValue,
    One = 1,
    Two = 2,
    Max = ushort.MaxValue
}

public enum Int16Enum : short
{
    Min = short.MinValue,
    One = 1,
    Two = 2,
    Max = short.MaxValue
}

public enum UInt32Enum : uint
{
    Min = uint.MinValue,
    One = 1,
    Two = 2,
    Max = uint.MaxValue
}

public enum Int32Enum
{
    Min = int.MinValue,
    One = 1,
    Two = 2,
    Max = int.MaxValue
}

public enum UInt64Enum : ulong
{
    Min = ulong.MinValue,
    One = 1,
    Two = 2,
    Max = ulong.MaxValue
}

public enum Int64Enum : long
{
    Min = long.MinValue,
    One = 1,
    Two = 2,
    Max = long.MaxValue
}