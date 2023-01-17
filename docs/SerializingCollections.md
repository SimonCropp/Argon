## Serializing Collections


## Serializing Collections

To serialize a collection - a generic list, array, dictionary, or your own custom collection - simply call the serializer with the object you want to get JSON for. Json.NET will serialize the collection and all of the values it contains.

<!-- snippet: SerializingCollectionsSerializing -->
<a id='snippet-serializingcollectionsserializing'></a>
```cs
var p1 = new Product
{
    Name = "Product 1",
    Price = 99.95m,
    ExpiryDate = new(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc)
};
var p2 = new Product
{
    Name = "Product 2",
    Price = 12.50m,
    ExpiryDate = new(2009, 7, 31, 0, 0, 0, DateTimeKind.Utc)
};

var products = new List<Product>
{
    p1,
    p2
};

var json = JsonConvert.SerializeObject(products, Formatting.Indented);
//[
//  {
//    "Name": "Product 1",
//    "ExpiryDate": "2000-12-29T00:00:00Z",
//    "Price": 99.95,
//    "Sizes": null
//  },
//  {
//    "Name": "Product 2",
//    "ExpiryDate": "2009-07-31T00:00:00Z",
//    "Price": 12.50,
//    "Sizes": null
//  }
//]
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L565-L602' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializingcollectionsserializing' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Deserializing Collections

To deserialize JSON into a .NET collection, just specify the collection type you want to deserialize to. Json.NET supports a wide range of collection types.

<!-- snippet: SerializingCollectionsDeserializing -->
<a id='snippet-serializingcollectionsdeserializing'></a>
```cs
var json = @"[
      {
        'Name': 'Product 1',
        'ExpiryDate': '2000-12-29T00:00Z',
        'Price': 99.95,
        'Sizes': null
      },
      {
        'Name': 'Product 2',
        'ExpiryDate': '2009-07-31T00:00Z',
        'Price': 12.50,
        'Sizes': null
      }
    ]";

var products = JsonConvert.DeserializeObject<List<Product>>(json);

Console.WriteLine(products.Count);
// 2

var p1 = products[0];

Console.WriteLine(p1.Name);
// Product 1
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L625-L652' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializingcollectionsdeserializing' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Deserializing Dictionaries

Using Json.NET you can also deserialize a JSON object into a .NET generic dictionary. The JSON object's property names and values will be added to the dictionary.

<!-- snippet: SerializingCollectionsDeserializingDictionaries -->
<a id='snippet-serializingcollectionsdeserializingdictionaries'></a>
```cs
var json = @"{""key1"":""value1"",""key2"":""value2""}";

var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

Console.WriteLine(values.Count);
// 2

Console.WriteLine(values["key1"]);
// value1
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L660-L672' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializingcollectionsdeserializingdictionaries' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * SerializationGuide
 * `Argon.JsonConvert`
 * `Argon.JsonSerializer`
