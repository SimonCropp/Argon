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

using System;
using System.Collections.Generic;
using Argon.Linq.JsonPath;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Argon.Linq;
using System.Linq;

namespace Argon.Tests.Linq.JsonPath
{
    [TestFixture]
    public class JPathParseTests : TestFixtureBase
    {
        [Fact]
        public void BooleanQuery_TwoValues()
        {
            JPath path = new JPath("[?(1 > 2)]");
            Assert.AreEqual(1, path.Filters.Count);
            BooleanQueryExpression booleanExpression = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(1, (int)(JValue)booleanExpression.Left);
            Assert.AreEqual(2, (int)(JValue)booleanExpression.Right);
            Assert.AreEqual(QueryOperator.GreaterThan, booleanExpression.Operator);
        }

        [Fact]
        public void BooleanQuery_TwoPaths()
        {
            JPath path = new JPath("[?(@.price > @.max_price)]");
            Assert.AreEqual(1, path.Filters.Count);
            BooleanQueryExpression booleanExpression = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> leftPaths = (List<PathFilter>)booleanExpression.Left;
            List<PathFilter> rightPaths = (List<PathFilter>)booleanExpression.Right;

            Assert.AreEqual("price", ((FieldFilter)leftPaths[0]).Name);
            Assert.AreEqual("max_price", ((FieldFilter)rightPaths[0]).Name);
            Assert.AreEqual(QueryOperator.GreaterThan, booleanExpression.Operator);
        }

