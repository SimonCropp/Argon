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

namespace Argon.Tests.Linq;

[TestFixture]
public class JPropertyAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task LoadAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader("{'propertyname':['value1']}"));
        await reader.ReadAsync();

        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
        await reader.ReadAsync();

        var property = await JProperty.LoadAsync(reader);
        Assert.AreEqual("propertyname", property.Name);
        Assert.IsTrue(JToken.DeepEquals(JArray.Parse("['value1']"), property.Value));

        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

        reader = new JsonTextReader(new StringReader("{'propertyname':null}"));
        await reader.ReadAsync();

        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
        await reader.ReadAsync();

        property = await JProperty.LoadAsync(reader);
        Assert.AreEqual("propertyname", property.Name);
        Assert.IsTrue(JToken.DeepEquals(JValue.CreateNull(), property.Value));

        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }
}