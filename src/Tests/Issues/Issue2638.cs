// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2638
{
    [Fact]
    public void DeserializeUsesSharedBooleans()
    {
        Test(true);
        Test(false);

        static void Test(bool value)
        {
            var obj = (JObject) JToken.Parse(@"{""x"": XXX, ""y"": XXX}".Replace("XXX", value ? "true" : "false"));
            var x = ((JValue) obj["x"]).Value;
            var y = ((JValue) obj["y"]).Value;

            Assert.Equal(value, (bool) x);
            Assert.Equal(value, (bool) y);
            Assert.Same(x, y);
        }
    }

    [Fact]
    public void DeserializeUsesSharedDoubleZeros()
    {
        Test(0, true);
        Test(double.NaN, true);
        Test(double.NegativeInfinity, true);
        Test(double.PositiveInfinity, true);
        Test(1, false);
        Test(42.42, false);

        static void Test(double value, bool expectSame)
        {
            var obj = (JObject) JToken.Parse(@"{""x"": XXX, ""y"": XXX}".Replace("XXX", value.ToString("0.0###", CultureInfo.InvariantCulture)));
            var x = ((JValue) obj["x"]).Value;
            var y = ((JValue) obj["y"]).Value;

            Assert.Equal(value, (double) x);
            Assert.Equal(value, (double) y);
            if (expectSame)
            {
                Assert.Same(x, y);
            }
            else
            {
                Assert.NotSame(x, y);
            }

            var unboxed = (double) x;
            Assert.Equal(double.IsNaN(value), double.IsNaN(unboxed));
            Assert.Equal(double.IsPositiveInfinity(value), double.IsPositiveInfinity(unboxed));
            Assert.Equal(double.IsNegativeInfinity(value), double.IsNegativeInfinity(unboxed));
        }
    }

    [Fact]
    public void DeserializeUsesSharedSmallInt64()
    {
        Test(-2, false);
        Test(-1, true);
        Test(0, true);
        Test(1, true);
        Test(2, true);
        Test(3, true);
        Test(4, true);
        Test(5, true);
        Test(6, true);
        Test(7, true);
        Test(8, true);
        Test(9, false);

        static void Test(long value, bool expectSame)
        {
            var obj = (JObject) JToken.Parse(@"{""x"": XXX, ""y"": XXX}".Replace("XXX", value.ToString(CultureInfo.InvariantCulture)));
            var x = ((JValue) obj["x"]).Value;
            var y = ((JValue) obj["y"]).Value;

            Assert.Equal(value, (long) x);
            Assert.Equal(value, (long) y);
            if (expectSame)
            {
                Assert.Equal(x, y);
            }
            else
            {
                Assert.NotSame(x, y);
            }
        }
    }
}