        [Fact]
        public void SingleProperty()
        {
            JPath path = new JPath("Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedProperty()
        {
            JPath path = new JPath("['Blah']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedPropertyWithWhitespace()
        {
            JPath path = new JPath("[  'Blah'  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedPropertyWithDots()
        {
            JPath path = new JPath("['Blah.Ha']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah.Ha", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedPropertyWithBrackets()
        {
            JPath path = new JPath("['[*]']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("[*]", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SinglePropertyWithRoot()
        {
            JPath path = new JPath("$.Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SinglePropertyWithRootWithStartAndEndWhitespace()
        {
            JPath path = new JPath(" $.Blah ");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void RootWithBadWhitespace()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JPath("$ .Blah"); }, @"Unexpected character while parsing path:  ");
        }

        [Fact]
        public void NoFieldNameAfterDot()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JPath("$.Blah."); }, @"Unexpected end while parsing path.");
        }

        [Fact]
        public void RootWithBadWhitespace2()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JPath("$. Blah"); }, @"Unexpected character while parsing path:  ");
        }

        [Fact]
        public void WildcardPropertyWithRoot()
        {
            JPath path = new JPath("$.*");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void WildcardArrayWithRoot()
        {
            JPath path = new JPath("$.[*]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void RootArrayNoDot()
        {
            JPath path = new JPath("$[1]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void WildcardArray()
        {
            JPath path = new JPath("[*]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void WildcardArrayWithProperty()
        {
            JPath path = new JPath("[ * ].derp");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.AreEqual("derp", ((FieldFilter)path.Filters[1]).Name);
        }

        [Fact]
        public void QuotedWildcardPropertyWithRoot()
        {
            JPath path = new JPath("$.['*']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("*", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleScanWithRoot()
        {
            JPath path = new JPath("$..Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((ScanFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void QueryTrue()
        {
            JPath path = new JPath("$.elements[?(true)]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("elements", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, ((QueryFilter)path.Filters[1]).Expression.Operator);
        }

        [Fact]
        public void ScanQuery()
        {
            JPath path = new JPath("$.elements..[?(@.id=='AAA')]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("elements", ((FieldFilter)path.Filters[0]).Name);

            BooleanQueryExpression expression = (BooleanQueryExpression)((QueryScanFilter) path.Filters[1]).Expression;

            List<PathFilter> paths = (List<PathFilter>)expression.Left;

            Assert.IsInstanceOf(typeof(FieldFilter), paths[0]);
        }

        [Fact]
        public void WildcardScanWithRoot()
        {
            JPath path = new JPath("$..*");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ScanFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void WildcardScanWithRootWithWhitespace()
        {
            JPath path = new JPath("$..* ");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ScanFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void TwoProperties()
        {
            JPath path = new JPath("Blah.Two");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual("Two", ((FieldFilter)path.Filters[1]).Name);
        }

        [Fact]
        public void OnePropertyOneScan()
        {
            JPath path = new JPath("Blah..Two");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual("Two", ((ScanFilter)path.Filters[1]).Name);
        }

        [Fact]
        public void SinglePropertyAndIndexer()
        {
            JPath path = new JPath("Blah[0]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
        }

        [Fact]
        public void SinglePropertyAndExistsQuery()
        {
            JPath path = new JPath("Blah[ ?( @..name ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Exists, expressions.Operator);
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("name", ((ScanFilter)paths[0]).Name);
        }

        [Fact]
        public void SinglePropertyAndFilterWithWhitespace()
        {
            JPath path = new JPath("Blah[ ?( @.name=='hi' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("hi", (string)(JToken)expressions.Right);
        }

        [Fact]
        public void SinglePropertyAndFilterWithEscapeQuote()
        {
            JPath path = new JPath(@"Blah[ ?( @.name=='h\'i' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("h'i", (string)(JToken)expressions.Right);
        }

        [Fact]
        public void SinglePropertyAndFilterWithDoubleEscape()
        {
            JPath path = new JPath(@"Blah[ ?( @.name=='h\\i' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("h\\i", (string)(JToken)expressions.Right);
        }

        [Fact]
        public void SinglePropertyAndFilterWithRegexAndOptions()
        {
            JPath path = new JPath("Blah[ ?( @.name=~/hi/i ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.RegexEquals, expressions.Operator);
            Assert.AreEqual("/hi/i", (string)(JToken)expressions.Right);
        }

        [Fact]
        public void SinglePropertyAndFilterWithRegex()
        {
            JPath path = new JPath("Blah[?(@.title =~ /^.*Sword.*$/)]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.RegexEquals, expressions.Operator);
            Assert.AreEqual("/^.*Sword.*$/", (string)(JToken)expressions.Right);
        }

        [Fact]
        public void SinglePropertyAndFilterWithEscapedRegex()
        {
            JPath path = new JPath(@"Blah[?(@.title =~ /[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g)]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.RegexEquals, expressions.Operator);
            Assert.AreEqual(@"/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g", (string)(JToken)expressions.Right);
        }

        [Fact]
        public void SinglePropertyAndFilterWithOpenRegex()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JPath(@"Blah[?(@.title =~ /[\"); }, "Path ended with an open regex.");
        }

        [Fact]
        public void SinglePropertyAndFilterWithUnknownEscape()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JPath(@"Blah[ ?( @.name=='h\i' ) ]"); }, @"Unknown escape character: \i");
        }

        [Fact]
        public void SinglePropertyAndFilterWithFalse()
        {
            JPath path = new JPath("Blah[ ?( @.name==false ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(false, (bool)(JToken)expressions.Right);
        }

        [Fact]
        public void SinglePropertyAndFilterWithTrue()
        {
            JPath path = new JPath("Blah[ ?( @.name==true ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(true, (bool)(JToken)expressions.Right);
        }

        [Fact]
        public void SinglePropertyAndFilterWithNull()
        {
            JPath path = new JPath("Blah[ ?( @.name==null ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(null, ((JValue)expressions.Right).Value);
        }

        [Fact]
        public void FilterWithScan()
        {
            JPath path = new JPath("[?(@..name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.AreEqual("name", ((ScanFilter)paths[0]).Name);
        }

        [Fact]
        public void FilterWithNotEquals()
        {
            JPath path = new JPath("[?(@.name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.NotEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithNotEquals2()
        {
            JPath path = new JPath("[?(@.name!=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.NotEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithLessThan()
        {
            JPath path = new JPath("[?(@.name<null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.LessThan, expressions.Operator);
        }

        [Fact]
        public void FilterWithLessThanOrEquals()
        {
            JPath path = new JPath("[?(@.name<=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.LessThanOrEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithGreaterThan()
        {
            JPath path = new JPath("[?(@.name>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.GreaterThan, expressions.Operator);
        }

        [Fact]
        public void FilterWithGreaterThanOrEquals()
        {
            JPath path = new JPath("[?(@.name>=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.GreaterThanOrEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithInteger()
        {
            JPath path = new JPath("[?(@.name>=12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(12, (int)(JToken)expressions.Right);
        }

        [Fact]
        public void FilterWithNegativeInteger()
        {
            JPath path = new JPath("[?(@.name>=-12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(-12, (int)(JToken)expressions.Right);
        }

        [Fact]
        public void FilterWithFloat()
        {
            JPath path = new JPath("[?(@.name>=12.1)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(12.1d, (double)(JToken)expressions.Right);
        }

        [Fact]
        public void FilterExistWithAnd()
        {
            JPath path = new JPath("[?(@.name&&@.title)]");
            CompositeExpression expressions = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.And, expressions.Operator);
            Assert.AreEqual(2, expressions.Expressions.Count);

            var first = (BooleanQueryExpression)expressions.Expressions[0];
            var firstPaths = (List<PathFilter>)first.Left;
            Assert.AreEqual("name", ((FieldFilter)firstPaths[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, first.Operator);

            var second = (BooleanQueryExpression)expressions.Expressions[1];
            var secondPaths = (List<PathFilter>)second.Left;
            Assert.AreEqual("title", ((FieldFilter)secondPaths[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, second.Operator);
        }

        [Fact]
        public void FilterExistWithAndOr()
        {
            JPath path = new JPath("[?(@.name&&@.title||@.pie)]");
            CompositeExpression andExpression = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.And, andExpression.Operator);
            Assert.AreEqual(2, andExpression.Expressions.Count);

            var first = (BooleanQueryExpression)andExpression.Expressions[0];
            var firstPaths = (List<PathFilter>)first.Left;
            Assert.AreEqual("name", ((FieldFilter)firstPaths[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, first.Operator);

            CompositeExpression orExpression = (CompositeExpression)andExpression.Expressions[1];
            Assert.AreEqual(2, orExpression.Expressions.Count);

            var orFirst = (BooleanQueryExpression)orExpression.Expressions[0];
            var orFirstPaths = (List<PathFilter>)orFirst.Left;
            Assert.AreEqual("title", ((FieldFilter)orFirstPaths[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, orFirst.Operator);

            var orSecond = (BooleanQueryExpression)orExpression.Expressions[1];
            var orSecondPaths = (List<PathFilter>)orSecond.Left;
            Assert.AreEqual("pie", ((FieldFilter)orSecondPaths[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, orSecond.Operator);
        }

        [Fact]
        public void FilterWithRoot()
        {
            JPath path = new JPath("[?($.name>=12.1)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.AreEqual(2, paths.Count);
            Assert.IsInstanceOf(typeof(RootFilter), paths[0]);
            Assert.IsInstanceOf(typeof(FieldFilter), paths[1]);
        }

        [Fact]
        public void BadOr1()
        {
            ExceptionAssert.Throws<JsonException>(() => new JPath("[?(@.name||)]"), "Unexpected character while parsing path query: )");
        }

        [Fact]
        public void BaddOr2()
        {
            ExceptionAssert.Throws<JsonException>(() => new JPath("[?(@.name|)]"), "Unexpected character while parsing path query: |");
        }

        [Fact]
        public void BaddOr3()
        {
            ExceptionAssert.Throws<JsonException>(() => new JPath("[?(@.name|"), "Unexpected character while parsing path query: |");
        }

        [Fact]
        public void BaddOr4()
        {
            ExceptionAssert.Throws<JsonException>(() => new JPath("[?(@.name||"), "Path ended with open query.");
        }

        [Fact]
        public void NoAtAfterOr()
        {
            ExceptionAssert.Throws<JsonException>(() => new JPath("[?(@.name||s"), "Unexpected character while parsing path query: s");
        }

        [Fact]
        public void NoPathAfterAt()
        {
            ExceptionAssert.Throws<JsonException>(() => new JPath("[?(@.name||@"), @"Path ended with open query.");
        }

        [Fact]
        public void NoPathAfterDot()
        {
            ExceptionAssert.Throws<JsonException>(() => new JPath("[?(@.name||@."), @"Unexpected end while parsing path.");
        }

        [Fact]
        public void NoPathAfterDot2()
        {
            ExceptionAssert.Throws<JsonException>(() => new JPath("[?(@.name||@.)]"), @"Unexpected end while parsing path.");
        }

        [Fact]
        public void FilterWithFloatExp()
        {
            JPath path = new JPath("[?(@.name>=5.56789e+0)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(5.56789e+0, (double)(JToken)expressions.Right);
        }

        [Fact]
        public void MultiplePropertiesAndIndexers()
        {
            JPath path = new JPath("Blah[0]..Two.Three[1].Four");
            Assert.AreEqual(6, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
            Assert.AreEqual("Two", ((ScanFilter)path.Filters[2]).Name);
            Assert.AreEqual("Three", ((FieldFilter)path.Filters[3]).Name);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[4]).Index);
            Assert.AreEqual("Four", ((FieldFilter)path.Filters[5]).Name);
        }

        [Fact]
        public void BadCharactersInIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JPath("Blah[[0]].Two.Three[1].Four"); }, @"Unexpected character while parsing path indexer: [");
        }

        [Fact]
        public void UnclosedIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JPath("Blah[0"); }, @"Path ended with open indexer.");
        }

        [Fact]
        public void IndexerOnly()
        {
            JPath path = new JPath("[111119990]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void IndexerOnlyWithWhitespace()
        {
            JPath path = new JPath("[  10  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(10, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void MultipleIndexes()
        {
            JPath path = new JPath("[111119990,3]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.AreEqual(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.AreEqual(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [Fact]
        public void MultipleIndexesWithWhitespace()
        {
            JPath path = new JPath("[   111119990  ,   3   ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.AreEqual(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.AreEqual(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [Fact]
        public void MultipleQuotedIndexes()
        {
            JPath path = new JPath("['111119990','3']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.AreEqual("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.AreEqual("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [Fact]
        public void MultipleQuotedIndexesWithWhitespace()
        {
            JPath path = new JPath("[ '111119990' , '3' ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.AreEqual("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.AreEqual("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [Fact]
        public void SlicingIndexAll()
        {
            JPath path = new JPath("[111119990:3:2]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndex()
        {
            JPath path = new JPath("[111119990:3]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexNegative()
        {
            JPath path = new JPath("[-111119990:-3:-2]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexEmptyStop()
        {
            JPath path = new JPath("[  -3  :  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexEmptyStart()
        {
            JPath path = new JPath("[ : 1 : ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(1, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexWhitespace()
        {
            JPath path = new JPath("[  -111119990  :  -3  :  -2  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void EmptyIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JPath("[]"); }, "Array index expected.");
        }

        [Fact]
        public void IndexerCloseInProperty()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JPath("]"); }, "Unexpected character while parsing path: ]");
        }

        [Fact]
        public void AdjacentIndexers()
        {
            JPath path = new JPath("[1][0][0][" + int.MaxValue + "]");
            Assert.AreEqual(4, path.Filters.Count);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[2]).Index);
            Assert.AreEqual(int.MaxValue, ((ArrayIndexFilter)path.Filters[3]).Index);
        }

        [Fact]
        public void MissingDotAfterIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JPath("[1]Blah"); }, "Unexpected character following indexer: B");
        }

        [Fact]
        public void PropertyFollowingEscapedPropertyName()
        {
            JPath path = new JPath("frameworks.NET5_0_OR_GREATER.dependencies.['System.Xml.ReaderWriter'].source");
            Assert.AreEqual(5, path.Filters.Count);

            Assert.AreEqual("frameworks", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual("NET5_0_OR_GREATER", ((FieldFilter)path.Filters[1]).Name);
            Assert.AreEqual("dependencies", ((FieldFilter)path.Filters[2]).Name);
            Assert.AreEqual("System.Xml.ReaderWriter", ((FieldFilter)path.Filters[3]).Name);
            Assert.AreEqual("source", ((FieldFilter)path.Filters[4]).Name);
        }
    }
}