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
        CheckInequality<CamelCaseNamingStrategy>(false, false, true);
        CheckInequality<CamelCaseNamingStrategy>(false, true, false);
        CheckInequality<CamelCaseNamingStrategy>(true, false, false);
        CheckInequality<CamelCaseNamingStrategy>(false, true, true);
        CheckInequality<CamelCaseNamingStrategy>(true, true, false);
        CheckInequality<CamelCaseNamingStrategy>(true, true, true);
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
        CheckInequality<DefaultNamingStrategy>(false, false, true);
        CheckInequality<DefaultNamingStrategy>(false, true, false);
        CheckInequality<DefaultNamingStrategy>(true, false, false);
        CheckInequality<DefaultNamingStrategy>(false, true, true);
        CheckInequality<DefaultNamingStrategy>(true, true, false);
        CheckInequality<DefaultNamingStrategy>(true, true, true);
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
        CheckInequality<SnakeCaseNamingStrategy>(false, false, true);
        CheckInequality<SnakeCaseNamingStrategy>(false, true, false);
        CheckInequality<SnakeCaseNamingStrategy>(true, false, false);
        CheckInequality<SnakeCaseNamingStrategy>(false, true, true);
        CheckInequality<SnakeCaseNamingStrategy>(true, true, false);
        CheckInequality<SnakeCaseNamingStrategy>(true, true, true);
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
        CheckInequality<KebabCaseNamingStrategy>(false, false, true);
        CheckInequality<KebabCaseNamingStrategy>(false, true, false);
        CheckInequality<KebabCaseNamingStrategy>(true, false, false);
        CheckInequality<KebabCaseNamingStrategy>(false, true, true);
        CheckInequality<KebabCaseNamingStrategy>(true, true, false);
        CheckInequality<KebabCaseNamingStrategy>(true, true, true);
    }

    [Fact]
    public void DifferentStrategyEquality()
    {
        NamingStrategy s1 = new SnakeCaseNamingStrategy();
        NamingStrategy s2 = new DefaultNamingStrategy();
        Assert.False(s1.Equals(s2));
        Assert.False(s1.GetHashCode() == s2.GetHashCode());
    }

    static void CheckInequality<T>(bool overrideSpecifiedNames, bool processDictionaryKeys, bool processExtensionDataNames)
        where T : NamingStrategy, new()
    {
        var s1 = new T
        {
            OverrideSpecifiedNames = false,
            ProcessDictionaryKeys = false,
            ProcessExtensionDataNames = false
        };

        var s2 = new T
        {
            OverrideSpecifiedNames = overrideSpecifiedNames,
            ProcessDictionaryKeys = processDictionaryKeys,
            ProcessExtensionDataNames = processExtensionDataNames
        };

        Assert.False(s1.Equals(s2));
        Assert.False(s1.GetHashCode() == s2.GetHashCode());
    }
}