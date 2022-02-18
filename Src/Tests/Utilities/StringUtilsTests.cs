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
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Utilities;

public class StringUtilsTests : TestFixtureBase
{
    [Fact]
    public void ToCamelCaseTest()
    {
        Xunit.Assert.Equal("urlValue", StringUtils.ToCamelCase("URLValue"));
        Xunit.Assert.Equal("url", StringUtils.ToCamelCase("URL"));
        Xunit.Assert.Equal("id", StringUtils.ToCamelCase("ID"));
        Xunit.Assert.Equal("i", StringUtils.ToCamelCase("I"));
        Xunit.Assert.Equal("", StringUtils.ToCamelCase(""));
        Xunit.Assert.Equal(null, StringUtils.ToCamelCase(null));
        Xunit.Assert.Equal("person", StringUtils.ToCamelCase("Person"));
        Xunit.Assert.Equal("iPhone", StringUtils.ToCamelCase("iPhone"));
        Xunit.Assert.Equal("iPhone", StringUtils.ToCamelCase("IPhone"));
        Xunit.Assert.Equal("i Phone", StringUtils.ToCamelCase("I Phone"));
        Xunit.Assert.Equal("i  Phone", StringUtils.ToCamelCase("I  Phone"));
        Xunit.Assert.Equal(" IPhone", StringUtils.ToCamelCase(" IPhone"));
        Xunit.Assert.Equal(" IPhone ", StringUtils.ToCamelCase(" IPhone "));
        Xunit.Assert.Equal("isCIA", StringUtils.ToCamelCase("IsCIA"));
        Xunit.Assert.Equal("vmQ", StringUtils.ToCamelCase("VmQ"));
        Xunit.Assert.Equal("xml2Json", StringUtils.ToCamelCase("Xml2Json"));
        Xunit.Assert.Equal("snAkEcAsE", StringUtils.ToCamelCase("SnAkEcAsE"));
        Xunit.Assert.Equal("snA__kEcAsE", StringUtils.ToCamelCase("SnA__kEcAsE"));
        Xunit.Assert.Equal("snA__ kEcAsE", StringUtils.ToCamelCase("SnA__ kEcAsE"));
        Xunit.Assert.Equal("already_snake_case_ ", StringUtils.ToCamelCase("already_snake_case_ "));
        Xunit.Assert.Equal("isJSONProperty", StringUtils.ToCamelCase("IsJSONProperty"));
        Xunit.Assert.Equal("shoutinG_CASE", StringUtils.ToCamelCase("SHOUTING_CASE"));
        Xunit.Assert.Equal("9999-12-31T23:59:59.9999999Z", StringUtils.ToCamelCase("9999-12-31T23:59:59.9999999Z"));
        Xunit.Assert.Equal("hi!! This is text. Time to test.", StringUtils.ToCamelCase("Hi!! This is text. Time to test."));
        Xunit.Assert.Equal("building", StringUtils.ToCamelCase("BUILDING"));
        Xunit.Assert.Equal("building Property", StringUtils.ToCamelCase("BUILDING Property"));
        Xunit.Assert.Equal("building Property", StringUtils.ToCamelCase("Building Property"));
        Xunit.Assert.Equal("building PROPERTY", StringUtils.ToCamelCase("BUILDING PROPERTY"));
    }

