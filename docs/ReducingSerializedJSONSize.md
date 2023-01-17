# Reducing JSON size

One of the common problems encountered when serializing .NET objects to JSON is that the JSON ends up containing a lot of unwanted properties and values. This can be especially significant when returning JSON to the client. More JSON means more bandwidth and a slower website.

To solve the issue of unwanted JSON, Json.NET has a range of built-in options to fine-tune what gets written from a serialized object.


## JsonIgnoreAttribute and DataMemberAttribute

By default Json.NET will include all of a class's public properties and fields in the JSON it creates. Adding the `Argon.JsonIgnoreAttribute` to a property tells the serializer to always skip writing it to the JSON result.

<!-- snippet: ReducingSerializedJsonSizeOptOut -->
<a id='snippet-reducingserializedjsonsizeoptout'></a>
```cs
public class Car
{
    // included in JSON
    public string Model { get; set; }
    public DateTime Year { get; set; }
    public List<string> Features { get; set; }

    // ignored
    [JsonIgnore] public DateTime LastModified { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L697-L710' title='Snippet source file'>snippet source</a> | <a href='#snippet-reducingserializedjsonsizeoptout' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If a class has many properties and you only want to serialize a small subset of them, then adding JsonIgnore to all the others will be tedious and error prone. The way to tackle this scenario is to add the `System.Runtime.Serialization.DataContractAttribute` to the class and `System.Runtime.Serialization.DataMemberAttribute` to the properties to serialize. This is opt-in serialization - only the properties you mark up will be serialized, unlike opt-out serialization using JsonIgnoreAttribute.

<!-- snippet: ReducingSerializedJsonSizeOptIn -->
<a id='snippet-reducingserializedjsonsizeoptin'></a>
```cs
[DataContract]
public class Computer
{
    // included in JSON
    [DataMember] public string Name { get; set; }

    [DataMember] public decimal SalePrice { get; set; }

