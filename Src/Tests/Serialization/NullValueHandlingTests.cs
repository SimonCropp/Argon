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

using Argon.Tests.TestObjects;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Serialization;

[TestFixture]
public class NullValueHandlingTests : TestFixtureBase
{
    const string MovieNullValueHandlingIncludeExpectedResult = @"{
  ""Name"": ""Bad Boys III"",
  ""Description"": ""It's no Bad Boys"",
  ""Classification"": null,
  ""Studio"": null,
  ""ReleaseDate"": null,
  ""ReleaseCountries"": null
}";

    const string MovieNullValueHandlingIgnoreExpectedResult = @"{
  ""Name"": ""Bad Boys III"",
  ""Description"": ""It's no Bad Boys""
}";

    [Fact]
    public void DeserializeNullIntoDateTime()
    {
        var c = JsonConvert.DeserializeObject<DateTimeTestClass>(@"{DateTimeField:null}", new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        Assert.AreEqual(c.DateTimeField, default(DateTime));
    }

    [Fact]
    public void DeserializeEmptyStringIntoDateTimeWithEmptyStringDefaultValue()
    {
        var c = JsonConvert.DeserializeObject<DateTimeTestClass>(@"{DateTimeField:""""}", new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        Assert.AreEqual(c.DateTimeField, default(DateTime));
    }

    [Fact]
    public void NullValueHandlingSerialization()
    {
        var s1 = new Store();

        var jsonSerializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var sw = new StringWriter();
        jsonSerializer.Serialize(sw, s1);

        //JsonConvert.ConvertDateTimeToJavaScriptTicks(s1.Establised.DateTime)

        Assert.AreEqual(@"{""Color"":4,""Establised"":""2010-01-22T01:01:01Z"",""Width"":1.1,""Employees"":999,""RoomsPerFloor"":[1,2,3,4,5,6,7,8,9],""Open"":false,""Symbol"":""@"",""Mottos"":[""Hello World"",""öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~"",null,"" ""],""Cost"":100980.1,""Escape"":""\r\n\t\f\b?{\\r\\n\""'"",""product"":[{""Name"":""Rocket"",""ExpiryDate"":""2000-02-02T23:01:30Z"",""Price"":0.0},{""Name"":""Alien"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0}]}", sw.GetStringBuilder().ToString());

        var s2 = (Store)jsonSerializer.Deserialize(new JsonTextReader(new StringReader("{}")), typeof(Store));
        Assert.AreEqual("\r\n\t\f\b?{\\r\\n\"\'", s2.Escape);

        var s3 = (Store)jsonSerializer.Deserialize(new JsonTextReader(new StringReader(@"{""Escape"":null}")), typeof(Store));
        Assert.AreEqual("\r\n\t\f\b?{\\r\\n\"\'", s3.Escape);

        var s4 = (Store)jsonSerializer.Deserialize(new JsonTextReader(new StringReader(@"{""Color"":2,""Establised"":""\/Date(1264071600000+1300)\/"",""Width"":1.1,""Employees"":999,""RoomsPerFloor"":[1,2,3,4,5,6,7,8,9],""Open"":false,""Symbol"":""@"",""Mottos"":[""Hello World"",""öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~"",null,"" ""],""Cost"":100980.1,""Escape"":""\r\n\t\f\b?{\\r\\n\""'"",""product"":[{""Name"":""Rocket"",""ExpiryDate"":""\/Date(949485690000+1300)\/"",""Price"":0},{""Name"":""Alien"",""ExpiryDate"":""\/Date(946638000000)\/"",""Price"":0.0}]}")), typeof(Store));
        Assert.AreEqual(s1.Establised, s3.Establised);
    }

    [Fact]
    public void NullValueHandlingBlogPost()
    {
        var movie = new Movie
        {
            Name = "Bad Boys III",
            Description = "It's no Bad Boys"
        };

        var included = JsonConvert.SerializeObject(movie,
            Formatting.Indented,
            new JsonSerializerSettings { });

        // {
        //   "Name": "Bad Boys III",
        //   "Description": "It's no Bad Boys",
        //   "Classification": null,
        //   "Studio": null,
        //   "ReleaseDate": null,
        //   "ReleaseCountries": null
        // }

        var ignored = JsonConvert.SerializeObject(movie,
            Formatting.Indented,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        // {
        //   "Name": "Bad Boys III",
        //   "Description": "It's no Bad Boys"
        // }

        StringAssert.AreEqual(MovieNullValueHandlingIncludeExpectedResult, included);

        StringAssert.AreEqual(MovieNullValueHandlingIgnoreExpectedResult, ignored);
    }

    [Fact]
    public void JsonObjectNullValueHandlingIgnore()
    {
        var movie = new MovieWithJsonObjectNullValueHandlingIgnore
        {
            Name = "Bad Boys III",
            Description = "It's no Bad Boys"
        };
            
        var ignored = JsonConvert.SerializeObject(movie,
            Formatting.Indented,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });

        // {
        //   "Name": "Bad Boys III",
        //   "Description": "It's no Bad Boys"
        // }

        StringAssert.AreEqual(MovieNullValueHandlingIgnoreExpectedResult, ignored);
    }

    [Fact]
    public void JsonObjectNullValueHandlingInclude()
    {
        var movie = new MovieWithJsonObjectNullValueHandlingInclude
        {
            Name = "Bad Boys III",
            Description = "It's no Bad Boys"
        };

        var included = JsonConvert.SerializeObject(movie,
            Formatting.Indented,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        // {
        //   "Name": "Bad Boys III",
        //   "Description": "It's no Bad Boys",
        //   "Classification": null,
        //   "Studio": null,
        //   "ReleaseDate": null,
        //   "ReleaseCountries": null
        // }

        StringAssert.AreEqual(MovieNullValueHandlingIncludeExpectedResult, included);
    }
}