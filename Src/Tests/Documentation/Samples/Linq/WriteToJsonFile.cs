﻿#region License
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

namespace Argon.Tests.Documentation.Samples.Linq;

public class WriteToJsonFile : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region Usage
        var videogameRatings = new JObject(
            new JProperty("Halo", 9),
            new JProperty("Starcraft", 9),
            new JProperty("Call of Duty", 7.5));

        File.WriteAllText(@"c:\videogames.json", videogameRatings.ToString());

        // write JSON directly to a file
        using (var file = File.CreateText(@"c:\videogames.json"))
        using (var writer = new JsonTextWriter(file))
        {
            videogameRatings.WriteTo(writer);
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