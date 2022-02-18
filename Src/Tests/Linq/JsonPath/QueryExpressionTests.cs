#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Xunit;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Linq.JsonPath;

public class QueryExpressionTests : TestFixtureBase
{
    [Fact]
    public void AndExpressionTest()
    {
        var compositeExpression = new CompositeExpression(QueryOperator.And)
        {
            Expressions = new List<QueryExpression>
            {
                new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("FirstName") }, null),
                new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("LastName") }, null)
            }
        };

        var o1 = new JObject
        {
            { "Title", "Title!" },
            { "FirstName", "FirstName!" },
            { "LastName", "LastName!" }
        };

        Xunit.Assert.True(compositeExpression.IsMatch(o1, o1));

        var o2 = new JObject
        {
            { "Title", "Title!" },
            { "FirstName", "FirstName!" }
        };

        Xunit.Assert.False(compositeExpression.IsMatch(o2, o2));

        var o3 = new JObject
        {
            { "Title", "Title!" }
        };

        Xunit.Assert.False(compositeExpression.IsMatch(o3, o3));
    }

    [Fact]
    public void OrExpressionTest()
    {
        var compositeExpression = new CompositeExpression(QueryOperator.Or)
        {
            Expressions = new List<QueryExpression>
            {
                new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("FirstName") }, null),
                new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("LastName") }, null)
            }
        };

        var o1 = new JObject
        {
            { "Title", "Title!" },
            { "FirstName", "FirstName!" },
            { "LastName", "LastName!" }
        };

        Xunit.Assert.True(compositeExpression.IsMatch(o1, o1));

        var o2 = new JObject
        {
            { "Title", "Title!" },
            { "FirstName", "FirstName!" }
        };

        Xunit.Assert.True(compositeExpression.IsMatch(o2, o2));

        var o3 = new JObject
        {
            { "Title", "Title!" }
        };

        Xunit.Assert.False(compositeExpression.IsMatch(o3, o3));
    }
        
    [Fact]
    public void BooleanExpressionTest_RegexEqualsOperator()
    {
        var e1 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/foo.*d/"));

        Xunit.Assert.True(e1.IsMatch(null, new JArray("food")));
        Xunit.Assert.True(e1.IsMatch(null, new JArray("fooood and drink")));
        Xunit.Assert.False(e1.IsMatch(null, new JArray("FOOD")));
        Xunit.Assert.False(e1.IsMatch(null, new JArray("foo", "foog", "good")));

        var e2 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/Foo.*d/i"));

        Xunit.Assert.True(e2.IsMatch(null, new JArray("food")));
        Xunit.Assert.True(e2.IsMatch(null, new JArray("fooood and drink")));
        Xunit.Assert.True(e2.IsMatch(null, new JArray("FOOD")));
        Xunit.Assert.False(e2.IsMatch(null, new JArray("foo", "foog", "good")));
    }

    [Fact]
    public void BooleanExpressionTest_RegexEqualsOperator_CornerCase()
    {
        var e1 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/// comment/"));

        Xunit.Assert.True(e1.IsMatch(null, new JArray("// comment")));
        Xunit.Assert.False(e1.IsMatch(null, new JArray("//comment", "/ comment")));

        var e2 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/<tag>.*</tag>/i"));

        Xunit.Assert.True(e2.IsMatch(null, new JArray("<Tag>Test</Tag>", "")));
        Xunit.Assert.False(e2.IsMatch(null, new JArray("<tag>Test<tag>")));
    }

    [Fact]
    public void BooleanExpressionTest()
    {
        var e1 = new BooleanQueryExpression(QueryOperator.LessThan, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

        Xunit.Assert.True(e1.IsMatch(null, new JArray(1, 2, 3, 4, 5)));
        Xunit.Assert.True(e1.IsMatch(null, new JArray(2, 3, 4, 5)));
        Xunit.Assert.False(e1.IsMatch(null, new JArray(3, 4, 5)));
        Xunit.Assert.False(e1.IsMatch(null, new JArray(4, 5)));
        Xunit.Assert.False(e1.IsMatch(null, new JArray("11", 5)));

        var e2 = new BooleanQueryExpression(QueryOperator.LessThanOrEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

        Xunit.Assert.True(e2.IsMatch(null, new JArray(1, 2, 3, 4, 5)));
        Xunit.Assert.True(e2.IsMatch(null, new JArray(2, 3, 4, 5)));
        Xunit.Assert.True(e2.IsMatch(null, new JArray(3, 4, 5)));
        Xunit.Assert.False(e2.IsMatch(null, new JArray(4, 5)));
        Xunit.Assert.False(e1.IsMatch(null, new JArray("11", 5)));
    }

    [Fact]
    public void BooleanExpressionTest_GreaterThanOperator()
    {
        var e1 = new BooleanQueryExpression(QueryOperator.GreaterThan, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

        Xunit.Assert.True(e1.IsMatch(null, new JArray("2", "26")));
        Xunit.Assert.True(e1.IsMatch(null, new JArray(2, 26)));
        Xunit.Assert.False(e1.IsMatch(null, new JArray(2, 3)));
        Xunit.Assert.False(e1.IsMatch(null, new JArray("2", "3")));
    }

    [Fact]
    public void BooleanExpressionTest_GreaterThanOrEqualsOperator()
    {
        var e1 = new BooleanQueryExpression(QueryOperator.GreaterThanOrEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

        Xunit.Assert.True(e1.IsMatch(null, new JArray("2", "26")));
        Xunit.Assert.True(e1.IsMatch(null, new JArray(2, 26)));
        Xunit.Assert.True(e1.IsMatch(null, new JArray(2, 3)));
        Xunit.Assert.True(e1.IsMatch(null, new JArray("2", "3")));
        Xunit.Assert.False(e1.IsMatch(null, new JArray(2, 1)));
        Xunit.Assert.False(e1.IsMatch(null, new JArray("2", "1")));
    }
}