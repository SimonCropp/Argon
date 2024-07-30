# Serialization using ContractResolver

The `Argon.IContractResolver` interface provides a way to customize how the JsonSerializer serializes and deserializes .NET objects to JSON without placing attributes on classes.

Anything that can be set on an object, collection, property, etc, using attributes or methods to control serialization can also be set using an IContractResolver.

For performance create a contract resolver once and reuse instances when possible. Resolving contracts is slow and implementations of `Argon.IContractResolver` typically cache contracts.


## DefaultContractResolver

The `Argon.DefaultContractResolver` is the default resolver used by the serializer. It provides many avenues of extensibility in the form of virtual methods that can be overridden.


## CamelCasePropertyNamesContractResolver

`Argon.CamelCasePropertyNamesContractResolver` inherits from DefaultContractResolver and overrides the JSON property name to be written in [camelcase](http://en.wikipedia.org/wiki/CamelCase).

<!-- snippet: ContractResolver -->
<a id='snippet-ContractResolver'></a>
```cs
var product = new Product
{
    ExpiryDate = new(2010, 12, 20, 18, 1, 0, DateTimeKind.Utc),
    Name = "Widget",
    Price = 9.99m,
    Sizes = ["Small", "Medium", "Large"]
};

var json =
    JsonConvert.SerializeObject(
        product,
        Formatting.Indented,
        new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()}
    );

//{
//  "name": "Widget",
//  "expiryDate": "2010-12-20T18:01:00Z",
//  "price": 9.99,
//  "sizes": [
//    "Small",
//    "Medium",
//    "Large"
//  ]
//}
```
<sup><a href='/src/ArgonTests/Documentation/SerializationTests.cs#L457-L485' title='Snippet source file'>snippet source</a> | <a href='#snippet-ContractResolver' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Custom IContractResolver Examples

TODO:
```cs
public class ConverterContractResolver : DefaultContractResolver
{
    public new static readonly ConverterContractResolver Instance = new();

    protected override JsonContract CreateContract(Type type)
    {
        var contract = base.CreateContract(type);

        // this will only be called once and then cached
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            contract.Converter = new JavaScriptDateTimeConverter();
        }

        return contract;
    }
}
```