    // ignored
    public string Manufacture { get; set; }
    public int StockCount { get; set; }
    public decimal WholeSalePrice { get; set; }
    public DateTime NextShipmentDate { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L712-L729' title='Snippet source file'>snippet source</a> | <a href='#snippet-reducingserializedjsonsizeoptin' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Formatting

JSON written by the serializer with an option of `Argon.Formatting` set to Indented produces nicely formatted, easy-to-read JSON that is great for readability when you are developing. `Formatting.None` on the other hand keeps the JSON result small, skipping all unnecessary spaces and line breaks to produce the most compact and efficient JSON possible.


## NullValueHandling

`Argon.NullValueHandling` is an option on the JsonSerializer and controls how the serializer handles properties with a null value. By setting a value of NullValueHandling.Ignore the JsonSerializer skips writing any properties that have a value of null.

<!-- snippet: ReducingSerializedJsonSizeNullValueHandlingObject -->
<a id='snippet-reducingserializedjsonsizenullvaluehandlingobject'></a>
```cs
public class Movie
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Classification { get; set; }
    public string Studio { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public List<string> ReleaseCountries { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L731-L743' title='Snippet source file'>snippet source</a> | <a href='#snippet-reducingserializedjsonsizenullvaluehandlingobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: ReducingSerializedJsonSizeNullValueHandlingExample -->
<a id='snippet-reducingserializedjsonsizenullvaluehandlingexample'></a>
```cs
var movie = new Movie
{
    Name = "Bad Boys III",
    Description = "It's no Bad Boys"
};

var included = JsonConvert.SerializeObject(movie,
    Formatting.Indented,
    new JsonSerializerSettings());

// {
//   "Name": "Bad Boys III",
//   "Description": "It's no Bad Boys",
//   "Classification": null,
//   "Studio": null,
//   "ReleaseDate": null,
//   "ReleaseCountries": null
// }

var ignored = JsonConvert.SerializeObject(movie,
    Formatting.Indented,
    new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

// {
//   "Name": "Bad Boys III",
//   "Description": "It's no Bad Boys"
// }
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L748-L778' title='Snippet source file'>snippet source</a> | <a href='#snippet-reducingserializedjsonsizenullvaluehandlingexample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

NullValueHandling can also be customized on individual properties using the `Argon.JsonPropertyAttribute`. The JsonPropertyAttribute value of NullValueHandling will override the setting on the JsonSerializer for that property.


## DefaultValueHandling

`Argon.DefaultValueHandling` is an option on the JsonSerializer and controls how the serializer handles properties with a default value. Setting a value of DefaultValueHandling.Ignore will make the JsonSerializer skip writing any properties that have a default value to the JSON result. For object references this will be null. For value types like int and DateTime the serializer will skip the default uninitialized value for that value type.

Json.NET also allows you to customize what the default value of an individual property is using the `System.ComponentModel.DefaultValueAttribute`. For example, if a string property called Department always returns an empty string in its default state and you don't want that empty string in your JSON, then placing the DefaultValueAttribute on Department with that value will mean Department is no longer written to JSON unless it has a value.

<!-- snippet: ReducingSerializedJsonSizeDefaultValueHandlingObject -->
<a id='snippet-reducingserializedjsonsizedefaultvaluehandlingobject'></a>
```cs
public class Invoice
{
    public string Company { get; set; }
    public decimal Amount { get; set; }

    // false is default value of bool
    public bool Paid { get; set; }

    // null is default value of nullable
    public DateTime? PaidDate { get; set; }

    // customize default values
    [DefaultValue(30)] public int FollowUpDays { get; set; }

    [DefaultValue("")] public string FollowUpEmailAddress { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L795-L814' title='Snippet source file'>snippet source</a> | <a href='#snippet-reducingserializedjsonsizedefaultvaluehandlingobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: ReducingSerializedJsonSizeDefaultValueHandlingExample -->
<a id='snippet-reducingserializedjsonsizedefaultvaluehandlingexample'></a>
```cs
var invoice = new Invoice
{
    Company = "Acme Ltd.",
    Amount = 50.0m,
    Paid = false,
    FollowUpDays = 30,
    FollowUpEmailAddress = string.Empty,
    PaidDate = null
};

var included = JsonConvert.SerializeObject(invoice,
    Formatting.Indented,
    new JsonSerializerSettings());

// {
//   "Company": "Acme Ltd.",
//   "Amount": 50.0,
//   "Paid": false,
//   "PaidDate": null,
//   "FollowUpDays": 30,
//   "FollowUpEmailAddress": ""
// }

var ignored = JsonConvert.SerializeObject(invoice,
    Formatting.Indented,
    new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Ignore});

// {
//   "Company": "Acme Ltd.",
//   "Amount": 50.0
// }
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L819-L853' title='Snippet source file'>snippet source</a> | <a href='#snippet-reducingserializedjsonsizedefaultvaluehandlingexample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

DefaultValueHandling can also be customized on individual properties using the `Argon.JsonPropertyAttribute`. The JsonPropertyAttribute value of DefaultValueHandling will override the setting on the JsonSerializer for that property.


## IContractResolver

For more flexibility, the `Argon.IContractResolver` provides an interface to customize almost every aspect of how a .NET object gets serialized to JSON, including changing serialization behavior at runtime.

<!-- snippet: ReducingSerializedJsonSizeContractResolverObject -->
<a id='snippet-reducingserializedjsonsizecontractresolverobject'></a>
```cs
public class DynamicContractResolver : DefaultContractResolver
{
    readonly char _startingWithChar;

    public DynamicContractResolver(char startingWithChar) =>
        _startingWithChar = startingWithChar;

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization serialization) =>
        // only serializer properties that start with the specified character
        base.CreateProperties(type, serialization)
            .Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();
}

public class Book
{
    public string BookName { get; set; }
    public decimal BookPrice { get; set; }
    public string AuthorName { get; set; }
    public int AuthorAge { get; set; }
    public string AuthorCountry { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L874-L898' title='Snippet source file'>snippet source</a> | <a href='#snippet-reducingserializedjsonsizecontractresolverobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: ReducingSerializedJsonSizeContractResolverExample -->
<a id='snippet-reducingserializedjsonsizecontractresolverexample'></a>
```cs
var book = new Book
{
    BookName = "The Gathering Storm",
    BookPrice = 16.19m,
    AuthorName = "Brandon Sanderson",
    AuthorAge = 34,
    AuthorCountry = "United States of America"
};

var startingWithA = JsonConvert.SerializeObject(book, Formatting.Indented,
    new JsonSerializerSettings {ContractResolver = new DynamicContractResolver('A')});

// {
//   "AuthorName": "Brandon Sanderson",
//   "AuthorAge": 34,
//   "AuthorCountry": "United States of America"
// }

var startingWithB = JsonConvert.SerializeObject(book, Formatting.Indented,
    new JsonSerializerSettings {ContractResolver = new DynamicContractResolver('B')});

// {
//   "BookName": "The Gathering Storm",
//   "BookPrice": 16.19
// }
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L903-L931' title='Snippet source file'>snippet source</a> | <a href='#snippet-reducingserializedjsonsizecontractresolverexample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * `Argon.Formatting`
 * `Argon.JsonIgnoreAttribute`
 * `Argon.DefaultValueHandling`
 * `Argon.NullValueHandling`
