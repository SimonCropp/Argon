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

namespace Argon.Tests.Documentation.Samples.Linq;

public class ToObjectComplex : TestFixtureBase
{
    #region Types
    public class Person
    {
        public string Name { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region ToObjectComplex
        var json = @"{
              'd': [
                {
                  'Name': 'John Smith'
                },
                {
                  'Name': 'Mike Smith'
                }
              ]
            }";

        var o = JObject.Parse(json);

        var a = (JArray)o["d"];

        var person = a.ToObject<IList<Person>>();

        Console.WriteLine(person[0].Name);
        // John Smith

        Console.WriteLine(person[1].Name);
        // Mike Smith
        #endregion

        Assert.Equal("John Smith", person[0].Name);
        Assert.Equal("Mike Smith", person[1].Name);
    }
}