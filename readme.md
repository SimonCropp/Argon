# <img src='/src/icon.png' height='30px'> Argon

[![Build status](https://ci.appveyor.com/api/projects/status/t9tj73533brq9in3/branch/main?svg=true)](https://ci.appveyor.com/project/SimonCropp/Argon)
[![NuGet Status](https://img.shields.io/nuget/v/Argon.svg?label=Argon)](https://www.nuget.org/packages/Argon/)


Argon is a JSON framework for .NET. It is a hard fork of [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json).


## Serialize JSON

<!-- snippet: SerializeJson -->
<a id='snippet-serializejson'></a>
```cs
var product = new Product
{
    Name = "Apple",
    Expiry = new(2008, 12, 28),
    Sizes = new[] {"Small"}
};

var json = JsonConvert.SerializeObject(product);
// {
//   "Name": "Apple",
//   "Expiry": "2008-12-28T00:00:00",
//   "Sizes": [
//     "Small"
//   ]
// }
```
<sup><a href='/src/Tests/Documentation/Snippets.cs#L37-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializejson' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Deserialize JSON

<!-- snippet: DeserializeJson -->
<a id='snippet-deserializejson'></a>
```cs
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
```
<sup><a href='/src/Tests/Documentation/Snippets.cs#L68-L84' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializejson' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## LINQ to JSON
<!-- snippet: LinqToJson -->
<a id='snippet-linqtojson'></a>
```cs
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
```
<sup><a href='/src/Tests/Documentation/Snippets.cs#L10-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojson' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

include: index

## Icon

[Helmet](https://thenounproject.com/term/helmet/1681772/) designed by [Juan Manuel Corredor](https://thenounproject.com/juan_corredor/) from [The Noun Project](https://thenounproject.com).
