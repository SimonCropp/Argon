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

namespace Argon.Tests.Documentation.Samples.Serializer;

public class CustomJsonConverterGeneric : TestFixtureBase
{
    #region CustomJsonConverterGenericTypes
    public class VersionConverter : JsonConverter<Version>
    {
        public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override Version ReadJson(JsonReader reader, Type type, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = (string)reader.Value;

            return new Version(s);
        }
    }

    public class NuGetPackage
    {
        public string PackageId { get; set; }
        public Version Version { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region CustomJsonConverterGenericUsage
        var p1 = new NuGetPackage
        {
            PackageId = "Argon",
            Version = new Version(10, 0, 4)
        };

        var json = JsonConvert.SerializeObject(p1, Formatting.Indented, new VersionConverter());

        Console.WriteLine(json);
        // {
        //   "PackageId": "Argon",
        //   "Version": "10.0.4"
        // }

        var p2 = JsonConvert.DeserializeObject<NuGetPackage>(json, new VersionConverter());

        Console.WriteLine(p2.Version.ToString());
        // 10.0.4
        #endregion

        Assert.Equal("10.0.4", p2.Version.ToString());
    }
}