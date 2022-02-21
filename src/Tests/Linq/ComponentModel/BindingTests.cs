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

#if !NET5_0_OR_GREATER
using System.Web.UI;

namespace Argon.Tests.Linq.ComponentModel;

public class BindingTests : TestFixtureBase
{
    [Fact]
    public void DataBinderEval()
    {
        var o = new JObject(
            new JProperty("First", "String!"),
            new JProperty("Second", 12345.6789m),
            new JProperty("Third", new JArray(
                1,
                2,
                3,
                4,
                5,
                new JObject(
                    new JProperty("Fourth", "String!"),
                    new JProperty("Fifth", new JObject(
                        new JProperty("Sixth", "String!")))))));

        object value = (string) DataBinder.Eval(o, "First.Value");
        Assert.Equal(value, (string) o["First"]);

        value = DataBinder.Eval(o, "Second.Value");
        Assert.Equal(value, (decimal) o["Second"]);

        value = DataBinder.Eval(o, "Third");
        Assert.Equal(value, o["Third"]);

        value = DataBinder.Eval(o, "Third[0].Value");
        Assert.Equal((int) value, (int) o["Third"][0]);

        value = DataBinder.Eval(o, "Third[5].Fourth.Value");
        Assert.Equal(value, (string) o["Third"][5]["Fourth"]);

        value = DataBinder.Eval(o, "Third[5].Fifth.Sixth.Value");
        Assert.Equal(value, (string) o["Third"][5]["Fifth"]["Sixth"]);
    }
}
#endif