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

namespace Argon.Tests.Converters;

public class VersionClass
{
    public VersionClass(string version1, string version2)
    {
        StringProperty1 = "StringProperty1";
        Version1 = new Version(version1);
        Version2 = new Version(version2);
        StringProperty2 = "StringProperty2";
    }

    public VersionClass()
    {
    }

    public string StringProperty1 { get; set; }
    public Version Version1 { get; set; }
    public Version Version2 { get; set; }
    public string StringProperty2 { get; set; }
}

public class VersionConverterTests : TestFixtureBase
{
    internal static class VersionHelperClass
    {
        internal static void SerializeVersionClass(string version1, string version2)
        {
            var versionClass = new VersionClass(version1, version2);

            var json = JsonConvert.SerializeObject(versionClass, Formatting.Indented, new VersionConverter());

            var expectedJson = string.Format(@"{{
  ""StringProperty1"": ""StringProperty1"",
  ""Version1"": ""{0}"",
  ""Version2"": ""{1}"",
  ""StringProperty2"": ""StringProperty2""
}}", version1, version2);

            StringAssert.AreEqual(expectedJson, json);
        }

        internal static void DeserializeVersionClass(string version1, string version2)
        {
            var json = string.Format(@"{{""StringProperty1"": ""StringProperty1"", ""Version1"": ""{0}"", ""Version2"": ""{1}"", ""StringProperty2"": ""StringProperty2""}}", version1, version2);
            var expectedVersion1 = new Version(version1);
            var expectedVersion2 = new Version(version2);

            var versionClass = JsonConvert.DeserializeObject<VersionClass>(json, new VersionConverter());

            Xunit.Assert.Equal("StringProperty1", versionClass.StringProperty1);
            Xunit.Assert.Equal(expectedVersion1, versionClass.Version1);
            Xunit.Assert.Equal(expectedVersion2, versionClass.Version2);
            Xunit.Assert.Equal("StringProperty2", versionClass.StringProperty2);
        }
    }

    [Fact]
    public void WriteJsonNull()
    {
        var sw = new StringWriter();
        var jsonWriter = new JsonTextWriter(sw);

        var converter = new VersionConverter();
        converter.WriteJson(jsonWriter, null, null);

        StringAssert.AreEqual(@"null", sw.ToString());
    }

    [Fact]
    public void SerializeVersionClass()
    {
        VersionHelperClass.SerializeVersionClass("1.0.0.0", "2.0.0.0");
        VersionHelperClass.SerializeVersionClass("1.2.0.0", "2.3.0.0");
        VersionHelperClass.SerializeVersionClass("1.2.3.0", "2.3.4.0");
        VersionHelperClass.SerializeVersionClass("1.2.3.4", "2.3.4.5");

        VersionHelperClass.SerializeVersionClass("1.2", "2.3");
        VersionHelperClass.SerializeVersionClass("1.2.3", "2.3.4");
        VersionHelperClass.SerializeVersionClass("1.2.3.4", "2.3.4.5");
    }

    [Fact]
    public void DeserializeVersionClass()
    {
        VersionHelperClass.DeserializeVersionClass("1.0.0.0", "2.0.0.0");
        VersionHelperClass.DeserializeVersionClass("1.2.0.0", "2.3.0.0");
        VersionHelperClass.DeserializeVersionClass("1.2.3.0", "2.3.4.0");
        VersionHelperClass.DeserializeVersionClass("1.2.3.4", "2.3.4.5");

        VersionHelperClass.DeserializeVersionClass("1.2", "2.3");
        VersionHelperClass.DeserializeVersionClass("1.2.3", "2.3.4");
        VersionHelperClass.DeserializeVersionClass("1.2.3.4", "2.3.4.5");
    }

    [Fact]
    public void RoundtripImplicitConverter()
    {
        var version = new Version(1, 0, 0, 0);
        var reportJSON = JsonConvert.SerializeObject(version);

        //Test
        var report2 = JsonConvert.DeserializeObject<Version>(reportJSON);
        var reportJSON2 = JsonConvert.SerializeObject(report2);

        Xunit.Assert.Equal(reportJSON, reportJSON2);
    }
}