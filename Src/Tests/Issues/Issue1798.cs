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

namespace Argon.Tests.Issues;

public class Issue1798
{
    public class NonSerializableException : Exception
    {
    }

    [Fact]
    public void Test()
    {
        string nonSerializableJson = null;
        string serializableJson = null;

        try
        {
            throw new NonSerializableException();
        }
        catch (Exception ex)
        {
            nonSerializableJson = JsonConvert.SerializeObject(ex, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
        }

        try
        {
            throw new Exception();
        }
        catch (Exception ex)
        {
            serializableJson = JsonConvert.SerializeObject(ex, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
        }

        AssertNoTargetSite(nonSerializableJson);
        AssertNoTargetSite(serializableJson);
    }

    [Fact]
    public void Test_DefaultContractResolver()
    {
        var resolver = new DefaultContractResolver();

        var objectContract = (JsonObjectContract) resolver.ResolveContract(typeof(NonSerializableException));
        Xunit.Assert.False(objectContract.Properties.Contains("TargetSite"));

        object o = resolver.ResolveContract(typeof(Exception));
        Xunit.Assert.IsType(typeof(JsonISerializableContract), o);
    }

    void AssertNoTargetSite(string json)
    {
        var o = JObject.Parse(json);

        if (o.ContainsKey("TargetSite"))
        {
            XUnitAssert.Fail("JSON has TargetSite property.");
        }
    }
}