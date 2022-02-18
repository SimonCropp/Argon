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

namespace Argon.Tests.Documentation.Samples.Serializer;

public class SerializeWithJsonSerializerToFile : TestFixtureBase
{
    #region Types
    public class Movie
    {
        public string Name { get; set; }
        public int Year { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region Usage
        var movie = new Movie
        {
            Name = "Bad Boys",
            Year = 1995
        };

        // serialize JSON to a string and then write string to a file
        File.WriteAllText(@"c:\movie.json", JsonConvert.SerializeObject(movie));

        // serialize JSON directly to a file
        using (var file = File.CreateText(@"c:\movie.json"))
        {
            var serializer = new JsonSerializer();
            serializer.Serialize(file, movie);
        }
        #endregion
    }

    public static class File
    {
        public static StreamWriter CreateText(string path)
        {
            return new StreamWriter(new MemoryStream());
        }

        public static void WriteAllText(string s1, string s2)
        {
        }
    }
}