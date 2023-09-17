// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class NamingStrategyEquality : TestFixtureBase
{
    [Fact]
    public void CamelCaseNamingStrategyEquality()
    {
        var s1 = new CamelCaseNamingStrategy();
        var s2 = new CamelCaseNamingStrategy();
        Assert.True(s1.Equals(s2));
        Assert.True(s1.GetHashCode() == s2.GetHashCode());
    }

    [Fact]
    public void CamelCaseNamingStrategyEqualityVariants()
    {
        CheckInequality<CamelCaseNamingStrategy>(false, true);
        CheckInequality<CamelCaseNamingStrategy>(true, false);
        CheckInequality<CamelCaseNamingStrategy>(false, true);
        CheckInequality<CamelCaseNamingStrategy>(true, true);
        CheckInequality<CamelCaseNamingStrategy>(true, true);
    }

    [Fact]
    public void DefaultNamingStrategyEquality()
    {
        var s1 = new DefaultNamingStrategy();
        var s2 = new DefaultNamingStrategy();
        Assert.True(s1.Equals(s2));
        Assert.True(s1.GetHashCode() == s2.GetHashCode());
    }

    [Fact]
    public void DefaultNamingStrategyEqualityVariants()
    {
        CheckInequality<DefaultNamingStrategy>(false, true);
        CheckInequality<DefaultNamingStrategy>(true, false);
    }

    [Fact]
    public void SnakeCaseStrategyEquality()
    {
        var s1 = new SnakeCaseNamingStrategy();
        var s2 = new SnakeCaseNamingStrategy();
        Assert.True(s1.Equals(s2));
        Assert.True(s1.GetHashCode() == s2.GetHashCode());
    }

    [Fact]
    public void SnakeCaseNamingStrategyEqualityVariants()
    {
        CheckInequality<SnakeCaseNamingStrategy>(false, true);
        CheckInequality<SnakeCaseNamingStrategy>(true, false);
    }

    [Fact]
    public void KebabCaseStrategyEquality()
    {
        var s1 = new KebabCaseNamingStrategy();
        var s2 = new KebabCaseNamingStrategy();
        Assert.True(s1.Equals(s2));
        Assert.True(s1.GetHashCode() == s2.GetHashCode());
    }

    [Fact]
    public void KebabCaseNamingStrategyEqualityVariants()
    {
        CheckInequality<KebabCaseNamingStrategy>(false, true);
        CheckInequality<KebabCaseNamingStrategy>(true, false);
    }

    [Fact]
    public void DifferentStrategyEquality()
    {
        NamingStrategy s1 = new SnakeCaseNamingStrategy();
        NamingStrategy s2 = new DefaultNamingStrategy();
        Assert.False(s1.Equals(s2));
        Assert.False(s1.GetHashCode() == s2.GetHashCode());
    }

    static void CheckInequality<T>(bool overrideSpecifiedNames, bool processDictionaryKeys)
        where T : NamingStrategy, new()
    {
        var s1 = new T
        {
            OverrideSpecifiedNames = false,
            ProcessDictionaryKeys = false,
        };

        var s2 = new T
        {
            OverrideSpecifiedNames = overrideSpecifiedNames,
            ProcessDictionaryKeys = processDictionaryKeys,
        };

        Assert.False(s1.Equals(s2));
        Assert.False(s1.GetHashCode() == s2.GetHashCode());
    }
}