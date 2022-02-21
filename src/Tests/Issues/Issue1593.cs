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

using System.Dynamic;

namespace Argon.Tests.Issues;

public class Issue1593 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var json = JsonConvert.SerializeObject(CreateModel());
        Assert.Equal(@"{""Specific"":2,""A"":1}", json);
    }

    class BaseModel
    {
        public BaseModel()
        {
            Extra = new ExpandoObject();
        }
        [JsonExtensionData]
        public ExpandoObject Extra { get; set; }
    }

    class SpecificModel : BaseModel
    {
        public int Specific { get; set; }
    }

    static BaseModel CreateModel()
    {
        var model = new SpecificModel();
        var extra = model.Extra as IDictionary<string, object>;
        extra["A"] = 1;
        model.Specific = 2;
        return model;
    }
}