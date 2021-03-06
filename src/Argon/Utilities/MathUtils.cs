// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class MathUtils
{
    public static int IntLength(ulong i)
    {
        if (i < 10000000000)
        {
            if (i < 10)
            {
                return 1;
            }

            if (i < 100)
            {
                return 2;
            }

            if (i < 1000)
            {
                return 3;
            }

            if (i < 10000)
            {
                return 4;
            }

            if (i < 100000)
            {
                return 5;
            }

            if (i < 1000000)
            {
                return 6;
            }

            if (i < 10000000)
            {
                return 7;
            }

            if (i < 100000000)
            {
                return 8;
            }

            if (i < 1000000000)
            {
                return 9;
            }

            return 10;
        }

        if (i < 100000000000)
        {
            return 11;
        }

        if (i < 1000000000000)
        {
            return 12;
        }

        if (i < 10000000000000)
        {
            return 13;
        }

        if (i < 100000000000000)
        {
            return 14;
        }

        if (i < 1000000000000000)
        {
            return 15;
        }

        if (i < 10000000000000000)
        {
            return 16;
        }

        if (i < 100000000000000000)
        {
            return 17;
        }

        if (i < 1000000000000000000)
        {
            return 18;
        }

        if (i < 10000000000000000000)
        {
            return 19;
        }

        return 20;
    }

    public static char IntToHex(int n)
    {
        if (n <= 9)
        {
            return (char) (n + 48);
        }

        return (char) (n - 10 + 97);
    }

    public static bool ApproxEquals(double d1, double d2)
    {
        const double epsilon = 2.2204460492503131E-16;

        if (d1 == d2)
        {
            return true;
        }

        var tolerance = (Math.Abs(d1) + Math.Abs(d2) + 10.0) * epsilon;
        var difference = d1 - d2;

        return -tolerance < difference && tolerance > difference;
    }
}