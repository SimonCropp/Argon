// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class QueryExpressionTests : TestFixtureBase
{
    [Fact]
    public void AndExpressionTest()
    {
        var compositeExpression = new CompositeExpression(QueryOperator.And)
        {
            Expressions = new()
            {
                new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> {new FieldFilter("FirstName")}, null),
                new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> {new FieldFilter("LastName")}, null)
            }
        };

        var o1 = new JObject
        {
            {"Title", "Title!"},
            {"FirstName", "FirstName!"},
            {"LastName", "LastName!"}
        };

        Assert.True(compositeExpression.IsMatch(o1, o1));

        var o2 = new JObject
        {
            {"Title", "Title!"},
            {"FirstName", "FirstName!"}
        };

        Assert.False(compositeExpression.IsMatch(o2, o2));

        var o3 = new JObject
        {
            {"Title", "Title!"}
        };

        Assert.False(compositeExpression.IsMatch(o3, o3));
    }

    [Fact]
    public void OrExpressionTest()
    {
        var compositeExpression = new CompositeExpression(QueryOperator.Or)
        {
            Expressions =
            [
                new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> {new FieldFilter("FirstName")}, null),
                new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> {new FieldFilter("LastName")}, null)
            ]
        };

        var o1 = new JObject
        {
            {"Title", "Title!"},
            {"FirstName", "FirstName!"},
            {"LastName", "LastName!"}
        };

        Assert.True(compositeExpression.IsMatch(o1, o1));

        var o2 = new JObject
        {
            {"Title", "Title!"},
            {"FirstName", "FirstName!"}
        };

        Assert.True(compositeExpression.IsMatch(o2, o2));

        var o3 = new JObject
        {
            {"Title", "Title!"}
        };

        Assert.False(compositeExpression.IsMatch(o3, o3));
    }

    [Fact]
    public void BooleanExpressionTest_RegexEqualsOperator()
    {
        var e1 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> {new ArrayIndexFilter()}, new JValue("/foo.*d/"));

        Assert.True(e1.IsMatch(null, new JArray("food")));
        Assert.True(e1.IsMatch(null, new JArray("fooood and drink")));
        Assert.False(e1.IsMatch(null, new JArray("FOOD")));
        Assert.False(e1.IsMatch(null, new JArray("foo", "foog", "good")));

        var e2 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> {new ArrayIndexFilter()}, new JValue("/Foo.*d/i"));

        Assert.True(e2.IsMatch(null, new JArray("food")));
        Assert.True(e2.IsMatch(null, new JArray("fooood and drink")));
        Assert.True(e2.IsMatch(null, new JArray("FOOD")));
        Assert.False(e2.IsMatch(null, new JArray("foo", "foog", "good")));
    }

    [Fact]
    public void BooleanExpressionTest_RegexEqualsOperator_CornerCase()
    {
        var e1 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> {new ArrayIndexFilter()}, new JValue("/// comment/"));

        Assert.True(e1.IsMatch(null, new JArray("// comment")));
        Assert.False(e1.IsMatch(null, new JArray("//comment", "/ comment")));

        var e2 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> {new ArrayIndexFilter()}, new JValue("/<tag>.*</tag>/i"));

        Assert.True(e2.IsMatch(null, new JArray("<Tag>Test</Tag>", "")));
        Assert.False(e2.IsMatch(null, new JArray("<tag>Test<tag>")));
    }

    [Fact]
    public void BooleanExpressionTest()
    {
        var e1 = new BooleanQueryExpression(QueryOperator.LessThan, new List<PathFilter> {new ArrayIndexFilter()}, new JValue(3));

        Assert.True(e1.IsMatch(null, new JArray(1, 2, 3, 4, 5)));
        Assert.True(e1.IsMatch(null, new JArray(2, 3, 4, 5)));
        Assert.False(e1.IsMatch(null, new JArray(3, 4, 5)));
        Assert.False(e1.IsMatch(null, new JArray(4, 5)));
        Assert.False(e1.IsMatch(null, new JArray("11", 5)));

        var e2 = new BooleanQueryExpression(QueryOperator.LessThanOrEquals, new List<PathFilter> {new ArrayIndexFilter()}, new JValue(3));

        Assert.True(e2.IsMatch(null, new JArray(1, 2, 3, 4, 5)));
        Assert.True(e2.IsMatch(null, new JArray(2, 3, 4, 5)));
        Assert.True(e2.IsMatch(null, new JArray(3, 4, 5)));
        Assert.False(e2.IsMatch(null, new JArray(4, 5)));
        Assert.False(e1.IsMatch(null, new JArray("11", 5)));
    }

    [Fact]
    public void BooleanExpressionTest_GreaterThanOperator()
    {
        var e1 = new BooleanQueryExpression(QueryOperator.GreaterThan, new List<PathFilter> {new ArrayIndexFilter()}, new JValue(3));

        Assert.True(e1.IsMatch(null, new JArray("2", "26")));
        Assert.True(e1.IsMatch(null, new JArray(2, 26)));
        Assert.False(e1.IsMatch(null, new JArray(2, 3)));
        Assert.False(e1.IsMatch(null, new JArray("2", "3")));
    }

    [Fact]
    public void BooleanExpressionTest_GreaterThanOrEqualsOperator()
    {
        var e1 = new BooleanQueryExpression(QueryOperator.GreaterThanOrEquals, new List<PathFilter> {new ArrayIndexFilter()}, new JValue(3));

        Assert.True(e1.IsMatch(null, new JArray("2", "26")));
        Assert.True(e1.IsMatch(null, new JArray(2, 26)));
        Assert.True(e1.IsMatch(null, new JArray(2, 3)));
        Assert.True(e1.IsMatch(null, new JArray("2", "3")));
        Assert.False(e1.IsMatch(null, new JArray(2, 1)));
        Assert.False(e1.IsMatch(null, new JArray("2", "1")));
    }
}