    [Fact]
    public void ToSnakeCaseTest()
    {
        Xunit.Assert.Equal("url_value", StringUtils.ToSnakeCase("URLValue"));
        Xunit.Assert.Equal("url", StringUtils.ToSnakeCase("URL"));
        Xunit.Assert.Equal("id", StringUtils.ToSnakeCase("ID"));
        Xunit.Assert.Equal("i", StringUtils.ToSnakeCase("I"));
        Xunit.Assert.Equal("", StringUtils.ToSnakeCase(""));
        Xunit.Assert.Equal(null, StringUtils.ToSnakeCase(null));
        Xunit.Assert.Equal("person", StringUtils.ToSnakeCase("Person"));
        Xunit.Assert.Equal("i_phone", StringUtils.ToSnakeCase("iPhone"));
        Xunit.Assert.Equal("i_phone", StringUtils.ToSnakeCase("IPhone"));
        Xunit.Assert.Equal("i_phone", StringUtils.ToSnakeCase("I Phone"));
        Xunit.Assert.Equal("i_phone", StringUtils.ToSnakeCase("I  Phone"));
        Xunit.Assert.Equal("i_phone", StringUtils.ToSnakeCase(" IPhone"));
        Xunit.Assert.Equal("i_phone", StringUtils.ToSnakeCase(" IPhone "));
        Xunit.Assert.Equal("is_cia", StringUtils.ToSnakeCase("IsCIA"));
        Xunit.Assert.Equal("vm_q", StringUtils.ToSnakeCase("VmQ"));
        Xunit.Assert.Equal("xml2_json", StringUtils.ToSnakeCase("Xml2Json"));
        Xunit.Assert.Equal("sn_ak_ec_as_e", StringUtils.ToSnakeCase("SnAkEcAsE"));
        Xunit.Assert.Equal("sn_a__k_ec_as_e", StringUtils.ToSnakeCase("SnA__kEcAsE"));
        Xunit.Assert.Equal("sn_a__k_ec_as_e", StringUtils.ToSnakeCase("SnA__ kEcAsE"));
        Xunit.Assert.Equal("already_snake_case_", StringUtils.ToSnakeCase("already_snake_case_ "));
        Xunit.Assert.Equal("is_json_property", StringUtils.ToSnakeCase("IsJSONProperty"));
        Xunit.Assert.Equal("shouting_case", StringUtils.ToSnakeCase("SHOUTING_CASE"));
        Xunit.Assert.Equal("9999-12-31_t23:59:59.9999999_z", StringUtils.ToSnakeCase("9999-12-31T23:59:59.9999999Z"));
        Xunit.Assert.Equal("hi!!_this_is_text._time_to_test.", StringUtils.ToSnakeCase("Hi!! This is text. Time to test."));
    }

    [Fact]
    public void ToKebabCaseTest()
    {
        Xunit.Assert.Equal("url-value", StringUtils.ToKebabCase("URLValue"));
        Xunit.Assert.Equal("url", StringUtils.ToKebabCase("URL"));
        Xunit.Assert.Equal("id", StringUtils.ToKebabCase("ID"));
        Xunit.Assert.Equal("i", StringUtils.ToKebabCase("I"));
        Xunit.Assert.Equal("", StringUtils.ToKebabCase(""));
        Xunit.Assert.Equal(null, StringUtils.ToKebabCase(null));
        Xunit.Assert.Equal("person", StringUtils.ToKebabCase("Person"));
        Xunit.Assert.Equal("i-phone", StringUtils.ToKebabCase("iPhone"));
        Xunit.Assert.Equal("i-phone", StringUtils.ToKebabCase("IPhone"));
        Xunit.Assert.Equal("i-phone", StringUtils.ToKebabCase("I Phone"));
        Xunit.Assert.Equal("i-phone", StringUtils.ToKebabCase("I  Phone"));
        Xunit.Assert.Equal("i-phone", StringUtils.ToKebabCase(" IPhone"));
        Xunit.Assert.Equal("i-phone", StringUtils.ToKebabCase(" IPhone "));
        Xunit.Assert.Equal("is-cia", StringUtils.ToKebabCase("IsCIA"));
        Xunit.Assert.Equal("vm-q", StringUtils.ToKebabCase("VmQ"));
        Xunit.Assert.Equal("xml2-json", StringUtils.ToKebabCase("Xml2Json"));
        Xunit.Assert.Equal("ke-ba-bc-as-e", StringUtils.ToKebabCase("KeBaBcAsE"));
        Xunit.Assert.Equal("ke-b--a-bc-as-e", StringUtils.ToKebabCase("KeB--aBcAsE"));
        Xunit.Assert.Equal("ke-b--a-bc-as-e", StringUtils.ToKebabCase("KeB-- aBcAsE"));
        Xunit.Assert.Equal("already-kebab-case-", StringUtils.ToKebabCase("already-kebab-case- "));
        Xunit.Assert.Equal("is-json-property", StringUtils.ToKebabCase("IsJSONProperty"));
        Xunit.Assert.Equal("shouting-case", StringUtils.ToKebabCase("SHOUTING-CASE"));
        Xunit.Assert.Equal("9999-12-31-t23:59:59.9999999-z", StringUtils.ToKebabCase("9999-12-31T23:59:59.9999999Z"));
        Xunit.Assert.Equal("hi!!-this-is-text.-time-to-test.", StringUtils.ToKebabCase("Hi!! This is text. Time to test."));
    }
}