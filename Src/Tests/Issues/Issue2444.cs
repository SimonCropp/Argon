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

namespace Argon.Tests.Issues;

public class Issue2444
{
    [Fact]
    public void Test()
    {
        var namingStrategy = new SnakeCaseNamingStrategy();
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = namingStrategy
            }
        };

        var json = @"{""dict"":{""value1"":""a"",""text_value"":""b""}}";
        var c = JsonConvert.DeserializeObject<DataClass>(json, settings);

        Xunit.Assert.Equal(2, c.Dict.Count);
        Xunit.Assert.Equal("a", c.Dict[MyEnum.Value1]);
        Xunit.Assert.Equal("b", c.Dict[MyEnum.TextValue]);

        var json1 = @"{""dict"":{""Value1"":""a"",""TextValue"":""b""}}";
        var c1 = JsonConvert.DeserializeObject<DataClass>(json1, settings);

        Xunit.Assert.Equal(2, c1.Dict.Count);
        Xunit.Assert.Equal("a", c1.Dict[MyEnum.Value1]);
        Xunit.Assert.Equal("b", c1.Dict[MyEnum.TextValue]);

        // Non-dictionary values should still error
        ExceptionAssert.Throws<JsonSerializationException>(() =>
        {
            JsonConvert.DeserializeObject<List<MyEnum>>(@"[""text_value""]", settings);
        }, @"Error converting value ""text_value"" to type 'Argon.Tests.Issues.Issue2444+MyEnum'. Path '[0]', line 1, position 13.");
    }

    public enum MyEnum
    {
        Value1,
        TextValue
    }

    public class DataClass
    {
        public Dictionary<MyEnum, string> Dict { get; set; }
    }
}