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

public class Snippets
{
    [Fact]
    public void LinqToJson()
    {
        #region LinqToJson

        var jArray = new JArray
        {
            "Manual text",
            new DateTime(2000, 5, 23)
        };

        var jObject = new JObject
        {
            ["MyArray"] = jArray
        };

        var json = jObject.ToString();
// {
//   "MyArray": [
//     "Manual text",
//     "2000-05-23T00:00:00"
//   ]
// }

        #endregion
    }

    [Fact]
    public void SerializeJson()
    {
        #region SerializeJson

        var product = new Product
        {
            Name = "Apple",
            Expiry = new DateTime(2008, 12, 28),
            Sizes = new string[] { "Small" }
        };

        var json = JsonConvert.SerializeObject(product);
// {
//   "Name": "Apple",
//   "Expiry": "2008-12-28T00:00:00",
//   "Sizes": [
//     "Small"
//   ]
// }
        #endregion
    }

    public class Product
    {
        public string Name { get; set; }
        public DateTime Expiry { get; set; }
        public string[] Sizes { get; set; }
    }

    [Fact]
    public void DeserializeJson()
    {
        #region DeserializeJson

        var json = @"{
  'Name': 'Bad Boys',
  'ReleaseDate': '1995-4-7T00:00:00',
  'Genres': [
    'Action',
    'Comedy'
  ]
}";

        var movie = JsonConvert.DeserializeObject<Movie>(json);

        var name = movie.Name;
// Bad Boys

        #endregion
    }

    class Movie
    {
        public string Name { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<string> Genres { get; set; }
    }
}