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

namespace Argon.Tests.Utilities;

public class StringUtilsTests : TestFixtureBase
{
    [Fact]
    public void ToCamelCaseTest()
    {
        Assert.Equal("urlValue", StringUtils.ToCamelCase("URLValue"));
        Assert.Equal("url", StringUtils.ToCamelCase("URL"));
        Assert.Equal("id", StringUtils.ToCamelCase("ID"));
        Assert.Equal("i", StringUtils.ToCamelCase("I"));
        Assert.Equal("", StringUtils.ToCamelCase(""));
        Assert.Equal(null, StringUtils.ToCamelCase(null));
        Assert.Equal("person", StringUtils.ToCamelCase("Person"));
        Assert.Equal("iPhone", StringUtils.ToCamelCase("iPhone"));
        Assert.Equal("iPhone", StringUtils.ToCamelCase("IPhone"));
        Assert.Equal("i Phone", StringUtils.ToCamelCase("I Phone"));
        Assert.Equal("i  Phone", StringUtils.ToCamelCase("I  Phone"));
        Assert.Equal(" IPhone", StringUtils.ToCamelCase(" IPhone"));
        Assert.Equal(" IPhone ", StringUtils.ToCamelCase(" IPhone "));
        Assert.Equal("isCIA", StringUtils.ToCamelCase("IsCIA"));
        Assert.Equal("vmQ", StringUtils.ToCamelCase("VmQ"));
        Assert.Equal("xml2Json", StringUtils.ToCamelCase("Xml2Json"));
        Assert.Equal("snAkEcAsE", StringUtils.ToCamelCase("SnAkEcAsE"));
        Assert.Equal("snA__kEcAsE", StringUtils.ToCamelCase("SnA__kEcAsE"));
        Assert.Equal("snA__ kEcAsE", StringUtils.ToCamelCase("SnA__ kEcAsE"));
        Assert.Equal("already_snake_case_ ", StringUtils.ToCamelCase("already_snake_case_ "));
        Assert.Equal("isJSONProperty", StringUtils.ToCamelCase("IsJSONProperty"));
        Assert.Equal("shoutinG_CASE", StringUtils.ToCamelCase("SHOUTING_CASE"));
        Assert.Equal("9999-12-31T23:59:59.9999999Z", StringUtils.ToCamelCase("9999-12-31T23:59:59.9999999Z"));
        Assert.Equal("hi!! This is text. Time to test.", StringUtils.ToCamelCase("Hi!! This is text. Time to test."));
        Assert.Equal("building", StringUtils.ToCamelCase("BUILDING"));
        Assert.Equal("building Property", StringUtils.ToCamelCase("BUILDING Property"));
        Assert.Equal("building Property", StringUtils.ToCamelCase("Building Property"));
        Assert.Equal("building PROPERTY", StringUtils.ToCamelCase("BUILDING PROPERTY"));
    }

    [Fact]
    public void ToSnakeCaseTest()
    {
        Assert.Equal("url_value", StringUtils.ToSnakeCase("URLValue"));
        Assert.Equal("url", StringUtils.ToSnakeCase("URL"));
        Assert.Equal("id", StringUtils.ToSnakeCase("ID"));
        Assert.Equal("i", StringUtils.ToSnakeCase("I"));
        Assert.Equal("", StringUtils.ToSnakeCase(""));
        Assert.Equal(null, StringUtils.ToSnakeCase(null));
        Assert.Equal("person", StringUtils.ToSnakeCase("Person"));
        Assert.Equal("i_phone", StringUtils.ToSnakeCase("iPhone"));
        Assert.Equal("i_phone", StringUtils.ToSnakeCase("IPhone"));
        Assert.Equal("i_phone", StringUtils.ToSnakeCase("I Phone"));
        Assert.Equal("i_phone", StringUtils.ToSnakeCase("I  Phone"));
        Assert.Equal("i_phone", StringUtils.ToSnakeCase(" IPhone"));
        Assert.Equal("i_phone", StringUtils.ToSnakeCase(" IPhone "));
        Assert.Equal("is_cia", StringUtils.ToSnakeCase("IsCIA"));
        Assert.Equal("vm_q", StringUtils.ToSnakeCase("VmQ"));
        Assert.Equal("xml2_json", StringUtils.ToSnakeCase("Xml2Json"));
        Assert.Equal("sn_ak_ec_as_e", StringUtils.ToSnakeCase("SnAkEcAsE"));
        Assert.Equal("sn_a__k_ec_as_e", StringUtils.ToSnakeCase("SnA__kEcAsE"));
        Assert.Equal("sn_a__k_ec_as_e", StringUtils.ToSnakeCase("SnA__ kEcAsE"));
        Assert.Equal("already_snake_case_", StringUtils.ToSnakeCase("already_snake_case_ "));
        Assert.Equal("is_json_property", StringUtils.ToSnakeCase("IsJSONProperty"));
        Assert.Equal("shouting_case", StringUtils.ToSnakeCase("SHOUTING_CASE"));
        Assert.Equal("9999-12-31_t23:59:59.9999999_z", StringUtils.ToSnakeCase("9999-12-31T23:59:59.9999999Z"));
        Assert.Equal("hi!!_this_is_text._time_to_test.", StringUtils.ToSnakeCase("Hi!! This is text. Time to test."));
    }

    [Fact]
    public void ToKebabCaseTest()
    {
        Assert.Equal("url-value", StringUtils.ToKebabCase("URLValue"));
        Assert.Equal("url", StringUtils.ToKebabCase("URL"));
        Assert.Equal("id", StringUtils.ToKebabCase("ID"));
        Assert.Equal("i", StringUtils.ToKebabCase("I"));
        Assert.Equal("", StringUtils.ToKebabCase(""));
        Assert.Equal(null, StringUtils.ToKebabCase(null));
        Assert.Equal("person", StringUtils.ToKebabCase("Person"));
        Assert.Equal("i-phone", StringUtils.ToKebabCase("iPhone"));
        Assert.Equal("i-phone", StringUtils.ToKebabCase("IPhone"));
        Assert.Equal("i-phone", StringUtils.ToKebabCase("I Phone"));
        Assert.Equal("i-phone", StringUtils.ToKebabCase("I  Phone"));
        Assert.Equal("i-phone", StringUtils.ToKebabCase(" IPhone"));
        Assert.Equal("i-phone", StringUtils.ToKebabCase(" IPhone "));
        Assert.Equal("is-cia", StringUtils.ToKebabCase("IsCIA"));
        Assert.Equal("vm-q", StringUtils.ToKebabCase("VmQ"));
        Assert.Equal("xml2-json", StringUtils.ToKebabCase("Xml2Json"));
        Assert.Equal("ke-ba-bc-as-e", StringUtils.ToKebabCase("KeBaBcAsE"));
        Assert.Equal("ke-b--a-bc-as-e", StringUtils.ToKebabCase("KeB--aBcAsE"));
        Assert.Equal("ke-b--a-bc-as-e", StringUtils.ToKebabCase("KeB-- aBcAsE"));
        Assert.Equal("already-kebab-case-", StringUtils.ToKebabCase("already-kebab-case- "));
        Assert.Equal("is-json-property", StringUtils.ToKebabCase("IsJSONProperty"));
        Assert.Equal("shouting-case", StringUtils.ToKebabCase("SHOUTING-CASE"));
        Assert.Equal("9999-12-31-t23:59:59.9999999-z", StringUtils.ToKebabCase("9999-12-31T23:59:59.9999999Z"));
        Assert.Equal("hi!!-this-is-text.-time-to-test.", StringUtils.ToKebabCase("Hi!! This is text. Time to test."));
    }
}