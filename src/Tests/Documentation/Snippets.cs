// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

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
            Expiry = new(2008, 12, 28),
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